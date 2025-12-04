using Spark.Core;
using Spark.Internal;
using UnityEngine;

namespace Spark.API
{
    /// <summary>
    /// Simplified API for attaching Spark effects to any GameObject or Item.
    /// Currently supports Fire and Lightning - other elements coming soon.
    ///
    /// For items that can be holstered/unholstered (weapons, shields), use AttachToItem().
    /// For static objects (armor, buildings, world items), use AttachElemental().
    /// </summary>
    public static class SparkEffect
    {
        #region Item-Based Effects (Auto-Reapply on Holster/Unholster)

        /// <summary>
        /// Attach an elemental effect to an equipped item. The effect will automatically
        /// be reapplied when the item visual is recreated (holster/unholster/re-equip).
        ///
        /// This is the recommended method for weapons, shields, and other equipment.
        /// </summary>
        /// <param name="item">The item data (from inventory).</param>
        /// <param name="element">Element type for the effect.</param>
        /// <param name="intensity">Effect intensity (no max limit, default 1).</param>
        public static void AttachToItem(ItemDrop.ItemData item, Element element, float intensity = 1f)
        {
            if (item == null)
            {
                Plugin.Log?.LogWarning("SparkEffect.AttachToItem: item is null");
                return;
            }

            // Register with tracker - effect will be applied when visual is created/recreated
            ItemEffectTracker.RegisterItemEffect(item, element, intensity);

            // If item is currently equipped and visible, apply immediately
            var player = Player.m_localPlayer;
            if (player != null && item.m_equipped)
            {
                TryApplyToCurrentVisual(player, item, element, intensity);
            }
        }

        /// <summary>
        /// Remove the tracked effect from an item.
        /// </summary>
        public static void RemoveFromItem(ItemDrop.ItemData item)
        {
            if (item == null) return;
            ItemEffectTracker.UnregisterItemEffect(item);
        }

        /// <summary>
        /// Check if an item has a tracked effect.
        /// </summary>
        public static bool HasItemEffect(ItemDrop.ItemData item)
        {
            return ItemEffectTracker.HasTrackedEffect(item);
        }

        private static void TryApplyToCurrentVisual(Player player, ItemDrop.ItemData item, Element element, float intensity)
        {
            var visual = player.GetComponentInChildren<VisEquipment>();
            if (visual == null) return;

            // Try to find the current visual for this item
            GameObject targetVisual = null;

            // Check if it's the current weapon (right hand or left hand)
            var currentWeapon = player.GetCurrentWeapon();
            if (currentWeapon == item)
            {
                // Check both hands - weapon could be in either
                targetVisual = FindItemVisual(visual.m_rightHand) ?? FindItemVisual(visual.m_leftHand);
            }
            else
            {
                // Check for shield in left hand
                targetVisual = FindItemVisual(visual.m_leftHand);
            }

            if (targetVisual != null)
            {
                // Get the tracked effect and apply it
                var trackedEffect = ItemEffectTracker.GetTrackedEffectByHash(
                    item.m_dropPrefab?.name.GetStableHashCode() ?? 0, player);

                if (trackedEffect != null)
                {
                    ItemEffectTracker.ApplyEffectToVisual(trackedEffect, targetVisual);
                }
            }
        }

        private static GameObject FindItemVisual(Transform handTransform)
        {
            if (handTransform == null) return null;

            foreach (Transform child in handTransform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    // Look for mesh - indicates actual item visual
                    if (child.GetComponent<MeshFilter>() != null ||
                        child.GetComponent<SkinnedMeshRenderer>() != null ||
                        child.GetComponentInChildren<MeshFilter>() != null)
                    {
                        return child.gameObject;
                    }
                }
            }

            return null;
        }

        #endregion

        #region GameObject-Based Effects (Direct Attachment)

