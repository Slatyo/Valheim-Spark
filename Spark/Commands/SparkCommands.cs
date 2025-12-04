using Munin;
using Spark.API;
using Spark.Core;
using Spark.Internal;
using UnityEngine;

namespace Spark.Commands
{
    /// <summary>
    /// Console commands for testing Spark effects.
    /// Registered with Munin: munin spark <command>
    /// </summary>
    internal static class SparkCommands
    {
        private static IEffectController _activeController;
        private static readonly System.Collections.Generic.List<IEffectController> _gearEffects = new System.Collections.Generic.List<IEffectController>();

        public static void Register()
        {
            Command.RegisterMany("spark",
                // Item-based effect (persists through holster)
                new CommandConfig
                {
                    Name = "item",
                    Description = "Attach effect to equipped weapon (persists through holster)",
                    Usage = "<element> [intensity]",
                    Permission = PermissionLevel.Admin,
                    Examples = new[] { "fire", "lightning 2.0" },
                    Handler = HandleItem
                },
                // Direct attach (debug, no persist)
                new CommandConfig
                {
                    Name = "attach",
                    Description = "Attach effect to weapon visual (debug, no persist)",
                    Usage = "<element> [intensity]",
                    Permission = PermissionLevel.Admin,
                    Examples = new[] { "fire", "lightning 1.5" },
                    Handler = HandleAttach
                },
                // Gear attachment
                new CommandConfig
                {
                    Name = "gear",
                    Description = "Attach effect to gear slot",
                    Usage = "<element> <slot> [intensity]",
                    Permission = PermissionLevel.Admin,
                    Examples = new[] { "fire chest", "lightning shield 0.5" },
                    Handler = HandleGear
                },
                // Impact effect
                new CommandConfig
                {
                    Name = "impact",
                    Description = "Spawn impact effect in front of player",
                    Usage = "[element]",
                    Permission = PermissionLevel.Admin,
                    Examples = new[] { "fire", "lightning" },
                    Handler = HandleImpact
                },
                // Explosion
                new CommandConfig
                {
                    Name = "explosion",
                    Description = "Spawn explosion in front of player",
                    Usage = "[element] [radius]",
                    Permission = PermissionLevel.Admin,
                    Examples = new[] { "fire", "lightning 10" },
                    Handler = HandleExplosion
                },
                // Lightning bolt
                new CommandConfig
                {
                    Name = "bolt",
                    Description = "Spawn lightning bolt from sky",
                    Permission = PermissionLevel.Admin,
                    Handler = HandleBolt
                },
                // Aura
                new CommandConfig
                {
                    Name = "aura",
                    Description = "Apply aura to nearest creature",
                    Usage = "<enraged|frozen|elite|boss>",
                    Permission = PermissionLevel.Admin,
                    Examples = new[] { "elite", "boss" },
                    Handler = HandleAura
                },
                // Status
                new CommandConfig
                {
                    Name = "status",
                    Description = "Apply status effect to player",
                    Usage = "<burning|freezing|poisoned|blessed|cursed>",
                    Permission = PermissionLevel.Admin,
                    Examples = new[] { "burning", "blessed" },
                    Handler = HandleStatus
                },
                // Bounds
                new CommandConfig
                {
                    Name = "bounds",
                    Description = "Show calculated bounds for equipped weapon",
                    Permission = PermissionLevel.Admin,
                    Handler = HandleBounds
                },
                // Textures
                new CommandConfig
                {
                    Name = "textures",
                    Description = "List all loaded particle textures",
                    Permission = PermissionLevel.Admin,
                    Handler = HandleTextures
                },
                // Clear
                new CommandConfig
                {
                    Name = "clear",
                    Description = "Remove all Spark effects from player and weapon",
                    Permission = PermissionLevel.Admin,
                    Handler = HandleClear
                }
            );

            Plugin.Log?.LogInfo("Registered Spark commands with Munin");
        }

        public static void Unregister()
        {
            Command.UnregisterMod("spark");
        }

