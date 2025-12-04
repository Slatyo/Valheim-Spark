using System.Collections.Generic;
using Spark.API;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Tracks effects attached to GameObjects.
    /// </summary>
    internal static class EffectTracker
    {
        private static readonly Dictionary<int, List<SparkEffectHandle>> TrackedEffects = new Dictionary<int, List<SparkEffectHandle>>();

        /// <summary>
        /// Registers an effect for tracking.
        /// </summary>
        public static void RegisterEffect(GameObject target, SparkEffectHandle handle)
        {
            if (target == null || handle == null) return;

            int id = target.GetInstanceID();
            if (!TrackedEffects.TryGetValue(id, out var list))
            {
                list = new List<SparkEffectHandle>();
                TrackedEffects[id] = list;
            }

            list.Add(handle);
        }

        /// <summary>
        /// Checks if a GameObject has any effects.
        /// </summary>
        public static bool HasEffects(GameObject target)
        {
            if (target == null) return false;

            int id = target.GetInstanceID();
            if (TrackedEffects.TryGetValue(id, out var list))
            {
                // Clean up destroyed effects
                list.RemoveAll(h => h == null || !h.IsActive);
                return list.Count > 0;
            }

            return false;
        }

        /// <summary>
        /// Removes all effects from a GameObject.
        /// </summary>
        public static void RemoveAllEffects(GameObject target)
        {
            if (target == null) return;

            int id = target.GetInstanceID();
            if (TrackedEffects.TryGetValue(id, out var list))
            {
                foreach (var handle in list)
                {
                    handle?.Destroy();
                }
                list.Clear();
                TrackedEffects.Remove(id);
            }
        }

        /// <summary>
        /// Sets intensity on all effects on a GameObject.
        /// </summary>
        public static void SetIntensity(GameObject target, float intensity)
        {
            if (target == null) return;

            int id = target.GetInstanceID();
            if (TrackedEffects.TryGetValue(id, out var list))
            {
                foreach (var handle in list)
                {
                    handle?.SetIntensity(intensity);
                }
            }
        }

        /// <summary>
        /// Cleans up tracking for destroyed objects.
        /// </summary>
        public static void Cleanup()
        {
            var toRemove = new List<int>();

            foreach (var kvp in TrackedEffects)
            {
                kvp.Value.RemoveAll(h => h == null || !h.IsActive);
                if (kvp.Value.Count == 0)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var id in toRemove)
            {
                TrackedEffects.Remove(id);
            }
        }
    }
}
