using Spark.Core;
using Spark.Core.Configs;
using Spark.Internal;
using UnityEngine;

namespace Spark.API
{
    /// <summary>
    /// Main API for attaching elemental visual effects to GameObjects.
    /// </summary>
    public static class SparkVFX
    {
        /// <summary>
        /// Attaches an elemental particle effect to a GameObject.
        /// </summary>
        /// <param name="target">Target GameObject to attach effect to.</param>
        /// <param name="element">Element type for the effect.</param>
        /// <param name="config">Optional configuration.</param>
        /// <returns>Handle to the spawned effect, or null if failed.</returns>
        public static SparkEffectHandle AttachElemental(GameObject target, Element element, ElementalConfig config = null)
        {
            if (target == null)
            {
                Plugin.Log?.LogWarning("SparkVFX.AttachElemental: target is null");
                return null;
            }

            config ??= new ElementalConfig();
            return EffectFactory.CreateElementalEffect(target, element, config);
        }

        /// <summary>
        /// Removes all elemental effects from a GameObject.
        /// </summary>
        /// <param name="target">Target GameObject.</param>
        public static void RemoveElemental(GameObject target)
        {
            if (target == null) return;
            EffectTracker.RemoveAllEffects(target);
        }

        /// <summary>
        /// Removes a specific effect by handle.
        /// </summary>
        /// <param name="handle">Effect handle returned from Attach methods.</param>
        public static void Remove(SparkEffectHandle handle)
        {
            if (handle == null) return;
            handle.Destroy();
        }

        /// <summary>
        /// Checks if a GameObject has any Spark effects attached.
        /// </summary>
        /// <param name="target">Target GameObject.</param>
        /// <returns>True if effects are attached.</returns>
        public static bool HasEffects(GameObject target)
        {
            if (target == null) return false;
            return EffectTracker.HasEffects(target);
        }

        /// <summary>
        /// Sets the intensity of all effects on a GameObject.
        /// </summary>
        /// <param name="target">Target GameObject.</param>
        /// <param name="intensity">New intensity (0-2).</param>
        public static void SetIntensity(GameObject target, float intensity)
        {
            if (target == null) return;
            EffectTracker.SetIntensity(target, intensity);
        }
    }
}