        #region Helpers

        private static Element ParseElement(string name)
        {
            return name?.ToLower() switch
            {
                "fire" => Element.Fire,
                "frost" => Element.Frost,
                "ice" => Element.Frost,
                "lightning" => Element.Lightning,
                "electric" => Element.Lightning,
                "poison" => Element.Poison,
                "spirit" => Element.Spirit,
                "holy" => Element.Spirit,
                "shadow" => Element.Shadow,
                "dark" => Element.Shadow,
                "arcane" => Element.Arcane,
                "magic" => Element.Arcane,
                _ => Element.Fire
            };
        }

        private static T GetFieldValue<T>(object obj, string fieldName) where T : class
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            return field?.GetValue(obj) as T;
        }

        private static GameObject FindEquippedItem(Transform handTransform)
        {
            foreach (Transform child in handTransform)
            {
                if (child.gameObject.activeInHierarchy &&
                    (child.GetComponent<MeshFilter>() != null || child.GetComponent<SkinnedMeshRenderer>() != null))
                {
                    return child.gameObject;
                }
            }
            if (handTransform.childCount > 0)
                return handTransform.GetChild(0).gameObject;
            return handTransform.gameObject;
        }

        #endregion

        #region Handlers

        private static CommandResult HandleItem(CommandArgs args)
        {
            var player = args.Player;
            if (player == null)
                return CommandResult.Error("No player found");

            var weapon = player.GetCurrentWeapon();
            if (weapon == null)
                return CommandResult.Error("No weapon equipped");

            // Toggle off if already tracked
            if (SparkEffect.HasItemEffect(weapon))
            {
                SparkEffect.RemoveFromItem(weapon);
                return CommandResult.Success($"Removed tracked effect from {weapon.m_shared.m_name}");
            }

            Element element = args.Count > 0 ? ParseElement(args.Get(0)) : Element.Fire;
            float intensity = args.Get<float>(1, 1f);
            intensity = Mathf.Max(0.1f, intensity);

            SparkEffect.AttachToItem(weapon, element, intensity);
            return CommandResult.Success($"Attached {element} effect to {weapon.m_shared.m_name} (intensity: {intensity:F1})\nEffect will persist through holster/unholster!");
        }

        private static CommandResult HandleAttach(CommandArgs args)
        {
            var player = args.Player;
            if (player == null)
                return CommandResult.Error("No player found");

            // Toggle off if already active
            if (_activeController != null)
            {
                if (_activeController is MonoBehaviour mb)
                    Object.Destroy(mb.gameObject);
                _activeController = null;
                return CommandResult.Success("Removed effect");
            }

            Element element = args.Count > 0 ? ParseElement(args.Get(0)) : Element.Fire;
            float intensity = args.Get<float>(1, 1f);
            intensity = Mathf.Max(0.1f, intensity);

            // Try to attach to weapon
            var visual = player.GetComponentInChildren<VisEquipment>();
            if (visual != null && visual.m_rightHand != null)
            {
                _activeController = SparkEffect.AttachElemental(visual.m_rightHand.gameObject, element, intensity);

                if (_activeController != null)
                {
                    var bounds = SparkEffect.GetBounds(visual.m_rightHand.gameObject);
                    return CommandResult.Success($"Attached {element} effect (intensity: {intensity:F1}, length: {bounds.Length:F2})");
                }
            }

            // Fallback: attach to player
            _activeController = SparkEffect.AttachElemental(player.gameObject, element, intensity);

            if (_activeController != null)
                return CommandResult.Success($"Attached {element} effect to player (intensity: {intensity:F1})");

            return CommandResult.Error("Failed to create effect");
        }