        /// <summary>
        /// Attach an elemental effect to any target with automatic bounds detection.
        /// Currently only Fire and Lightning have specialized controllers.
        ///
        /// Note: For equipment items that can be holstered, use AttachToItem() instead
        /// to ensure effects persist across holster/unholster.
        /// </summary>
        /// <param name="target">Target GameObject to attach effect to.</param>
        /// <param name="element">Element type for the effect.</param>
        /// <param name="intensity">Effect intensity (no max limit, default 1).</param>
        /// <returns>The controller component implementing IEffectController, or null if failed.</returns>
        public static IEffectController AttachElemental(GameObject target, Element element, float intensity = 1f)
        {
            if (target == null)
            {
                Plugin.Log?.LogWarning("SparkEffect.AttachElemental: target is null");
                return null;
            }

            var bounds = BoundsCalculator.Calculate(target);

            IEffectController controller = element switch
            {
                Element.Fire => AttachFireInternal(target, bounds, intensity),
                Element.Lightning => AttachLightningInternal(target, bounds, intensity),
                // Other elements fall back to fire for now until we port their specialized controllers
                Element.Frost => AttachFireInternal(target, bounds, intensity),  // TODO: Port FrostEffectController
                Element.Poison => AttachFireInternal(target, bounds, intensity), // TODO: Port PoisonEffectController
                Element.Spirit => AttachFireInternal(target, bounds, intensity),
                Element.Shadow => AttachFireInternal(target, bounds, intensity),
                Element.Arcane => AttachFireInternal(target, bounds, intensity),
                _ => AttachFireInternal(target, bounds, intensity)
            };

            // Apply target type adaptation if supported
            if (controller is ITargetTypeAdapter adapter)
            {
                adapter.AdaptToTargetType(bounds.TargetType);
            }

            return controller;
        }

        /// <summary>
        /// Attach a fire effect to any target.
        /// </summary>
        public static FireEffectController AttachFire(GameObject target, float intensity = 1f)
        {
            if (target == null) return null;
            var bounds = BoundsCalculator.Calculate(target);
            var controller = AttachFireInternal(target, bounds, intensity);
            controller?.AdaptToTargetType(bounds.TargetType);
            return controller;
        }

        private static FireEffectController AttachFireInternal(GameObject target, SparkBounds bounds, float intensity)
        {
            var effectGo = new GameObject("SparkEffect_Fire");
            effectGo.transform.SetParent(target.transform, false);

            var controller = effectGo.AddComponent<FireEffectController>();
            controller.Initialize(bounds);
            controller.SetIntensity(intensity);

            return controller;
        }

        /// <summary>
        /// Attach a lightning effect to any target.
        /// </summary>
        public static LightningEffectController AttachLightning(GameObject target, float intensity = 1f)
        {
            if (target == null) return null;
            var bounds = BoundsCalculator.Calculate(target);
            var controller = AttachLightningInternal(target, bounds, intensity);
            controller?.AdaptToTargetType(bounds.TargetType);
            return controller;
        }

        /// <summary>
        /// Attach a lightning effect with custom zap intervals.
        /// </summary>
        public static LightningEffectController AttachLightning(GameObject target, float zapIntervalMin, float zapIntervalMax, float chainChance = 0.2f)
        {
            if (target == null) return null;
            var bounds = BoundsCalculator.Calculate(target);

            var effectGo = new GameObject("SparkEffect_Lightning");
            effectGo.transform.SetParent(target.transform, false);

            var controller = effectGo.AddComponent<LightningEffectController>();
            controller.Initialize(bounds);
            controller.Configure(zapIntervalMin, zapIntervalMax, chainChance);

            return controller;
        }

        private static LightningEffectController AttachLightningInternal(GameObject target, SparkBounds bounds, float intensity)
        {
            var effectGo = new GameObject("SparkEffect_Lightning");
            effectGo.transform.SetParent(target.transform, false);

            var controller = effectGo.AddComponent<LightningEffectController>();
            controller.Initialize(bounds);
            controller.SetIntensity(intensity);

            return controller;
        }

        /// <summary>
        /// Remove all Spark effects from a target.
        /// </summary>
        public static void RemoveAll(GameObject target)
        {
            if (target == null) return;

            var toDestroy = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in target.transform)
            {
                if (child.name.StartsWith("SparkEffect"))
                {
                    toDestroy.Add(child.gameObject);
                }
            }

            foreach (var go in toDestroy)
            {
                Object.Destroy(go);
            }
        }

        /// <summary>
        /// Check if a target has any Spark effects attached.
        /// </summary>
        public static bool HasEffects(GameObject target)
        {
            if (target == null) return false;

            foreach (Transform child in target.transform)
            {
                if (child.name.StartsWith("SparkEffect"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the calculated bounds for a target (useful for debugging or custom effects).
        /// </summary>
        public static SparkBounds GetBounds(GameObject target)
        {
            return BoundsCalculator.Calculate(target);
        }

        #endregion
    }
}
