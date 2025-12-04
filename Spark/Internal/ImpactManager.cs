using Spark.Core;
using Spark.Core.Configs;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Manages one-shot impact effects.
    /// </summary>
    internal static class ImpactManager
    {
        /// <summary>
        /// Spawns a preset impact effect.
        /// </summary>
        public static void SpawnImpact(Vector3 position, ImpactType type)
        {
            Element element = GetElementForImpact(type);
            float scale = GetScaleForImpact(type);

            SpawnCustomImpact(position, new ImpactConfig
            {
                Element = element,
                Scale = scale,
                PlaySound = true
            });
        }

        /// <summary>
        /// Spawns a custom impact effect.
        /// </summary>
        public static void SpawnCustomImpact(Vector3 position, ImpactConfig config)
        {
            if (!SparkConfig.EffectsEnabled) return;

            var effectGo = EffectPool.Get(config.Element);
            if (effectGo == null) return;

            effectGo.transform.position = position;
            if (config.Rotation.HasValue)
            {
                effectGo.transform.rotation = config.Rotation.Value;
            }

            var definition = ElementDefinitions.Get(config.Element);
            ConfigureImpactParticles(effectGo, definition, config);

            // Auto-return to pool after effect completes
            var returner = effectGo.AddComponent<PoolReturner>();
            returner.Initialize(config.Element, definition.ParticleLifetime + 0.5f);

            // Play sound
            if (config.PlaySound && !string.IsNullOrEmpty(definition.ImpactSound))
            {
                AudioManager.PlaySound(definition.ImpactSound, position, new AudioConfig
                {
                    Category = SoundCategory.Impact,
                    PitchVariation = 0.1f
                });
            }
        }

        /// <summary>
        /// Spawns an explosion effect.
        /// </summary>
        public static void SpawnExplosion(Vector3 position, ExplosionConfig config)
        {
            if (!SparkConfig.EffectsEnabled) return;

            var effectGo = EffectPool.Get(config.Element);
            if (effectGo == null) return;

            effectGo.transform.position = position;

            var definition = ElementDefinitions.Get(config.Element);
            ConfigureExplosionParticles(effectGo, definition, config);

            // Auto-return to pool
            var returner = effectGo.AddComponent<PoolReturner>();
            returner.Initialize(config.Element, config.Duration + 0.5f);

            // Camera shake
            if (config.CameraShake)
            {
                // TODO: Integrate with game's camera shake system
            }

            // Play explosion sound
            if (config.PlaySound)
            {
                string soundId = $"{config.Element.ToString().ToLower()}_explosion";
                AudioManager.PlaySound(soundId, position, new AudioConfig
                {
                    Category = SoundCategory.Impact,
                    Volume = 1f,
                    MaxDistance = config.Radius * 3
                });
            }
        }

        private static void ConfigureImpactParticles(GameObject go, ElementalEffectDefinition definition, ImpactConfig config)
        {
            var ps = go.GetComponent<ParticleSystem>();
            if (ps == null) ps = go.AddComponent<ParticleSystem>();

            Color color = config.ColorOverride ?? definition.PrimaryColor;

            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color, definition.SecondaryColor);
            main.startLifetime = definition.ParticleLifetime * 0.5f;
            main.startSize = definition.ParticleSize * config.Scale * 2f;
            main.startSpeed = 3f * config.Scale;
            main.gravityModifier = 0.5f;
            main.maxParticles = (int)(30 * SparkConfig.QualityMultiplier);

            var emission = ps.emission;
            emission.enabled = false; // Burst only

            var burst = new ParticleSystem.Burst(0f, (short)(20 * SparkConfig.QualityMultiplier));
            ps.emission.SetBurst(0, burst);
            emission.enabled = true;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f * config.Scale;

            ps.Play();
        }

        private static void ConfigureExplosionParticles(GameObject go, ElementalEffectDefinition definition, ExplosionConfig config)
        {
            var ps = go.GetComponent<ParticleSystem>();
            if (ps == null) ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(definition.PrimaryColor, definition.SecondaryColor);
            main.startLifetime = config.Duration;
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f * config.Radius, 0.5f * config.Radius);
            main.startSpeed = config.Radius * 2;
            main.gravityModifier = definition.GravityModifier;
            main.maxParticles = (int)(100 * SparkConfig.QualityMultiplier);

            var emission = ps.emission;
            emission.enabled = false;

            var burst = new ParticleSystem.Burst(0f, (short)(50 * SparkConfig.QualityMultiplier));
            ps.emission.SetBurst(0, burst);
            emission.enabled = true;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = config.Radius * 0.2f;

            // Add sub-emitter for secondary particles if high quality
            if (SparkConfig.QualityMultiplier >= 1f)
            {
                // TODO: Add secondary particle burst
            }

            ps.Play();
        }

        private static Element GetElementForImpact(ImpactType type)
        {
            return type switch
            {
                ImpactType.FireHit => Element.Fire,
                ImpactType.FrostHit => Element.Frost,
                ImpactType.LightningStrike => Element.Lightning,
                ImpactType.PoisonSplash => Element.Poison,
                ImpactType.SpiritHit => Element.Spirit,
                ImpactType.ShadowHit => Element.Shadow,
                ImpactType.ArcaneHit => Element.Arcane,
                _ => Element.None
            };
        }

        private static float GetScaleForImpact(ImpactType type)
        {
            return type switch
            {
                ImpactType.CriticalHit => 1.5f,
                ImpactType.LightningStrike => 1.2f,
                _ => 1f
            };
        }
    }
}
