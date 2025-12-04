using Spark.Core;
using Spark.Core.Configs;
using Spark.Internal;
using UnityEngine;

namespace Spark.API
{
    /// <summary>
    /// API for creature aura effects.
    /// </summary>
    public static class SparkAura
    {
        /// <summary>
        /// Attaches a custom aura to a creature.
        /// </summary>
        /// <param name="creature">Target Character component.</param>
        /// <param name="config">Aura configuration.</param>
        /// <returns>Aura ID for removal.</returns>
        public static string Attach(Character creature, AuraConfig config)
        {
            if (creature == null)
            {
                Plugin.Log?.LogWarning("SparkAura.Attach: creature is null");
                return null;
            }

            config ??= new AuraConfig();
            return AuraManager.AttachAura(creature, config);
        }

        /// <summary>
        /// Removes a specific aura by ID.
        /// </summary>
        /// <param name="creature">Target Character.</param>
        /// <param name="auraId">Aura ID from Attach.</param>
        public static void Remove(Character creature, string auraId)
        {
            if (creature == null || string.IsNullOrEmpty(auraId)) return;
            AuraManager.RemoveAura(creature, auraId);
        }

        /// <summary>
        /// Removes all auras from a creature.
        /// </summary>
        /// <param name="creature">Target Character.</param>
        public static void RemoveAll(Character creature)
        {
            if (creature == null) return;
            AuraManager.RemoveAllAuras(creature);
        }

        // === Preset Auras ===

        /// <summary>Attaches enraged aura (red pulsing).</summary>
        public static string AttachEnraged(Character creature)
        {
            return Attach(creature, new AuraConfig
            {
                Type = AuraType.Ring,
                Color = new Color(1f, 0.2f, 0.1f),
                Radius = 1.5f,
                Intensity = 1.2f,
                Pulse = true,
                PulseSpeed = 2f
            });
        }

        /// <summary>Attaches frozen aura (blue ice particles).</summary>
        public static string AttachFrozen(Character creature)
        {
            return Attach(creature, new AuraConfig
            {
                Type = AuraType.Sphere,
                Color = new Color(0.5f, 0.8f, 1f),
                Radius = 1.2f,
                Intensity = 0.8f,
                Element = Element.Frost
            });
        }

        /// <summary>Attaches poisoned aura (green cloud).</summary>
        public static string AttachPoisoned(Character creature)
        {
            return Attach(creature, new AuraConfig
            {
                Type = AuraType.Ground,
                Color = new Color(0.3f, 0.9f, 0.2f),
                Radius = 2f,
                Intensity = 0.7f,
                Element = Element.Poison
            });
        }

        /// <summary>Attaches shielded aura (golden barrier).</summary>
        public static string AttachShielded(Character creature)
        {
            return Attach(creature, new AuraConfig
            {
                Type = AuraType.Sphere,
                Color = new Color(1f, 0.85f, 0.3f),
                Radius = 1.5f,
                Intensity = 0.6f,
                Pulse = true,
                PulseSpeed = 0.5f
            });
        }

        /// <summary>Attaches elite creature indicator.</summary>
        public static string AttachElite(Character creature)
        {
            return Attach(creature, new AuraConfig
            {
                Type = AuraType.Ring,
                Color = new Color(0.8f, 0.5f, 1f),
                Radius = 2f,
                Intensity = 1f,
                Rotate = true,
                RotationSpeed = 30f
            });
        }

        /// <summary>Attaches boss aura.</summary>
        public static string AttachBoss(Character creature)
        {
            return Attach(creature, new AuraConfig
            {
                Type = AuraType.Pillar,
                Color = new Color(1f, 0.3f, 0.1f),
                Radius = 3f,
                Height = 5f,
                Intensity = 1.5f,
                Pulse = true,
                PulseSpeed = 1f
            });
        }
    }
}
