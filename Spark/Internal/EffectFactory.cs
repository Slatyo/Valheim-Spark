using Spark.API;
using Spark.Core;
using Spark.Core.Configs;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Factory for creating visual effects.
    /// </summary>
    internal static class EffectFactory
    {
        /// <summary>
        /// Creates an elemental particle effect.
        /// </summary>
        public static SparkEffectHandle CreateElementalEffect(GameObject target, Element element, ElementalConfig config)
        {
            var effectGo = EffectPool.Get(element);
            if (effectGo == null) return null;

            effectGo.transform.SetParent(target.transform);
            effectGo.transform.localPosition = Vector3.zero;
            effectGo.transform.localRotation = Quaternion.identity;

            var definition = ElementDefinitions.Get(element);
            ConfigureParticleSystem(effectGo, definition, config);

            // Add light if enabled
            if (config.LightEnabled)
            {
                AddPointLight(effectGo, definition.LightColor, definition.LightIntensity * config.Intensity, config.LightRange);
            }

            // Track effect
            var handle = new SparkEffectHandle
            {
                EffectObject = effectGo,
                EffectId = System.Guid.NewGuid().ToString()
            };

            EffectTracker.RegisterEffect(target, handle);

            // Play particles
            var ps = effectGo.GetComponent<ParticleSystem>();
            ps?.Play();

            return handle;
        }

        /// <summary>
        /// Creates a glow effect.
        /// </summary>
        public static SparkEffectHandle CreateGlowEffect(GameObject target, GlowConfig config)
        {
            var effectGo = new GameObject("SparkGlow");
            effectGo.transform.SetParent(target.transform);
            effectGo.transform.localPosition = Vector3.zero;

            // Add glow component
            var glow = effectGo.AddComponent<GlowEffect>();
            glow.Initialize(config);

            if (config.LightEnabled)
            {
                AddPointLight(effectGo, config.Color, config.Intensity * 0.5f, config.LightRange);
            }

            var handle = new SparkEffectHandle
            {
                EffectObject = effectGo,
                EffectId = System.Guid.NewGuid().ToString()
            };

            EffectTracker.RegisterEffect(target, handle);
            return handle;
        }

        /// <summary>
        /// Creates a trail effect.
        /// </summary>
        public static SparkEffectHandle CreateTrailEffect(GameObject target, TrailConfig config)
        {
            var effectGo = new GameObject("SparkTrail");
            effectGo.transform.SetParent(target.transform);
            effectGo.transform.localPosition = Vector3.zero;

            // Add trail renderer
            var trail = effectGo.AddComponent<TrailRenderer>();
            trail.time = config.Duration;
            trail.startWidth = config.Width;
            trail.endWidth = 0f;
            trail.material = GetDefaultTrailMaterial();

            // Set colors
            Color color = config.Color;
            if (config.Element != Element.None)
            {
                var def = ElementDefinitions.Get(config.Element);
                color = def.PrimaryColor;
            }

            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0f);

            // Add velocity tracker for min velocity threshold
            var tracker = effectGo.AddComponent<TrailVelocityTracker>();
            tracker.Initialize(trail, config.MinVelocity);

            var handle = new SparkEffectHandle
            {
                EffectObject = effectGo,
                EffectId = System.Guid.NewGuid().ToString()
            };

            EffectTracker.RegisterEffect(target, handle);
            return handle;
        }

        /// <summary>
        /// Creates a full elemental weapon effect.
        /// </summary>
        public static SparkEffectHandle CreateWeaponElementalEffect(GameObject target, Element element, WeaponEffectConfig config)
        {
            var effectGo = new GameObject($"SparkWeaponEffect_{element}");
            effectGo.transform.SetParent(target.transform);
            effectGo.transform.localPosition = Vector3.zero;

            var definition = ElementDefinitions.Get(element);

            // Add particles
            if (config.ParticleIntensity > 0)
            {
                var particleGo = new GameObject("Particles");
                particleGo.transform.SetParent(effectGo.transform);
                particleGo.transform.localPosition = Vector3.zero;

                var ps = particleGo.AddComponent<ParticleSystem>();
                var elementalConfig = new ElementalConfig { Intensity = config.ParticleIntensity };
                ConfigureParticleSystem(particleGo, definition, elementalConfig);
                ps.Play();
            }

            // Add glow
            if (config.GlowIntensity > 0)
            {
                var glowGo = new GameObject("Glow");
                glowGo.transform.SetParent(effectGo.transform);
                glowGo.transform.localPosition = Vector3.zero;

                var glow = glowGo.AddComponent<GlowEffect>();
                glow.Initialize(new GlowConfig
                {
                    Color = definition.PrimaryColor,
                    Intensity = config.GlowIntensity
                });
            }

            // Add trail
            if (config.TrailEnabled)
            {
                var trailGo = new GameObject("Trail");
                trailGo.transform.SetParent(effectGo.transform);
                trailGo.transform.localPosition = Vector3.zero;

                var trail = trailGo.AddComponent<TrailRenderer>();
                trail.time = 0.3f;
                trail.startWidth = 0.1f;
                trail.endWidth = 0f;
                trail.material = GetDefaultTrailMaterial();
                trail.startColor = definition.PrimaryColor;
                trail.endColor = new Color(definition.PrimaryColor.r, definition.PrimaryColor.g, definition.PrimaryColor.b, 0f);

                var tracker = trailGo.AddComponent<TrailVelocityTracker>();
                tracker.Initialize(trail, 1f);
            }

            // Add light
            if (config.LightEnabled)
            {
                AddPointLight(effectGo, definition.LightColor, definition.LightIntensity, config.LightRange);
            }

            // Add ambient sound
            if (config.SoundEnabled && !string.IsNullOrEmpty(definition.AmbientSound))
            {
                AudioManager.PlayAttached(definition.AmbientSound, effectGo, loop: true);
            }

            var handle = new SparkEffectHandle
            {
                EffectObject = effectGo,
                EffectId = System.Guid.NewGuid().ToString()
            };

            EffectTracker.RegisterEffect(target, handle);
            return handle;
        }

        private static void ConfigureParticleSystem(GameObject go, ElementalEffectDefinition definition, ElementalConfig config)
        {
            var ps = go.GetComponent<ParticleSystem>();
            if (ps == null) ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(definition.PrimaryColor, definition.SecondaryColor);
            main.startLifetime = definition.ParticleLifetime;
            main.startSize = definition.ParticleSize * config.Scale;
            main.gravityModifier = definition.GravityModifier;
            main.maxParticles = SparkConfig.MaxParticlesPerSystem;

            var emission = ps.emission;
            emission.rateOverTime = definition.EmissionRate * config.Intensity * config.EmissionMultiplier * SparkConfig.QualityMultiplier;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f * config.Scale;

            if (definition.NoiseStrength > 0)
            {
                var noise = ps.noise;
                noise.enabled = true;
                noise.strength = definition.NoiseStrength;
                noise.frequency = definition.NoiseFrequency;
            }

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.material = GetDefaultParticleMaterial();
            }
        }

        private static void AddPointLight(GameObject parent, Color color, float intensity, float range)
        {
            // Check light limit
            var existingLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            int activeLights = 0;
            foreach (var l in existingLights)
            {
                if (l.enabled && l.type == LightType.Point) activeLights++;
            }

            if (activeLights >= SparkConfig.MaxActiveLights)
            {
                return;
            }

            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(parent.transform);
            lightGo.transform.localPosition = Vector3.zero;

            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
        }

        private static Material _defaultParticleMaterial;
        private static Material GetDefaultParticleMaterial()
        {
            if (_defaultParticleMaterial == null)
            {
                _defaultParticleMaterial = ShaderUtils.CreateParticleMaterial();
            }
            return _defaultParticleMaterial;
        }

        private static Material _defaultTrailMaterial;
        private static Material GetDefaultTrailMaterial()
        {
            if (_defaultTrailMaterial == null)
            {
                _defaultTrailMaterial = ShaderUtils.CreateParticleMaterial();
            }
            return _defaultTrailMaterial;
        }
    }
}