        private static CommandResult HandleGear(CommandArgs args)
        {
            var player = args.Player;
            if (player == null)
                return CommandResult.Error("No player found");

            if (args.Count < 2)
                return CommandResult.Error("Usage: munin spark gear <element> <slot> [intensity]\nSlots: chest, helmet, shield, lefthand, righthand");

            Element element = ParseElement(args.Get(0));
            string slot = args.Get(1).ToLower();
            float intensity = args.Get<float>(2, 0.5f);
            intensity = Mathf.Max(0.1f, intensity);

            var visual = player.GetComponentInChildren<VisEquipment>();
            if (visual == null)
                return CommandResult.Error("No VisEquipment found");

            GameObject target = null;
            string targetName = slot;

            switch (slot)
            {
                case "chest":
                case "body":
                    target = visual.m_bodyModel?.gameObject;
                    targetName = "chest armor";
                    break;
                case "helmet":
                case "head":
                    target = GetFieldValue<GameObject>(visual, "m_helmetItemInstance");
                    targetName = "helmet";
                    break;
                case "lefthand":
                case "shield":
                    if (visual.m_leftHand != null)
                        target = FindEquippedItem(visual.m_leftHand);
                    targetName = "left hand/shield";
                    break;
                case "righthand":
                case "weapon":
                    if (visual.m_rightHand != null)
                        target = FindEquippedItem(visual.m_rightHand);
                    targetName = "right hand/weapon";
                    break;
                default:
                    return CommandResult.Error($"Unknown slot: {slot}");
            }

            if (target == null)
                return CommandResult.NotFound($"No {targetName} equipped");

            var controller = SparkEffect.AttachElemental(target, element, intensity);
            if (controller != null)
            {
                _gearEffects.Add(controller);
                var bounds = SparkEffect.GetBounds(target);
                return CommandResult.Success($"Added {element} to {targetName} (intensity: {intensity:F1}, type: {bounds.TargetType})");
            }

            return CommandResult.Error($"Failed to attach effect to {targetName}");
        }

        private static CommandResult HandleImpact(CommandArgs args)
        {
            var player = args.Player;
            if (player == null)
                return CommandResult.Error("No player found");

            Element element = args.Count > 0 ? ParseElement(args.Get(0)) : Element.Fire;
            Vector3 pos = player.transform.position + player.transform.forward * 3f + Vector3.up;

            SparkImpact.SpawnElemental(pos, element, 1.5f);
            return CommandResult.Success($"Spawned {element} impact");
        }

        private static CommandResult HandleExplosion(CommandArgs args)
        {
            var player = args.Player;
            if (player == null)
                return CommandResult.Error("No player found");

            Element element = args.Count > 0 ? ParseElement(args.Get(0)) : Element.Fire;
            float radius = args.Get<float>(1, 5f);

            Vector3 pos = player.transform.position + player.transform.forward * 5f;

            SparkImpact.Explosion(pos, new Core.Configs.ExplosionConfig
            {
                Element = element,
                Radius = radius,
                CameraShake = true
            });
            return CommandResult.Success($"Spawned {element} explosion (radius {radius})");
        }

        private static CommandResult HandleBolt(CommandArgs args)
        {
            var player = args.Player;
            if (player == null)
                return CommandResult.Error("No player found");

            Vector3 start = player.transform.position + Vector3.up * 10f + player.transform.forward * 3f;
            Vector3 end = player.transform.position + player.transform.forward * 3f;

            SparkProcedural.LightningBolt(start, end, new Core.Configs.LightningConfig
            {
                Segments = 12,
                JaggedAmount = 0.15f,
                Branches = 3,
                Duration = 0.3f
            });

            return CommandResult.Success("Spawned lightning bolt");
        }

        private static CommandResult HandleAura(CommandArgs args)
        {
            string preset = args.Get(0, "elite").ToLower();

            var creatures = Character.GetAllCharacters();
            Character nearest = null;
            float nearestDist = float.MaxValue;
            var playerPos = Player.m_localPlayer?.transform.position ?? Vector3.zero;

            foreach (var c in creatures)
            {
                if (c == Player.m_localPlayer) continue;
                float dist = Vector3.Distance(c.transform.position, playerPos);
                if (dist < nearestDist && dist < 50f)
                {
                    nearestDist = dist;
                    nearest = c;
                }
            }

            if (nearest == null)
                return CommandResult.NotFound("No creature found nearby");

            switch (preset)
            {
                case "enraged": SparkAura.AttachEnraged(nearest); break;
                case "frozen": SparkAura.AttachFrozen(nearest); break;
                case "elite": SparkAura.AttachElite(nearest); break;
                case "boss": SparkAura.AttachBoss(nearest); break;
                default: SparkAura.AttachElite(nearest); break;
            }

            return CommandResult.Success($"Added {preset} aura to {nearest.m_name}");
        }

