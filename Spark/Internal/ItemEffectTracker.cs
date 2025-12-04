using System.Collections.Generic;
using Spark.API;
using Spark.Core;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Tracks elemental effects attached to items (not GameObjects).
    /// Handles automatic reapplication when item visuals are recreated (holster/unholster).
    /// </summary>
    internal static class ItemEffectTracker
    {
        /// <summary>
        /// Data for a tracked item effect.
        /// </summary>
        internal class TrackedItemEffect
        {
            public string ItemKey { get; set; }
            public Element Element { get; set; }
            public float Intensity { get; set; }
            public IEffectController CurrentController { get; set; }
            public GameObject CurrentVisual { get; set; }
        }

        // Track effects by item key (prefab hash + grid position + quality)
        private static readonly Dictionary<string, TrackedItemEffect> TrackedItems = new Dictionary<string, TrackedItemEffect>();

        // Track effects by visual instance ID for quick lookup
        private static readonly Dictionary<int, TrackedItemEffect> VisualToEffect = new Dictionary<int, TrackedItemEffect>();

        /// <summary>
        /// Gets a unique key for an ItemDrop.ItemData that persists across visual recreation.
        /// </summary>
        public static string GetItemKey(ItemDrop.ItemData item)
        {
            if (item == null) return null;

            // Combine prefab hash, grid position, and quality for uniqueness
            int prefabHash = item.m_dropPrefab != null ? item.m_dropPrefab.name.GetStableHashCode() : 0;
            return $"{prefabHash}_{item.m_gridPos.x}_{item.m_gridPos.y}_{item.m_quality}";
        }

        /// <summary>
        /// Gets item key from prefab hash (used in AttachItem hook).
        /// Returns null if no matching tracked item.
        /// </summary>
        public static TrackedItemEffect GetTrackedEffectByHash(int prefabHash, Player player)
        {
            if (player == null) return null;

            var inventory = player.GetInventory();
            if (inventory == null) return null;

            // Find equipped item matching this hash
            foreach (var item in inventory.GetAllItems())
            {
                if (!item.m_equipped) continue;
                if (item.m_dropPrefab == null) continue;
                if (item.m_dropPrefab.name.GetStableHashCode() != prefabHash) continue;

                string key = GetItemKey(item);
                if (TrackedItems.TryGetValue(key, out var effect))
                {
                    return effect;
                }
            }

            return null;
        }

        /// <summary>
        /// Register an effect for an item. The effect will be auto-reapplied on holster/unholster.
        /// </summary>
        public static void RegisterItemEffect(ItemDrop.ItemData item, Element element, float intensity)
        {
            if (item == null) return;

            string key = GetItemKey(item);
            if (key == null) return;

            // Remove existing if any
            if (TrackedItems.TryGetValue(key, out var existing))
            {
                DestroyEffect(existing);
            }

            TrackedItems[key] = new TrackedItemEffect
            {
                ItemKey = key,
                Element = element,
                Intensity = intensity,
                CurrentController = null,
                CurrentVisual = null
            };

            Plugin.Log?.LogDebug($"[ItemEffectTracker] Registered {element} effect for item: {key}");
        }

        /// <summary>
        /// Remove effect tracking for an item.
        /// </summary>
        public static void UnregisterItemEffect(ItemDrop.ItemData item)
        {
            if (item == null) return;

            string key = GetItemKey(item);
            if (key == null) return;

            if (TrackedItems.TryGetValue(key, out var effect))
            {
                DestroyEffect(effect);
                TrackedItems.Remove(key);
                Plugin.Log?.LogDebug($"[ItemEffectTracker] Unregistered effect for item: {key}");
            }
        }

        /// <summary>
        /// Check if an item has a tracked effect.
        /// </summary>
        public static bool HasTrackedEffect(ItemDrop.ItemData item)
        {
            if (item == null) return false;
            string key = GetItemKey(item);
            return key != null && TrackedItems.ContainsKey(key);
        }

        /// <summary>
        /// Apply effect to a visual GameObject. Called by AttachItem hook.
        /// </summary>
        public static void ApplyEffectToVisual(TrackedItemEffect effect, GameObject visual)
        {
            if (effect == null || visual == null) return;

            // Clean up old visual if different
            if (effect.CurrentVisual != null && effect.CurrentVisual != visual)
            {
                DestroyEffect(effect);
            }

            // Check if visual already has effect
            if (effect.CurrentVisual == visual && effect.CurrentController != null)
            {
                return; // Already applied
            }

            // Calculate bounds and create effect
            var bounds = BoundsCalculator.Calculate(visual);
            IEffectController controller = effect.Element switch
            {
                Element.Fire => CreateFireEffect(visual, bounds, effect.Intensity),
                Element.Lightning => CreateLightningEffect(visual, bounds, effect.Intensity),
                // Other elements fall back to fire for now
                _ => CreateFireEffect(visual, bounds, effect.Intensity)
            };

            if (controller is ITargetTypeAdapter adapter)
            {
                adapter.AdaptToTargetType(bounds.TargetType);
            }

            effect.CurrentController = controller;
            effect.CurrentVisual = visual;

            // Track by visual instance ID
            VisualToEffect[visual.GetInstanceID()] = effect;

            Plugin.Log?.LogDebug($"[ItemEffectTracker] Applied {effect.Element} effect to visual: {visual.name}");
        }

        private static FireEffectController CreateFireEffect(GameObject target, SparkBounds bounds, float intensity)
        {
            var effectGo = new GameObject("SparkEffect_Fire");
            effectGo.transform.SetParent(target.transform, false);

            var controller = effectGo.AddComponent<FireEffectController>();
            controller.Initialize(bounds);
            controller.SetIntensity(intensity);

            return controller;
        }

        private static LightningEffectController CreateLightningEffect(GameObject target, SparkBounds bounds, float intensity)
        {
            var effectGo = new GameObject("SparkEffect_Lightning");
            effectGo.transform.SetParent(target.transform, false);

            var controller = effectGo.AddComponent<LightningEffectController>();
            controller.Initialize(bounds);
            controller.SetIntensity(intensity);

            return controller;
        }

        private static void DestroyEffect(TrackedItemEffect effect)
        {
            if (effect == null) return;

            if (effect.CurrentVisual != null)
            {
                VisualToEffect.Remove(effect.CurrentVisual.GetInstanceID());
            }

            if (effect.CurrentController is MonoBehaviour mb && mb != null)
            {
                Object.Destroy(mb.gameObject);
            }

            effect.CurrentController = null;
            effect.CurrentVisual = null;
        }

        /// <summary>
        /// Clean up tracking for destroyed visuals.
        /// </summary>
        public static void Cleanup()
        {
            var toClean = new List<string>();

            foreach (var kvp in TrackedItems)
            {
                var effect = kvp.Value;

                // Check if visual was destroyed
                if (effect.CurrentVisual == null && effect.CurrentController != null)
                {
                    // Visual destroyed but we still have controller reference - clean it
                    if (effect.CurrentController is MonoBehaviour mb && mb != null)
                    {
                        Object.Destroy(mb.gameObject);
                    }
                    effect.CurrentController = null;
                }
            }

            // Clean up visual mapping
            var visualsToRemove = new List<int>();
            foreach (var kvp in VisualToEffect)
            {
                // Can't easily check if GO is destroyed, but TrackedItems cleanup handles controller
            }
        }

        /// <summary>
        /// Remove all tracked effects.
        /// </summary>
        public static void ClearAll()
        {
            foreach (var effect in TrackedItems.Values)
            {
                DestroyEffect(effect);
            }
            TrackedItems.Clear();
            VisualToEffect.Clear();
        }

        /// <summary>
        /// Get count of tracked items (for debugging).
        /// </summary>
        public static int TrackedCount => TrackedItems.Count;
    }
}
