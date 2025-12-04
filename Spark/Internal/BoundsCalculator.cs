using System.Collections.Generic;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Result of bounds calculation for effect placement.
    /// </summary>
    public class SparkBounds
    {
        /// <summary>Local-space center of the object.</summary>
        public Vector3 Center { get; set; } = Vector3.zero;

        /// <summary>Size along each axis.</summary>
        public Vector3 Size { get; set; } = Vector3.one;

        /// <summary>The longest dimension length.</summary>
        public float Length => Mathf.Max(Size.x, Size.y, Size.z);

        /// <summary>The shortest dimension (thickness).</summary>
        public float Thickness => Mathf.Min(Size.x, Size.y, Size.z);

        /// <summary>Primary axis (0=X, 1=Y, 2=Z) - the longest dimension.</summary>
        public int LengthAxis { get; set; } = 1;

        /// <summary>Secondary axis - second longest dimension.</summary>
        public int SecondaryAxis { get; set; } = 0;

        /// <summary>Tertiary axis - shortest dimension.</summary>
        public int TertiaryAxis { get; set; } = 2;

        /// <summary>Local-space min point of bounds.</summary>
        public Vector3 Min { get; set; } = Vector3.zero;

        /// <summary>Local-space max point of bounds.</summary>
        public Vector3 Max { get; set; } = Vector3.zero;

        /// <summary>Type of object detected.</summary>
        public BoundsTargetType TargetType { get; set; } = BoundsTargetType.Unknown;

        /// <summary>Whether bounds were successfully calculated.</summary>
        public bool IsValid { get; set; }

        /// <summary>Direction along the length axis in local space.</summary>
        public Vector3 LengthDirection
        {
            get
            {
                return LengthAxis switch
                {
                    0 => Vector3.right,
                    2 => Vector3.forward,
                    _ => Vector3.up
                };
            }
        }

        /// <summary>
        /// Gets shape scale suitable for particle system box emitter.
        /// </summary>
        /// <param name="thickness">Override thickness (default uses calculated).</param>
        public Vector3 GetParticleShapeScale(float? thickness = null)
        {
            float t = thickness ?? Mathf.Max(0.05f, Thickness * 0.5f);
            return LengthAxis switch
            {
                0 => new Vector3(Length, t, t),
                2 => new Vector3(t, t, Length),
                _ => new Vector3(t, Length, t)
            };
        }

        /// <summary>
        /// Gets an offset position along the length axis.
        /// </summary>
        /// <param name="t">Normalized position (-0.5 to 0.5 for full length).</param>
        public Vector3 GetPositionAlongLength(float t)
        {
            return Center + LengthDirection * (Length * t);
        }
    }

    /// <summary>
    /// Type of target detected by bounds calculator.
    /// </summary>
    public enum BoundsTargetType
    {
        Unknown,
        Weapon,         // Swords, axes, maces, etc.
        Tool,           // Pickaxe, hammer, hoe
        Shield,         // Shields (flat, wide)
        Bow,            // Bows (curved, long)
        Armor,          // Armor pieces
        Helmet,         // Head gear
        Cape,           // Capes/cloaks
        Character,      // Player or creature
        Creature,       // NPC/enemy
        Item,           // Dropped item in world
        Piece,          // Building piece
        Destructible,   // Trees, rocks, etc.
        Vehicle,        // Cart, boat, etc.
        Custom          // User-defined
    }

    /// <summary>
    /// Universal bounds calculator for effect placement on any GameObject.
    /// Analyzes mesh geometry to determine optimal effect positioning.
    /// </summary>
    public static class BoundsCalculator
    {
        /// <summary>
        /// Calculate bounds for any GameObject, auto-detecting type.
        /// </summary>
        public static SparkBounds Calculate(GameObject target)
        {
            if (target == null)
                return new SparkBounds { IsValid = false };

            var bounds = new SparkBounds();

            // Detect target type
            bounds.TargetType = DetectTargetType(target);

            // Calculate geometric bounds
            if (!CalculateGeometricBounds(target, bounds))
            {
                // Fallback to collider bounds
                if (!CalculateColliderBounds(target, bounds))
                {
                    // Last resort: use renderer bounds
                    if (!CalculateRendererBounds(target, bounds))
                    {
                        // Absolute fallback
                        bounds.Size = Vector3.one;
                        bounds.Center = Vector3.zero;
                        bounds.LengthAxis = 1;
                        bounds.IsValid = false;
                        return bounds;
                    }
                }
            }

            // Apply type-specific adjustments
            AdjustForTargetType(bounds);

            bounds.IsValid = true;
            return bounds;
        }

        /// <summary>
        /// Calculate bounds for a specific transform within a hierarchy.
        /// Useful for weapons attached to characters.
        /// </summary>
        public static SparkBounds Calculate(GameObject target, Transform relativeTo)
        {
            if (target == null)
                return new SparkBounds { IsValid = false };

            var bounds = Calculate(target);

            // Transform bounds to be relative to the specified transform
            if (relativeTo != null && bounds.IsValid)
            {
                // Convert center from target local space to relativeTo local space
                Vector3 worldCenter = target.transform.TransformPoint(bounds.Center);
                bounds.Center = relativeTo.InverseTransformPoint(worldCenter);
            }

            return bounds;
        }

        /// <summary>
        /// Calculate bounds specifically for a weapon visual.
        /// </summary>
        public static SparkBounds CalculateWeapon(GameObject weaponVisual)
        {
            var bounds = Calculate(weaponVisual);
            bounds.TargetType = BoundsTargetType.Weapon;

            // Ensure minimum weapon length
            if (bounds.Length < 0.3f)
            {
                bounds.Size = new Vector3(
                    bounds.LengthAxis == 0 ? 1.2f : bounds.Size.x,
                    bounds.LengthAxis == 1 ? 1.2f : bounds.Size.y,
                    bounds.LengthAxis == 2 ? 1.2f : bounds.Size.z
                );
            }

            return bounds;
        }

        /// <summary>
        /// Calculate bounds for a character (player or creature).
        /// </summary>
        public static SparkBounds CalculateCharacter(Character character)
        {
            if (character == null)
                return new SparkBounds { IsValid = false };

            var bounds = new SparkBounds
            {
                TargetType = character is Player ? BoundsTargetType.Character : BoundsTargetType.Creature
            };

            // Use character collider for accurate bounds
            var collider = character.GetComponent<CapsuleCollider>();
            if (collider != null)
            {
                float height = collider.height;
                float radius = collider.radius;

                bounds.Size = new Vector3(radius * 2, height, radius * 2);
                bounds.Center = collider.center;
                bounds.LengthAxis = 1; // Characters are vertical
                bounds.Min = bounds.Center - bounds.Size * 0.5f;
                bounds.Max = bounds.Center + bounds.Size * 0.5f;
                bounds.IsValid = true;
            }
            else
            {
                // Fallback to general calculation
                bounds = Calculate(character.gameObject);
                bounds.TargetType = character is Player ? BoundsTargetType.Character : BoundsTargetType.Creature;
            }

            return bounds;
        }

        /// <summary>
        /// Calculate bounds for a building piece.
        /// </summary>
        public static SparkBounds CalculatePiece(Piece piece)
        {
            if (piece == null)
                return new SparkBounds { IsValid = false };

            var bounds = Calculate(piece.gameObject);
            bounds.TargetType = BoundsTargetType.Piece;
            return bounds;
        }

        /// <summary>
        /// Calculate bounds for an item in the world.
        /// </summary>
        public static SparkBounds CalculateWorldItem(ItemDrop itemDrop)
        {
            if (itemDrop == null)
                return new SparkBounds { IsValid = false };

            var bounds = Calculate(itemDrop.gameObject);
            bounds.TargetType = BoundsTargetType.Item;
            return bounds;
        }

        #region Private Methods

        private static BoundsTargetType DetectTargetType(GameObject target)
        {
            string nameLower = target.name.ToLower();

            // Check components first
            if (target.GetComponent<Character>() != null)
                return target.GetComponent<Player>() != null ? BoundsTargetType.Character : BoundsTargetType.Creature;

            if (target.GetComponent<Piece>() != null)
                return BoundsTargetType.Piece;

            if (target.GetComponent<ItemDrop>() != null)
                return BoundsTargetType.Item;

            if (target.GetComponent<Destructible>() != null || target.GetComponent<TreeBase>() != null)
                return BoundsTargetType.Destructible;

            if (target.GetComponent<Ship>() != null || target.GetComponent<Vagon>() != null)
                return BoundsTargetType.Vehicle;

            // Name-based detection for equipment visuals
            if (nameLower.Contains("shield"))
                return BoundsTargetType.Shield;
            if (nameLower.Contains("bow"))
                return BoundsTargetType.Bow;
            if (nameLower.Contains("helmet") || nameLower.Contains("hood") || nameLower.Contains("hat"))
                return BoundsTargetType.Helmet;
            if (nameLower.Contains("cape") || nameLower.Contains("cloak"))
                return BoundsTargetType.Cape;
            if (nameLower.Contains("armor") || nameLower.Contains("chest") || nameLower.Contains("legs") ||
                nameLower.Contains("cuirass") || nameLower.Contains("greaves"))
                return BoundsTargetType.Armor;
            if (nameLower.Contains("pickaxe") || nameLower.Contains("hammer") || nameLower.Contains("hoe") ||
                nameLower.Contains("cultivator") || nameLower.Contains("fishing"))
                return BoundsTargetType.Tool;
            if (nameLower.Contains("sword") || nameLower.Contains("axe") || nameLower.Contains("mace") ||
                nameLower.Contains("knife") || nameLower.Contains("club") || nameLower.Contains("atgeir") ||
                nameLower.Contains("spear") || nameLower.Contains("sledge") || nameLower.Contains("battleaxe"))
                return BoundsTargetType.Weapon;

            return BoundsTargetType.Unknown;
        }

        private static bool CalculateGeometricBounds(GameObject target, SparkBounds bounds)
        {
            var meshFilters = target.GetComponentsInChildren<MeshFilter>(true);
            if (meshFilters.Length == 0)
                return false;

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;
            bool hasPoints = false;

            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;

                // Skip meshes that are likely not the main object
                string meshName = mf.name.ToLower();
                if (meshName.Contains("shadow") || meshName.Contains("lod") && !meshName.Contains("lod0"))
                    continue;

                var meshBounds = mf.sharedMesh.bounds;
                Vector3 min = meshBounds.min;
                Vector3 max = meshBounds.max;

                // Transform all 8 corners to target's local space
                Vector3[] corners = {
                    new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z),
                    new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z),
                    new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z),
                    new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z)
                };

                foreach (var corner in corners)
                {
                    Vector3 worldPos = mf.transform.TransformPoint(corner);
                    Vector3 localPos = target.transform.InverseTransformPoint(worldPos);

                    minX = Mathf.Min(minX, localPos.x);
                    maxX = Mathf.Max(maxX, localPos.x);
                    minY = Mathf.Min(minY, localPos.y);
                    maxY = Mathf.Max(maxY, localPos.y);
                    minZ = Mathf.Min(minZ, localPos.z);
                    maxZ = Mathf.Max(maxZ, localPos.z);
                    hasPoints = true;
                }
            }

            if (!hasPoints)
                return false;

            float sizeX = maxX - minX;
            float sizeY = maxY - minY;
            float sizeZ = maxZ - minZ;

            bounds.Min = new Vector3(minX, minY, minZ);
            bounds.Max = new Vector3(maxX, maxY, maxZ);
            bounds.Size = new Vector3(sizeX, sizeY, sizeZ);
            bounds.Center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, (minZ + maxZ) / 2f);

            // Determine axes by size
            DetermineAxes(bounds, sizeX, sizeY, sizeZ);

            return true;
        }

        private static bool CalculateColliderBounds(GameObject target, SparkBounds bounds)
        {
            var colliders = target.GetComponentsInChildren<Collider>(true);
            if (colliders.Length == 0)
                return false;

            Bounds combinedBounds = new Bounds();
            bool first = true;

            foreach (var collider in colliders)
            {
                if (collider.isTrigger) continue;

                Bounds colliderBounds = collider.bounds;

                // Transform to local space
                Vector3 localCenter = target.transform.InverseTransformPoint(colliderBounds.center);
                Vector3 localSize = colliderBounds.size; // Approximate - doesn't account for rotation

                if (first)
                {
                    combinedBounds = new Bounds(localCenter, localSize);
                    first = false;
                }
                else
                {
                    combinedBounds.Encapsulate(new Bounds(localCenter, localSize));
                }
            }

            if (first) return false;

            bounds.Center = combinedBounds.center;
            bounds.Size = combinedBounds.size;
            bounds.Min = combinedBounds.min;
            bounds.Max = combinedBounds.max;

            DetermineAxes(bounds, bounds.Size.x, bounds.Size.y, bounds.Size.z);

            return true;
        }

        private static bool CalculateRendererBounds(GameObject target, SparkBounds bounds)
        {
            var renderers = target.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return false;

            Bounds combinedBounds = new Bounds();
            bool first = true;

            foreach (var renderer in renderers)
            {
                if (!renderer.enabled) continue;

                Bounds rendererBounds = renderer.bounds;
                Vector3 localCenter = target.transform.InverseTransformPoint(rendererBounds.center);

                if (first)
                {
                    combinedBounds = new Bounds(localCenter, rendererBounds.size);
                    first = false;
                }
                else
                {
                    combinedBounds.Encapsulate(new Bounds(localCenter, rendererBounds.size));
                }
            }

            if (first) return false;

            bounds.Center = combinedBounds.center;
            bounds.Size = combinedBounds.size;
            bounds.Min = combinedBounds.min;
            bounds.Max = combinedBounds.max;

            DetermineAxes(bounds, bounds.Size.x, bounds.Size.y, bounds.Size.z);

            return true;
        }

        private static void DetermineAxes(SparkBounds bounds, float sizeX, float sizeY, float sizeZ)
        {
            // Sort axes by size
            var axes = new List<(int axis, float size)>
            {
                (0, sizeX),
                (1, sizeY),
                (2, sizeZ)
            };
            axes.Sort((a, b) => b.size.CompareTo(a.size));

            bounds.LengthAxis = axes[0].axis;
            bounds.SecondaryAxis = axes[1].axis;
            bounds.TertiaryAxis = axes[2].axis;
        }

        private static void AdjustForTargetType(SparkBounds bounds)
        {
            switch (bounds.TargetType)
            {
                case BoundsTargetType.Shield:
                    // Shields are flat - use the two larger dimensions as "surface"
                    // Particles should emit from the face, not the edge
                    break;

                case BoundsTargetType.Bow:
                    // Bows are curved - might need special handling
                    // For now, treat as weapon
                    break;

                case BoundsTargetType.Helmet:
                case BoundsTargetType.Armor:
                    // Armor wraps around body - center should be offset
                    break;

                case BoundsTargetType.Cape:
                    // Capes hang down - length axis should be Y (down)
                    bounds.LengthAxis = 1;
                    break;

                case BoundsTargetType.Character:
                case BoundsTargetType.Creature:
                    // Always vertical
                    bounds.LengthAxis = 1;
                    break;
            }
        }

        #endregion

        #region Helper Methods for Common Attach Points

        /// <summary>
        /// Get attachment points for a weapon (handle, blade tip, center).
        /// </summary>
        public static WeaponAttachPoints GetWeaponAttachPoints(SparkBounds bounds)
        {
            return new WeaponAttachPoints
            {
                Handle = bounds.GetPositionAlongLength(-0.4f),
                Center = bounds.Center,
                Tip = bounds.GetPositionAlongLength(0.4f),
                BladeStart = bounds.GetPositionAlongLength(-0.1f),
                BladeEnd = bounds.GetPositionAlongLength(0.45f)
            };
        }

        /// <summary>
        /// Get attachment points for a character (head, chest, hands, feet).
        /// </summary>
        public static CharacterAttachPoints GetCharacterAttachPoints(SparkBounds bounds)
        {
            float height = bounds.Size.y;
            return new CharacterAttachPoints
            {
                Head = bounds.Center + Vector3.up * (height * 0.4f),
                Chest = bounds.Center + Vector3.up * (height * 0.15f),
                Waist = bounds.Center,
                Feet = bounds.Center - Vector3.up * (height * 0.45f),
                LeftHand = bounds.Center + Vector3.left * bounds.Size.x * 0.4f + Vector3.up * height * 0.1f,
                RightHand = bounds.Center + Vector3.right * bounds.Size.x * 0.4f + Vector3.up * height * 0.1f
            };
        }

        #endregion
    }

    /// <summary>
    /// Common attachment points for weapons.
    /// </summary>
    public class WeaponAttachPoints
    {
        public Vector3 Handle { get; set; }
        public Vector3 Center { get; set; }
        public Vector3 Tip { get; set; }
        public Vector3 BladeStart { get; set; }
        public Vector3 BladeEnd { get; set; }
    }

    /// <summary>
    /// Common attachment points for characters.
    /// </summary>
    public class CharacterAttachPoints
    {
        public Vector3 Head { get; set; }
        public Vector3 Chest { get; set; }
        public Vector3 Waist { get; set; }
        public Vector3 Feet { get; set; }
        public Vector3 LeftHand { get; set; }
        public Vector3 RightHand { get; set; }
    }
}