        private static CommandResult HandleStatus(CommandArgs args)
        {
            var player = args.Player;
            if (player == null)
                return CommandResult.Error("No player found");

            string status = args.Get(0, "burning").ToLower();

            switch (status)
            {
                case "burning": SparkStatus.AttachBurning(player); break;
                case "freezing": SparkStatus.AttachFreezing(player); break;
                case "poisoned": SparkStatus.AttachPoisoned(player); break;
                case "blessed": SparkStatus.AttachBlessed(player); break;
                case "cursed": SparkStatus.AttachCursed(player); break;
                default: SparkStatus.AttachBurning(player); break;
            }

            return CommandResult.Success($"Added {status} status to player");
        }

        private static CommandResult HandleBounds(CommandArgs args)
        {
            var player = args.Player;
            if (player == null)
                return CommandResult.Error("No player found");

            var visual = player.GetComponentInChildren<VisEquipment>();
            if (visual == null || visual.m_rightHand == null)
                return CommandResult.Error("No weapon equipped");

            var bounds = SparkEffect.GetBounds(visual.m_rightHand.gameObject);
            var lines = new System.Collections.Generic.List<string>
            {
                "=== Weapon Bounds ===",
                $"Name: {visual.m_rightHand.gameObject.name}",
                $"Type: {bounds.TargetType}",
                $"Length: {bounds.Length:F3} (axis {bounds.LengthAxis})",
                $"Center: {bounds.Center}",
                $"Size: {bounds.Size}"
            };

            return CommandResult.Info(string.Join("\n", lines));
        }

        private static CommandResult HandleTextures(CommandArgs args)
        {
            var names = TextureLoader.GetLoadedTextureNames();
            var lines = new System.Collections.Generic.List<string> { "=== Loaded Textures ===" };
            int count = 0;
            foreach (var name in names)
            {
                lines.Add($"  {name}");
                count++;
            }
            lines.Add($"Total: {count} textures");

            return CommandResult.Info(string.Join("\n", lines));
        }

        private static CommandResult HandleClear(CommandArgs args)
        {
            var player = args.Player;
            if (player == null)
                return CommandResult.Error("No player found");

            // Clear from player
            SparkEffect.RemoveAll(player.gameObject);
            SparkVFX.RemoveElemental(player.gameObject);
            SparkStatus.RemoveAll(player);

            // Clear from weapon
            var visual = player.GetComponentInChildren<VisEquipment>();
            if (visual != null)
            {
                if (visual.m_rightHand != null)
                    SparkEffect.RemoveAll(visual.m_rightHand.gameObject);
                if (visual.m_leftHand != null)
                    SparkEffect.RemoveAll(visual.m_leftHand.gameObject);
                if (visual.m_bodyModel != null)
                    SparkEffect.RemoveAll(visual.m_bodyModel.gameObject);
            }

            // Clear tracked controller
            if (_activeController != null)
            {
                if (_activeController is MonoBehaviour mb)
                    Object.Destroy(mb.gameObject);
                _activeController = null;
            }

            // Clear gear effects
            foreach (var controller in _gearEffects)
            {
                if (controller is MonoBehaviour mb)
                    Object.Destroy(mb.gameObject);
            }
            _gearEffects.Clear();

            // Clear all item-tracked effects
            ItemEffectTracker.ClearAll();

            return CommandResult.Success("Cleared all effects (including tracked items)");
        }

        #endregion
    }
}
