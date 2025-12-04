using Spark.Core;
using Spark.Core.Configs;
using Spark.Internal;
using UnityEngine;

namespace Spark.API
{
    /// <summary>
    /// API for weapon visual effects (glow, trails, elemental).
    /// </summary>
    public static class SparkWeapon
    {
        /// <summary>
        /// Adds a glow effect to a weapon.
        /// </summary>
        /// <param name="weapon">Weapon GameObject (visual).</param>
        /// <param name="config">Glow configuration.</param>
        /// <returns>Effect handle.</returns>
        public static SparkEffectHandle AddGlow(GameObject weapon, GlowConfig config)
        {
            if (weapon == null)
            {
                Plugin.Log?.LogWarning("SparkWeapon.AddGlow: weapon is null");
                return null;
            }

            config ??= new GlowConfig();
            return EffectFactory.CreateGlowEffect(weapon, config);
        }

        /// <summary>
        /// Adds a trail effect to a weapon that follows swings.
        /// </summary>
        /// <param name="weapon">Weapon GameObject (visual).</param>
        /// <param name="config">Trail configuration.</param>
        /// <returns>Effect handle.</returns>
        public static SparkEffectHandle AddTrail(GameObject weapon, TrailConfig config)
        {
            if (weapon == null)
            {
                Plugin.Log?.LogWarning("SparkWeapon.AddTrail: weapon is null");
                return null;
            }

            config ??= new TrailConfig();
            return EffectFactory.CreateTrailEffect(weapon, config);
        }

        /// <summary>
        /// Adds a full elemental effect to a weapon (particles, glow, trail, light).
        /// </summary>
        /// <param name="weapon">Weapon GameObject (visual).</param>
        /// <param name="element">Element type.</param>
        /// <param name="config">Optional weapon effect configuration.</param>
        /// <returns>Effect handle.</returns>
        public static SparkEffectHandle AddElementalEffect(GameObject weapon, Element element, WeaponEffectConfig config = null)
        {
            if (weapon == null)
            {
                Plugin.Log?.LogWarning("SparkWeapon.AddElementalEffect: weapon is null");
                return null;
            }

            config ??= new WeaponEffectConfig();
            return EffectFactory.CreateWeaponElementalEffect(weapon, element, config);
        }

        /// <summary>
        /// Removes all effects from a weapon.
        /// </summary>
        /// <param name="weapon">Weapon GameObject.</param>
        public static void RemoveEffects(GameObject weapon)
        {
            if (weapon == null) return;
            EffectTracker.RemoveAllEffects(weapon);
        }

        /// <summary>
        /// Sets weapon effect intensity.
        /// </summary>
        /// <param name="weapon">Weapon GameObject.</param>
        /// <param name="intensity">New intensity (0-2).</param>
        public static void SetIntensity(GameObject weapon, float intensity)
        {
            if (weapon == null) return;
            EffectTracker.SetIntensity(weapon, intensity);
        }
    }
}
