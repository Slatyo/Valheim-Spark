using System.Collections.Generic;
using Spark.API;
using Spark.Core;
using Spark.Core.Configs;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Manages environmental zone effects.
    /// </summary>
    internal static class ZoneManager
    {
        private static readonly List<SparkZoneHandle> ActiveZones = new List<SparkZoneHandle>();

        /// <summary>
        /// Creates an environmental zone.
        /// </summary>
        public static SparkZoneHandle CreateZone(Vector3 position, ZoneConfig config)
        {
            if (!SparkConfig.EffectsEnabled) return null;

            var zoneGo = new GameObject($"SparkZone_{config.Type}");
            zoneGo.transform.position = position;

            Element element = GetElementForZone(config.Type);
            var definition = ElementDefinitions.Get(element);
            Color color = config.ColorOverride ?? definition.PrimaryColor;

            // Create particle system
            var ps = zoneGo.AddComponent<ParticleSystem>();
            ConfigureZoneParticles(ps, config, definition, color);

            // Play ambient sound
            if (config.PlaySound)
            {
                string soundId = GetZoneSound(config.Type);
                if (!string.IsNullOrEmpty(soundId))
                {
                    AudioManager.PlayAttached(soundId, zoneGo, loop: true);
                }
            }

            var handle = new SparkZoneHandle
            {
                ZoneObject = zoneGo,
                ZoneId = System.Guid.NewGuid().ToString(),
                Duration = config.Duration,
                ElapsedTime = 0f
            };

            ActiveZones.Add(handle);

            // Add updater component for duration tracking
            if (config.Duration > 0)
            {
                var updater = zoneGo.AddComponent<ZoneUpdater>();
                updater.Initialize(handle);
            }

            ps.Play();
            return handle;
        }

        /// <summary>
        /// Updates active zones (called from Plugin.Update).
        /// </summary>
        public static void Update(float deltaTime)
        {
            for (int i = ActiveZones.Count - 1; i >= 0; i--)
            {
                var handle = ActiveZones[i];
                if (handle == null || !handle.IsActive)
                {
                    ActiveZones.RemoveAt(i);
                    continue;
                }

                // Duration tracking is handled by ZoneUpdater component
            }
        }

        private static void ConfigureZoneParticles(ParticleSystem ps, ZoneConfig config, ElementalEffectDefinition definition, Color color)
        {
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color, new Color(color.r, color.g, color.b, 0.5f));
            main.startLifetime = 2f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.gravityModifier = definition.GravityModifier;
            main.maxParticles = (int)(200 * SparkConfig.QualityMultiplier);

            var emission = ps.emission;
            emission.rateOverTime = 50 * config.Intensity * SparkConfig.QualityMultiplier;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new UnityEngine.Vector3(config.Radius * 2, config.Height, config.Radius * 2);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.2f;
            noise.frequency = 0.5f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = ShaderUtils.CreateParticleMaterial();
        }

        private static Element GetElementForZone(ZoneType type)
        {
            return type switch
            {
                ZoneType.PoisonCloud => Element.Poison,
                ZoneType.FireField => Element.Fire,
                ZoneType.FrostZone => Element.Frost,
                ZoneType.HolyGround => Element.Spirit,
                ZoneType.Corruption => Element.Shadow,
                ZoneType.ArcaneCircle => Element.Arcane,
                ZoneType.LightningField => Element.Lightning,
                _ => Element.None
            };
        }

        private static string GetZoneSound(ZoneType type)
        {
            return type switch
            {
                ZoneType.FireField => "fire_zone_loop",
                ZoneType.FrostZone => "frost_zone_loop",
                ZoneType.LightningField => "lightning_zone_loop",
                _ => null
            };
        }
    }
}
