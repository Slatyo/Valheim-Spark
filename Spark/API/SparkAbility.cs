using System.Collections.Generic;
using Spark.Core;
using Spark.Internal;
using UnityEngine;

namespace Spark.API
{
    /// <summary>
    /// VFX system for Prime abilities.
    /// Maps ability VFX IDs (like "spark_fireball_cast") to actual effect configurations.
    /// </summary>
    public static class SparkAbility
    {
        private static readonly Dictionary<string, AbilityVFXConfig> VFXRegistry = new Dictionary<string, AbilityVFXConfig>();
        private static bool _initialized;

        /// <summary>
        /// Initialize the ability VFX registry.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            RegisterWarriorVFX();
            RegisterRangerVFX();
            RegisterSorcererVFX();
            RegisterGuardianVFX();
            RegisterCreatureVFX();
            RegisterUtilityVFX();
            RegisterKeystoneVFX();

            Plugin.Log?.LogInfo($"SparkAbility: Registered {VFXRegistry.Count} ability VFX configurations");
        }

        #region VFX Playback

        /// <summary>
        /// Play a VFX at a position (for cast effects, impacts, etc).
        /// </summary>
        public static GameObject PlayAtPosition(string vfxId, Vector3 position, float scale = 1f)
        {
            if (!VFXRegistry.TryGetValue(vfxId, out var config))
            {
                Plugin.Log?.LogDebug($"SparkAbility: Unknown VFX '{vfxId}'");
                return null;
            }

            return CreateEffect(config, position, null, scale);
        }

        /// <summary>
        /// Play a VFX attached to a target.
        /// </summary>
        public static GameObject PlayAttached(string vfxId, GameObject target, float scale = 1f)
        {
            if (target == null) return null;

            if (!VFXRegistry.TryGetValue(vfxId, out var config))
            {
                Plugin.Log?.LogDebug($"SparkAbility: Unknown VFX '{vfxId}'");
                return null;
            }

            return CreateEffect(config, target.transform.position, target.transform, scale);
        }

        /// <summary>
        /// Play a VFX on a character (auto-finds center/chest position).
        /// </summary>
        public static GameObject PlayOnCharacter(string vfxId, Character character, float scale = 1f)
        {
            if (character == null) return null;

            var position = character.GetCenterPoint();
            Transform parent = character.transform;

            if (!VFXRegistry.TryGetValue(vfxId, out var config))
            {
                Plugin.Log?.LogDebug($"SparkAbility: Unknown VFX '{vfxId}'");
                return null;
            }

            return CreateEffect(config, position, parent, scale);
        }

        /// <summary>
        /// Play a directional VFX (like breath attacks, projectiles).
        /// </summary>
        public static GameObject PlayDirectional(string vfxId, Vector3 origin, Vector3 direction, float scale = 1f)
        {
            if (!VFXRegistry.TryGetValue(vfxId, out var config))
            {
                Plugin.Log?.LogDebug($"SparkAbility: Unknown VFX '{vfxId}'");
                return null;
            }

            var effectGo = CreateEffect(config, origin, null, scale);
            if (effectGo != null)
            {
                effectGo.transform.rotation = Quaternion.LookRotation(direction);
            }
            return effectGo;
        }

        #endregion

        #region Effect Creation

        private static GameObject CreateEffect(AbilityVFXConfig config, Vector3 position, Transform parent, float scale)
        {
            var effectGo = new GameObject($"SparkAbilityVFX_{config.Id}");
            effectGo.transform.position = position;
            if (parent != null)
                effectGo.transform.SetParent(parent, true);

            // Create particle system
            var ps = effectGo.AddComponent<ParticleSystem>();
            ConfigureParticleSystem(ps, config, scale);

            // Add light if configured
            if (config.LightIntensity > 0)
            {
                var lightGo = new GameObject("Light");
                lightGo.transform.SetParent(effectGo.transform);
                lightGo.transform.localPosition = Vector3.zero;

                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = config.LightColor;
                light.intensity = config.LightIntensity * scale;
                light.range = config.LightRange * scale;

                // Fade out light with effect
                if (config.Duration > 0)
                {
                    var fader = lightGo.AddComponent<LightFader>();
                    fader.Initialize(config.Duration);
                }
            }

            // Auto-destroy after duration
            if (config.Duration > 0)
            {
                var destroyer = effectGo.AddComponent<TimedDestroyer>();
                destroyer.Initialize(config.Duration + 1f); // Extra time for particles to fade
            }

            ps.Play();
            return effectGo;
        }

        private static void ConfigureParticleSystem(ParticleSystem ps, AbilityVFXConfig config, float scale)
        {
            // Main module
            var main = ps.main;
            main.duration = config.Duration > 0 ? config.Duration : 1f;
            main.loop = config.Loop;
            main.startLifetime = config.ParticleLifetime;
            main.startSpeed = config.StartSpeed * scale;
            main.startSize = config.ParticleSize * scale;
            main.gravityModifier = config.Gravity;
            main.maxParticles = (int)(config.MaxParticles * SparkConfig.QualityMultiplier);

            // Color gradient
            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(config.PrimaryColor, 0f),
                    new GradientColorKey(config.SecondaryColor, 0.5f),
                    new GradientColorKey(config.SecondaryColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.1f),
                    new GradientAlphaKey(1f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLife.color = new ParticleSystem.MinMaxGradient(gradient);

            // Emission
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, config.BurstCount)
            });

            if (config.ContinuousEmission > 0)
            {
                emission.rateOverTime = config.ContinuousEmission * SparkConfig.QualityMultiplier;
            }

            // Shape
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = config.ShapeType;
            shape.radius = config.ShapeRadius * scale;
            shape.angle = config.ShapeAngle;
            shape.arc = config.ShapeArc;

            // Noise for organic look
            if (config.NoiseStrength > 0)
            {
                var noise = ps.noise;
                noise.enabled = true;
                noise.strength = config.NoiseStrength;
                noise.frequency = config.NoiseFrequency;
            }

            // Size over lifetime (shrink or grow)
            if (config.SizeOverLifetime != null)
            {
                var sizeOL = ps.sizeOverLifetime;
                sizeOL.enabled = true;
                sizeOL.size = new ParticleSystem.MinMaxCurve(1f, config.SizeOverLifetime);
            }

            // Renderer
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = config.RenderMode;

            // Use texture if specified
            var material = GetMaterialForConfig(config);
            if (material != null)
            {
                renderer.material = material;
            }
        }

        private static Material GetMaterialForConfig(AbilityVFXConfig config)
        {
            if (!string.IsNullOrEmpty(config.TextureName))
            {
                return TextureLoader.GetMaterial(config.TextureName, config.PrimaryColor);
            }
            return ShaderUtils.CreateParticleMaterial(config.PrimaryColor);
        }

        #endregion

        #region Registration Helpers

        private static void Register(AbilityVFXConfig config)
        {
            VFXRegistry[config.Id] = config;
        }

        #endregion

        #region Warrior VFX

        private static void RegisterWarriorVFX()
        {
            // War Cry - circular shockwave with ring effect
            Register(new AbilityVFXConfig
            {
                Id = "spark_warcry",
                PrimaryColor = new Color(1f, 0.8f, 0.3f),
                SecondaryColor = new Color(1f, 0.5f, 0.1f),
                Duration = 1.5f,
                BurstCount = 60,
                ParticleLifetime = 1.2f,
                ParticleSize = 0.2f,
                StartSpeed = 8f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 0.5f,
                LightIntensity = 1.5f,
                LightColor = new Color(1f, 0.7f, 0.3f),
                LightRange = 8f,
                TextureName = "kenney_circle_05" // Ring shockwave
            });

            // Whirlwind - spinning slash particles
            Register(new AbilityVFXConfig
            {
                Id = "spark_whirlwind",
                PrimaryColor = new Color(0.9f, 0.9f, 1f),
                SecondaryColor = new Color(0.6f, 0.6f, 0.7f),
                Duration = 2f,
                Loop = true,
                ContinuousEmission = 80f,
                ParticleLifetime = 0.4f,
                ParticleSize = 0.15f,
                StartSpeed = 4f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 2f,
                NoiseStrength = 0.3f,
                NoiseFrequency = 3f,
                TextureName = "kenney_twirl_02" // Spinning effect
            });

            // Shield Bash - impact flash with muzzle burst
            Register(new AbilityVFXConfig
            {
                Id = "spark_impact_blunt",
                PrimaryColor = new Color(1f, 0.9f, 0.7f),
                SecondaryColor = new Color(0.8f, 0.6f, 0.3f),
                Duration = 0.5f,
                BurstCount = 25,
                ParticleLifetime = 0.3f,
                ParticleSize = 0.3f,
                StartSpeed = 6f,
                ShapeType = ParticleSystemShapeType.Hemisphere,
                ShapeRadius = 0.3f,
                LightIntensity = 2f,
                LightColor = new Color(1f, 0.9f, 0.6f),
                LightRange = 4f,
                TextureName = "kenney_muzzle_03" // Impact flash
            });

            // Berserker Rage - red fire aura
            Register(new AbilityVFXConfig
            {
                Id = "spark_rage",
                PrimaryColor = new Color(1f, 0.2f, 0.1f),
                SecondaryColor = new Color(0.8f, 0.1f, 0.05f),
                Duration = 20f,
                Loop = true,
                ContinuousEmission = 35f,
                ParticleLifetime = 0.8f,
                ParticleSize = 0.2f,
                StartSpeed = 1.5f,
                Gravity = -0.4f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.8f,
                NoiseStrength = 0.2f,
                LightIntensity = 0.8f,
                LightColor = new Color(1f, 0.3f, 0.1f),
                LightRange = 5f,
                TextureName = "kenney_flame_02" // Flame particles
            });

            // Ground Slam - earth/dirt burst
            Register(new AbilityVFXConfig
            {
                Id = "spark_groundslam",
                PrimaryColor = new Color(0.6f, 0.5f, 0.3f),
                SecondaryColor = new Color(0.4f, 0.3f, 0.2f),
                Duration = 1f,
                BurstCount = 80,
                ParticleLifetime = 1f,
                ParticleSize = 0.3f,
                StartSpeed = 10f,
                Gravity = 1.5f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 0.5f,
                LightIntensity = 1f,
                LightColor = new Color(0.8f, 0.6f, 0.3f),
                LightRange = 6f,
                TextureName = "kenney_dirt_02" // Dirt chunks
            });

            // Execute - blood slash
            Register(new AbilityVFXConfig
            {
                Id = "spark_execute",
                PrimaryColor = new Color(0.8f, 0.1f, 0.1f),
                SecondaryColor = new Color(0.5f, 0.05f, 0.05f),
                Duration = 0.8f,
                BurstCount = 40,
                ParticleLifetime = 0.5f,
                ParticleSize = 0.25f,
                StartSpeed = 8f,
                Gravity = 0.5f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 25f,
                LightIntensity = 1.2f,
                LightColor = new Color(1f, 0.2f, 0.1f),
                LightRange = 4f,
                TextureName = "kenney_slash_01" // Slash arc
            });

            // Gladiator's Glory - victory sparkles with stars
            Register(new AbilityVFXConfig
            {
                Id = "spark_gladiator",
                PrimaryColor = new Color(1f, 0.85f, 0.4f),
                SecondaryColor = new Color(1f, 0.6f, 0.2f),
                Duration = 2f,
                BurstCount = 50,
                ParticleLifetime = 1.5f,
                ParticleSize = 0.12f,
                StartSpeed = 3f,
                Gravity = -0.2f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1f,
                TextureName = "kenney_star_04" // 4-pointed star
            });

            // Iron Skin - metallic shimmer with light particles
            Register(new AbilityVFXConfig
            {
                Id = "spark_iron_skin",
                PrimaryColor = new Color(0.75f, 0.75f, 0.8f),
                SecondaryColor = new Color(0.5f, 0.5f, 0.6f),
                Duration = 1f,
                BurstCount = 40,
                ParticleLifetime = 1f,
                ParticleSize = 0.12f,
                StartSpeed = 0.5f,
                Gravity = 0.1f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1.2f,
                TextureName = "kenney_light_02" // Soft light glow
            });

            // Juggernaut - heavy stomp with dust
            Register(new AbilityVFXConfig
            {
                Id = "spark_juggernaut",
                PrimaryColor = new Color(0.5f, 0.4f, 0.3f),
                SecondaryColor = new Color(0.3f, 0.25f, 0.2f),
                Duration = 1.5f,
                BurstCount = 100,
                ParticleLifetime = 1.2f,
                ParticleSize = 0.35f,
                StartSpeed = 5f,
                Gravity = 2f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 1f,
                LightIntensity = 0.5f,
                LightRange = 5f,
                TextureName = "kenney_smoke_04" // Dust cloud
            });
        }

        #endregion

        #region Ranger VFX

        private static void RegisterRangerVFX()
        {
            // Power Shot - charged arrow with trace
            Register(new AbilityVFXConfig
            {
                Id = "spark_powershot",
                PrimaryColor = new Color(0.3f, 0.8f, 1f),
                SecondaryColor = new Color(0.1f, 0.5f, 0.8f),
                Duration = 0.5f,
                BurstCount = 20,
                ParticleLifetime = 0.3f,
                ParticleSize = 0.12f,
                StartSpeed = 15f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 5f,
                LightIntensity = 1.5f,
                LightColor = new Color(0.3f, 0.7f, 1f),
                LightRange = 3f,
                TextureName = "kenney_trace_05" // Arrow trace
            });

            // Arrow Impact - spark burst
            Register(new AbilityVFXConfig
            {
                Id = "spark_arrow_impact",
                PrimaryColor = new Color(1f, 0.9f, 0.7f),
                SecondaryColor = new Color(0.8f, 0.7f, 0.5f),
                Duration = 0.4f,
                BurstCount = 15,
                ParticleLifetime = 0.3f,
                ParticleSize = 0.1f,
                StartSpeed = 4f,
                ShapeType = ParticleSystemShapeType.Hemisphere,
                ShapeRadius = 0.2f,
                TextureName = "kenney_spark_03" // Small sparks
            });

            // Multishot - spread arrow traces
            Register(new AbilityVFXConfig
            {
                Id = "spark_multishot",
                PrimaryColor = new Color(0.9f, 0.9f, 1f),
                SecondaryColor = new Color(0.6f, 0.6f, 0.8f),
                Duration = 0.3f,
                BurstCount = 30,
                ParticleLifetime = 0.25f,
                ParticleSize = 0.08f,
                StartSpeed = 20f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 30f,
                TextureName = "kenney_trace_02" // Light traces
            });

            // Evasive Roll - dust/smoke trail
            Register(new AbilityVFXConfig
            {
                Id = "spark_dodge",
                PrimaryColor = new Color(0.7f, 0.65f, 0.55f),
                SecondaryColor = new Color(0.5f, 0.45f, 0.35f),
                Duration = 0.8f,
                BurstCount = 30,
                ParticleLifetime = 0.6f,
                ParticleSize = 0.25f,
                StartSpeed = 2f,
                Gravity = -0.1f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 0.5f,
                NoiseStrength = 0.2f,
                TextureName = "kenney_smoke_07" // Wispy smoke
            });

            // Hunter's Mark - target indicator with symbol
            Register(new AbilityVFXConfig
            {
                Id = "spark_huntersmark",
                PrimaryColor = new Color(1f, 0.4f, 0.1f),
                SecondaryColor = new Color(1f, 0.2f, 0.05f),
                Duration = 15f,
                Loop = true,
                ContinuousEmission = 15f,
                ParticleLifetime = 1.5f,
                ParticleSize = 0.2f,
                StartSpeed = 0.5f,
                Gravity = -0.1f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 0.3f,
                LightIntensity = 0.6f,
                LightColor = new Color(1f, 0.4f, 0.1f),
                LightRange = 3f,
                TextureName = "kenney_symbol_01" // Target symbol
            });

            // Poison Arrow - green trace
            Register(new AbilityVFXConfig
            {
                Id = "spark_poisonarrow",
                PrimaryColor = new Color(0.3f, 0.9f, 0.2f),
                SecondaryColor = new Color(0.2f, 0.6f, 0.1f),
                Duration = 0.5f,
                BurstCount = 15,
                ParticleLifetime = 0.4f,
                ParticleSize = 0.1f,
                StartSpeed = 12f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 8f,
                TextureName = "kenney_trace_04" // Poison trail
            });

            // Poison Cloud (impact) - smoke cloud
            Register(new AbilityVFXConfig
            {
                Id = "spark_poison_cloud",
                PrimaryColor = new Color(0.3f, 0.8f, 0.2f, 0.6f),
                SecondaryColor = new Color(0.2f, 0.5f, 0.1f, 0.3f),
                Duration = 3f,
                Loop = true,
                ContinuousEmission = 20f,
                ParticleLifetime = 2f,
                ParticleSize = 0.6f,
                StartSpeed = 0.5f,
                Gravity = -0.05f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1.5f,
                NoiseStrength = 0.15f,
                TextureName = "kenney_smoke_10" // Thick cloud
            });

            // Trap placement - dirt puff
            Register(new AbilityVFXConfig
            {
                Id = "spark_trap_place",
                PrimaryColor = new Color(0.6f, 0.5f, 0.3f),
                SecondaryColor = new Color(0.4f, 0.35f, 0.2f),
                Duration = 0.5f,
                BurstCount = 20,
                ParticleLifetime = 0.4f,
                ParticleSize = 0.12f,
                StartSpeed = 2f,
                Gravity = 0.5f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 0.3f,
                TextureName = "kenney_dirt_01" // Dirt particles
            });

            // Trap trigger - spark explosion
            Register(new AbilityVFXConfig
            {
                Id = "spark_trap_trigger",
                PrimaryColor = new Color(0.9f, 0.7f, 0.3f),
                SecondaryColor = new Color(0.7f, 0.5f, 0.2f),
                Duration = 0.6f,
                BurstCount = 35,
                ParticleLifetime = 0.5f,
                ParticleSize = 0.15f,
                StartSpeed = 5f,
                Gravity = 0.8f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.5f,
                TextureName = "kenney_spark_05" // Metal sparks
            });

            // Shadow Strike - dark slash arc
            Register(new AbilityVFXConfig
            {
                Id = "spark_shadow_strike",
                PrimaryColor = new Color(0.2f, 0.1f, 0.3f),
                SecondaryColor = new Color(0.1f, 0.05f, 0.15f),
                Duration = 0.6f,
                BurstCount = 30,
                ParticleLifetime = 0.4f,
                ParticleSize = 0.2f,
                StartSpeed = 6f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 20f,
                LightIntensity = 0.5f,
                LightColor = new Color(0.3f, 0.1f, 0.4f),
                LightRange = 3f,
                TextureName = "kenney_slash_03" // Shadow slash
            });
        }

        #endregion

        #region Sorcerer VFX

        private static void RegisterSorcererVFX()
        {
            // Fireball cast - flame burst
            Register(new AbilityVFXConfig
            {
                Id = "spark_fireball_cast",
                PrimaryColor = new Color(1f, 0.6f, 0.1f),
                SecondaryColor = new Color(1f, 0.3f, 0.05f),
                Duration = 0.5f,
                BurstCount = 30,
                ParticleLifetime = 0.4f,
                ParticleSize = 0.15f,
                StartSpeed = 2f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.3f,
                LightIntensity = 1.5f,
                LightColor = new Color(1f, 0.5f, 0.1f),
                LightRange = 4f,
                TextureName = "kenney_flame_03" // Flame texture
            });

            // Fire explosion - massive fire burst
            Register(new AbilityVFXConfig
            {
                Id = "spark_fire_explosion",
                PrimaryColor = new Color(1f, 0.7f, 0.2f),
                SecondaryColor = new Color(1f, 0.3f, 0.05f),
                Duration = 1.5f,
                BurstCount = 100,
                ParticleLifetime = 1f,
                ParticleSize = 0.4f,
                StartSpeed = 8f,
                Gravity = -0.3f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.5f,
                NoiseStrength = 0.2f,
                LightIntensity = 3f,
                LightColor = new Color(1f, 0.6f, 0.2f),
                LightRange = 10f,
                TextureName = "kenney_fire_02" // Fire explosion
            });

            // Frost Nova - ice crystal burst
            Register(new AbilityVFXConfig
            {
                Id = "spark_frost_nova",
                PrimaryColor = new Color(0.7f, 0.9f, 1f),
                SecondaryColor = new Color(0.4f, 0.7f, 1f),
                Duration = 1.5f,
                BurstCount = 80,
                ParticleLifetime = 1.2f,
                ParticleSize = 0.18f,
                StartSpeed = 10f,
                Gravity = 0.1f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 0.5f,
                LightIntensity = 2f,
                LightColor = new Color(0.6f, 0.8f, 1f),
                LightRange = 8f,
                TextureName = "kenney_star_06" // Crystal-like star
            });

            // Lightning Bolt - electric sparks
            Register(new AbilityVFXConfig
            {
                Id = "spark_lightning_bolt",
                PrimaryColor = new Color(0.8f, 0.9f, 1f),
                SecondaryColor = new Color(0.5f, 0.7f, 1f),
                Duration = 0.3f,
                BurstCount = 40,
                ParticleLifetime = 0.15f,
                ParticleSize = 0.1f,
                StartSpeed = 25f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 3f,
                NoiseStrength = 0.5f,
                NoiseFrequency = 20f,
                LightIntensity = 3f,
                LightColor = new Color(0.8f, 0.9f, 1f),
                LightRange = 8f,
                TextureName = "kenney_spark_01" // Electric spark
            });

            // Lightning Strike (impact) - flash burst
            Register(new AbilityVFXConfig
            {
                Id = "spark_lightning_strike",
                PrimaryColor = new Color(1f, 1f, 1f),
                SecondaryColor = new Color(0.6f, 0.8f, 1f),
                Duration = 0.5f,
                BurstCount = 60,
                ParticleLifetime = 0.2f,
                ParticleSize = 0.15f,
                StartSpeed = 12f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.3f,
                NoiseStrength = 0.6f,
                NoiseFrequency = 15f,
                LightIntensity = 4f,
                LightColor = new Color(0.9f, 0.95f, 1f),
                LightRange = 10f,
                TextureName = "kenney_flare_01" // Bright flare
            });

            // Chain Lightning - arc sparks
            Register(new AbilityVFXConfig
            {
                Id = "spark_chain_lightning",
                PrimaryColor = new Color(0.7f, 0.85f, 1f),
                SecondaryColor = new Color(0.4f, 0.6f, 1f),
                Duration = 0.8f,
                BurstCount = 30,
                ParticleLifetime = 0.3f,
                ParticleSize = 0.08f,
                StartSpeed = 20f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 5f,
                NoiseStrength = 0.4f,
                NoiseFrequency = 12f,
                TextureName = "kenney_spark_02" // Chain sparks
            });

            // Mana Shield - magic circle
            Register(new AbilityVFXConfig
            {
                Id = "spark_mana_shield",
                PrimaryColor = new Color(0.4f, 0.6f, 1f, 0.5f),
                SecondaryColor = new Color(0.2f, 0.4f, 0.9f, 0.3f),
                Duration = 10f,
                Loop = true,
                ContinuousEmission = 25f,
                ParticleLifetime = 1.5f,
                ParticleSize = 0.12f,
                StartSpeed = 0.3f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1.2f,
                NoiseStrength = 0.1f,
                LightIntensity = 0.4f,
                LightColor = new Color(0.4f, 0.5f, 1f),
                LightRange = 4f,
                TextureName = "kenney_magic_02" // Magic particles
            });

            // Meteor channel - falling fire
            Register(new AbilityVFXConfig
            {
                Id = "spark_meteor_channel",
                PrimaryColor = new Color(1f, 0.5f, 0.1f),
                SecondaryColor = new Color(1f, 0.2f, 0.05f),
                Duration = 2f,
                Loop = true,
                ContinuousEmission = 40f,
                ParticleLifetime = 0.8f,
                ParticleSize = 0.2f,
                StartSpeed = 5f,
                Gravity = 1f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 15f,
                LightIntensity = 1.5f,
                LightColor = new Color(1f, 0.4f, 0.1f),
                LightRange = 6f,
                TextureName = "kenney_flame_05" // Intense flame
            });

            // Meteor impact - massive fire explosion
            Register(new AbilityVFXConfig
            {
                Id = "spark_meteor_impact",
                PrimaryColor = new Color(1f, 0.6f, 0.1f),
                SecondaryColor = new Color(1f, 0.2f, 0.05f),
                Duration = 2f,
                BurstCount = 200,
                ParticleLifetime = 1.5f,
                ParticleSize = 0.5f,
                StartSpeed = 15f,
                Gravity = 1.5f,
                ShapeType = ParticleSystemShapeType.Hemisphere,
                ShapeRadius = 1f,
                NoiseStrength = 0.3f,
                LightIntensity = 5f,
                LightColor = new Color(1f, 0.5f, 0.1f),
                LightRange = 15f,
                TextureName = "kenney_fire_01" // Fire burst
            });

            // Teleport - magic swirl
            Register(new AbilityVFXConfig
            {
                Id = "spark_teleport",
                PrimaryColor = new Color(0.6f, 0.3f, 1f),
                SecondaryColor = new Color(0.9f, 0.6f, 1f),
                Duration = 0.8f,
                BurstCount = 50,
                ParticleLifetime = 0.6f,
                ParticleSize = 0.12f,
                StartSpeed = 3f,
                Gravity = 0.5f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 0.5f,
                LightIntensity = 2f,
                LightColor = new Color(0.6f, 0.4f, 1f),
                LightRange = 5f,
                TextureName = "kenney_magic_04" // Magic swirl
            });
        }

        #endregion

        #region Guardian VFX

        private static void RegisterGuardianVFX()
        {
            // Healing Touch / general heal - green stars
            Register(new AbilityVFXConfig
            {
                Id = "spark_heal",
                PrimaryColor = new Color(0.4f, 1f, 0.5f),
                SecondaryColor = new Color(0.2f, 0.9f, 0.4f),
                Duration = 1.5f,
                BurstCount = 40,
                ParticleLifetime = 1.2f,
                ParticleSize = 0.15f,
                StartSpeed = 1f,
                Gravity = -0.3f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.8f,
                LightIntensity = 1f,
                LightColor = new Color(0.4f, 1f, 0.5f),
                LightRange = 5f,
                TextureName = "kenney_star_05" // Healing star
            });

            // Healing Aura - soft green glow
            Register(new AbilityVFXConfig
            {
                Id = "spark_healing_aura",
                PrimaryColor = new Color(0.5f, 1f, 0.6f, 0.6f),
                SecondaryColor = new Color(0.3f, 0.9f, 0.5f, 0.3f),
                Duration = 10f,
                Loop = true,
                ContinuousEmission = 20f,
                ParticleLifetime = 2f,
                ParticleSize = 0.18f,
                StartSpeed = 0.5f,
                Gravity = -0.1f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 5f,
                LightIntensity = 0.6f,
                LightColor = new Color(0.4f, 1f, 0.5f),
                LightRange = 8f,
                TextureName = "kenney_light_01" // Soft light
            });

            // Divine Shield - golden circle
            Register(new AbilityVFXConfig
            {
                Id = "spark_divine_shield",
                PrimaryColor = new Color(1f, 0.95f, 0.7f),
                SecondaryColor = new Color(1f, 0.85f, 0.5f),
                Duration = 8f,
                Loop = true,
                ContinuousEmission = 30f,
                ParticleLifetime = 1f,
                ParticleSize = 0.12f,
                StartSpeed = 0.2f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1.5f,
                LightIntensity = 1f,
                LightColor = new Color(1f, 0.9f, 0.6f),
                LightRange = 5f,
                TextureName = "kenney_circle_03" // Shield circle
            });

            // Taunt - aggro pulse ring
            Register(new AbilityVFXConfig
            {
                Id = "spark_taunt",
                PrimaryColor = new Color(1f, 0.3f, 0.2f),
                SecondaryColor = new Color(0.9f, 0.2f, 0.1f),
                Duration = 1f,
                BurstCount = 60,
                ParticleLifetime = 0.8f,
                ParticleSize = 0.25f,
                StartSpeed = 6f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 0.5f,
                LightIntensity = 1.5f,
                LightColor = new Color(1f, 0.3f, 0.1f),
                LightRange = 10f,
                TextureName = "kenney_circle_04" // Aggro ring
            });

            // Consecration - holy ground with symbols
            Register(new AbilityVFXConfig
            {
                Id = "spark_consecration",
                PrimaryColor = new Color(1f, 0.95f, 0.7f),
                SecondaryColor = new Color(1f, 0.85f, 0.5f),
                Duration = 10f,
                Loop = true,
                ContinuousEmission = 25f,
                ParticleLifetime = 1.5f,
                ParticleSize = 0.15f,
                StartSpeed = 0.3f,
                Gravity = -0.15f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 4f,
                LightIntensity = 0.8f,
                LightColor = new Color(1f, 0.9f, 0.6f),
                LightRange = 8f,
                TextureName = "kenney_symbol_02" // Holy symbol
            });

            // Resurrection - rising light
            Register(new AbilityVFXConfig
            {
                Id = "spark_resurrection",
                PrimaryColor = new Color(1f, 1f, 0.8f),
                SecondaryColor = new Color(1f, 0.95f, 0.6f),
                Duration = 5f,
                Loop = true,
                ContinuousEmission = 50f,
                ParticleLifetime = 2f,
                ParticleSize = 0.18f,
                StartSpeed = 2f,
                Gravity = -0.5f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 1f,
                LightIntensity = 2f,
                LightColor = new Color(1f, 1f, 0.8f),
                LightRange = 10f,
                TextureName = "kenney_star_08" // Rising stars
            });

            // Mass Heal - burst of healing stars
            Register(new AbilityVFXConfig
            {
                Id = "spark_mass_heal",
                PrimaryColor = new Color(0.5f, 1f, 0.6f),
                SecondaryColor = new Color(0.3f, 0.9f, 0.4f),
                Duration = 2f,
                BurstCount = 100,
                ParticleLifetime = 1.5f,
                ParticleSize = 0.18f,
                StartSpeed = 8f,
                Gravity = -0.2f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 1f,
                LightIntensity = 2f,
                LightColor = new Color(0.4f, 1f, 0.5f),
                LightRange = 12f,
                TextureName = "kenney_star_03" // Heal burst stars
            });
        }

        #endregion

        #region Creature VFX

        private static void RegisterCreatureVFX()
        {
            // Frost Breath - icy cone
            Register(new AbilityVFXConfig
            {
                Id = "spark_frost_breath",
                PrimaryColor = new Color(0.8f, 0.95f, 1f),
                SecondaryColor = new Color(0.5f, 0.8f, 1f),
                Duration = 2f,
                Loop = true,
                ContinuousEmission = 80f,
                ParticleLifetime = 0.8f,
                ParticleSize = 0.25f,
                StartSpeed = 12f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 25f,
                NoiseStrength = 0.15f,
                LightIntensity = 1f,
                LightColor = new Color(0.6f, 0.8f, 1f),
                LightRange = 8f,
                TextureName = "kenney_smoke_06" // Misty breath
            });

            // Fire Breath - flame cone
            Register(new AbilityVFXConfig
            {
                Id = "spark_fire_breath",
                PrimaryColor = new Color(1f, 0.6f, 0.1f),
                SecondaryColor = new Color(1f, 0.2f, 0.05f),
                Duration = 2f,
                Loop = true,
                ContinuousEmission = 100f,
                ParticleLifetime = 0.6f,
                ParticleSize = 0.3f,
                StartSpeed = 15f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 20f,
                NoiseStrength = 0.2f,
                LightIntensity = 2f,
                LightColor = new Color(1f, 0.5f, 0.1f),
                LightRange = 10f,
                TextureName = "kenney_flame_06" // Intense flame
            });

            // Enrage - angry red aura
            Register(new AbilityVFXConfig
            {
                Id = "spark_enrage",
                PrimaryColor = new Color(1f, 0.2f, 0.1f),
                SecondaryColor = new Color(0.8f, 0.1f, 0.05f),
                Duration = 15f,
                Loop = true,
                ContinuousEmission = 40f,
                ParticleLifetime = 1f,
                ParticleSize = 0.25f,
                StartSpeed = 2f,
                Gravity = -0.4f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1.5f,
                NoiseStrength = 0.25f,
                LightIntensity = 1f,
                LightColor = new Color(1f, 0.3f, 0.1f),
                LightRange = 6f,
                TextureName = "kenney_flame_01" // Rage flames
            });

            // Summon - dark portal magic
            Register(new AbilityVFXConfig
            {
                Id = "spark_summon",
                PrimaryColor = new Color(0.3f, 0.1f, 0.4f),
                SecondaryColor = new Color(0.5f, 0.2f, 0.6f),
                Duration = 2f,
                BurstCount = 60,
                ParticleLifetime = 1.5f,
                ParticleSize = 0.18f,
                StartSpeed = 3f,
                Gravity = -0.5f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 2f,
                LightIntensity = 1.5f,
                LightColor = new Color(0.4f, 0.2f, 0.5f),
                LightRange = 8f,
                TextureName = "kenney_magic_05" // Dark magic
            });

            // Poison Spit - toxic glob
            Register(new AbilityVFXConfig
            {
                Id = "spark_poison_spit",
                PrimaryColor = new Color(0.3f, 0.9f, 0.2f),
                SecondaryColor = new Color(0.2f, 0.6f, 0.1f),
                Duration = 0.5f,
                BurstCount = 20,
                ParticleLifetime = 0.4f,
                ParticleSize = 0.18f,
                StartSpeed = 15f,
                Gravity = 0.3f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 10f,
                TextureName = "kenney_smoke_05" // Toxic cloud
            });

            // Ice Spikes - frost burst
            Register(new AbilityVFXConfig
            {
                Id = "spark_ice_spikes",
                PrimaryColor = new Color(0.7f, 0.9f, 1f),
                SecondaryColor = new Color(0.4f, 0.7f, 1f),
                Duration = 1f,
                BurstCount = 50,
                ParticleLifetime = 0.8f,
                ParticleSize = 0.25f,
                StartSpeed = 8f,
                Gravity = 0.2f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 2f,
                LightIntensity = 1.5f,
                LightColor = new Color(0.6f, 0.8f, 1f),
                LightRange = 6f,
                TextureName = "kenney_star_07" // Ice crystal star
            });

            // Ground Pound - massive earth burst
            Register(new AbilityVFXConfig
            {
                Id = "spark_ground_pound",
                PrimaryColor = new Color(0.5f, 0.4f, 0.3f),
                SecondaryColor = new Color(0.3f, 0.25f, 0.2f),
                Duration = 1.5f,
                BurstCount = 120,
                ParticleLifetime = 1.2f,
                ParticleSize = 0.4f,
                StartSpeed = 12f,
                Gravity = 2f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 1f,
                LightIntensity = 1f,
                LightRange = 10f,
                TextureName = "kenney_dirt_03" // Large dirt chunks
            });

            // Roar - intimidation wave
            Register(new AbilityVFXConfig
            {
                Id = "spark_roar",
                PrimaryColor = new Color(0.9f, 0.85f, 0.7f),
                SecondaryColor = new Color(0.7f, 0.6f, 0.4f),
                Duration = 1.5f,
                BurstCount = 80,
                ParticleLifetime = 1f,
                ParticleSize = 0.25f,
                StartSpeed = 10f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 60f,
                NoiseStrength = 0.2f,
                TextureName = "kenney_smoke_03" // Breath/roar wave
            });

            // Spirit Drain - soul absorption
            Register(new AbilityVFXConfig
            {
                Id = "spark_spirit_drain",
                PrimaryColor = new Color(0.7f, 0.5f, 0.9f),
                SecondaryColor = new Color(0.5f, 0.3f, 0.7f),
                Duration = 2f,
                Loop = true,
                ContinuousEmission = 30f,
                ParticleLifetime = 0.8f,
                ParticleSize = 0.12f,
                StartSpeed = -5f, // Inward
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 3f,
                LightIntensity = 0.8f,
                LightColor = new Color(0.6f, 0.4f, 0.8f),
                LightRange = 5f,
                TextureName = "kenney_magic_03" // Spirit magic
            });
        }

        #endregion

        #region Utility VFX

        private static void RegisterUtilityVFX()
        {
            // Sprint - wind trail
            Register(new AbilityVFXConfig
            {
                Id = "spark_sprint",
                PrimaryColor = new Color(0.9f, 0.95f, 1f),
                SecondaryColor = new Color(0.6f, 0.7f, 0.8f),
                Duration = 5f,
                Loop = true,
                ContinuousEmission = 20f,
                ParticleLifetime = 0.4f,
                ParticleSize = 0.08f,
                StartSpeed = -2f,
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 15f,
                TextureName = "kenney_trace_06" // Speed trail
            });

            // Second Wind - refreshing breeze
            Register(new AbilityVFXConfig
            {
                Id = "spark_second_wind",
                PrimaryColor = new Color(0.8f, 0.95f, 1f),
                SecondaryColor = new Color(0.5f, 0.8f, 0.9f),
                Duration = 2f,
                BurstCount = 30,
                ParticleLifetime = 1f,
                ParticleSize = 0.1f,
                StartSpeed = 2f,
                Gravity = -0.2f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.8f,
                TextureName = "kenney_twirl_01" // Swirling wind
            });

            // Battle Focus - concentration glow
            Register(new AbilityVFXConfig
            {
                Id = "spark_focus",
                PrimaryColor = new Color(1f, 0.9f, 0.5f),
                SecondaryColor = new Color(1f, 0.7f, 0.3f),
                Duration = 10f,
                Loop = true,
                ContinuousEmission = 15f,
                ParticleLifetime = 1f,
                ParticleSize = 0.06f,
                StartSpeed = 0.5f,
                Gravity = -0.1f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 0.5f,
                TextureName = "kenney_star_01" // Focus star
            });

            // Fortify - stone armor particles
            Register(new AbilityVFXConfig
            {
                Id = "spark_fortify",
                PrimaryColor = new Color(0.6f, 0.55f, 0.5f),
                SecondaryColor = new Color(0.4f, 0.38f, 0.35f),
                Duration = 8f,
                Loop = true,
                ContinuousEmission = 20f,
                ParticleLifetime = 1.2f,
                ParticleSize = 0.12f,
                StartSpeed = 0.2f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1.2f,
                TextureName = "kenney_circle_01" // Stone shield
            });

            // Life Tap - health to mana conversion
            Register(new AbilityVFXConfig
            {
                Id = "spark_life_tap",
                PrimaryColor = new Color(0.8f, 0.2f, 0.2f),
                SecondaryColor = new Color(0.3f, 0.5f, 0.9f),
                Duration = 1f,
                BurstCount = 25,
                ParticleLifetime = 0.6f,
                ParticleSize = 0.1f,
                StartSpeed = 2f,
                Gravity = -0.2f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.5f,
                TextureName = "kenney_symbol_01" // Heart symbol
            });

            // Bloodlust - frenzy aura
            Register(new AbilityVFXConfig
            {
                Id = "spark_bloodlust",
                PrimaryColor = new Color(0.9f, 0.15f, 0.1f),
                SecondaryColor = new Color(0.6f, 0.1f, 0.08f),
                Duration = 12f,
                Loop = true,
                ContinuousEmission = 25f,
                ParticleLifetime = 0.8f,
                ParticleSize = 0.08f,
                StartSpeed = 1f,
                Gravity = -0.15f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.8f,
                LightIntensity = 0.5f,
                LightColor = new Color(0.9f, 0.2f, 0.1f),
                LightRange = 3f,
                TextureName = "kenney_slash_02" // Blood slash
            });

            // Potion drink effect - magical bubbles
            Register(new AbilityVFXConfig
            {
                Id = "spark_potion_drink",
                PrimaryColor = new Color(0.8f, 0.6f, 1f),
                SecondaryColor = new Color(0.6f, 0.4f, 0.9f),
                Duration = 1f,
                BurstCount = 20,
                ParticleLifetime = 0.8f,
                ParticleSize = 0.08f,
                StartSpeed = 1.5f,
                Gravity = -0.3f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.3f,
                TextureName = "kenney_circle_02" // Potion bubble
            });

            // Scroll use - arcane runes
            Register(new AbilityVFXConfig
            {
                Id = "spark_scroll_use",
                PrimaryColor = new Color(1f, 0.95f, 0.8f),
                SecondaryColor = new Color(0.9f, 0.8f, 0.5f),
                Duration = 1.5f,
                BurstCount = 30,
                ParticleLifetime = 1f,
                ParticleSize = 0.1f,
                StartSpeed = 2f,
                Gravity = -0.2f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.5f,
                LightIntensity = 1f,
                LightColor = new Color(1f, 0.9f, 0.7f),
                LightRange = 4f,
                TextureName = "kenney_magic_01" // Scroll magic
            });

            // Mushroom eat - nature particles
            Register(new AbilityVFXConfig
            {
                Id = "spark_mushroom_eat",
                PrimaryColor = new Color(0.9f, 0.4f, 0.3f),
                SecondaryColor = new Color(0.7f, 0.3f, 0.2f),
                Duration = 0.8f,
                BurstCount = 15,
                ParticleLifetime = 0.6f,
                ParticleSize = 0.06f,
                StartSpeed = 1f,
                Gravity = -0.1f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.2f,
                TextureName = "kenney_spark_06" // Spore particles
            });
        }

        #endregion

        #region Keystone VFX

        private static void RegisterKeystoneVFX()
        {
            // Deadeye - precision targeting aura
            Register(new AbilityVFXConfig
            {
                Id = "spark_deadeye",
                PrimaryColor = new Color(0.9f, 0.3f, 0.1f),
                SecondaryColor = new Color(1f, 0.5f, 0.2f),
                Duration = -1f, // Permanent
                Loop = true,
                ContinuousEmission = 10f,
                ParticleLifetime = 1.5f,
                ParticleSize = 0.05f,
                StartSpeed = 0.3f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 0.3f,
                TextureName = "kenney_star_02" // Targeting star
            });

            // Wind Walker - speed trails
            Register(new AbilityVFXConfig
            {
                Id = "spark_windwalker",
                PrimaryColor = new Color(0.8f, 0.95f, 1f),
                SecondaryColor = new Color(0.5f, 0.8f, 0.9f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 30f,
                ParticleLifetime = 0.3f,
                ParticleSize = 0.06f,
                StartSpeed = -3f, // Behind
                ShapeType = ParticleSystemShapeType.Cone,
                ShapeAngle = 10f,
                TextureName = "kenney_trace_01" // Wind trail
            });

            // Elemental Rotation - cycling elemental aura
            Register(new AbilityVFXConfig
            {
                Id = "spark_elemental_rotation",
                PrimaryColor = new Color(1f, 0.5f, 0.2f), // Fire
                SecondaryColor = new Color(0.5f, 0.8f, 1f), // Frost
                Duration = 5f,
                Loop = true,
                ContinuousEmission = 20f,
                ParticleLifetime = 1f,
                ParticleSize = 0.08f,
                StartSpeed = 0.5f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 0.8f,
                TextureName = "kenney_twirl_03" // Elemental swirl
            });

            // Gladiator's Glory - golden warrior aura
            Register(new AbilityVFXConfig
            {
                Id = "spark_gladiators_glory",
                PrimaryColor = new Color(1f, 0.85f, 0.4f),
                SecondaryColor = new Color(1f, 0.7f, 0.2f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 15f,
                ParticleLifetime = 1.2f,
                ParticleSize = 0.1f,
                StartSpeed = 0.8f,
                Gravity = -0.2f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1f,
                LightIntensity = 0.8f,
                LightColor = new Color(1f, 0.8f, 0.3f),
                LightRange = 4f,
                TextureName = "kenney_star_09" // Glory star
            });

            // Iron Skin - metallic shield particles
            Register(new AbilityVFXConfig
            {
                Id = "spark_iron_skin",
                PrimaryColor = new Color(0.7f, 0.7f, 0.75f),
                SecondaryColor = new Color(0.5f, 0.5f, 0.55f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 12f,
                ParticleLifetime = 1f,
                ParticleSize = 0.08f,
                StartSpeed = 0.2f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1.2f,
                TextureName = "kenney_circle_01" // Metal shield
            });

            // Juggernaut - unstoppable force
            Register(new AbilityVFXConfig
            {
                Id = "spark_juggernaut",
                PrimaryColor = new Color(0.8f, 0.3f, 0.1f),
                SecondaryColor = new Color(0.5f, 0.2f, 0.1f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 25f,
                ParticleLifetime = 0.6f,
                ParticleSize = 0.15f,
                StartSpeed = 1.5f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.8f,
                LightIntensity = 0.6f,
                LightColor = new Color(0.9f, 0.4f, 0.1f),
                LightRange = 3f,
                TextureName = "kenney_muzzle_02" // Impact force
            });

            // Warlord - command presence
            Register(new AbilityVFXConfig
            {
                Id = "spark_warlord",
                PrimaryColor = new Color(0.9f, 0.2f, 0.15f),
                SecondaryColor = new Color(0.6f, 0.1f, 0.1f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 18f,
                ParticleLifetime = 1.5f,
                ParticleSize = 0.12f,
                StartSpeed = 0.5f,
                Gravity = -0.1f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 2f,
                LightIntensity = 0.5f,
                LightColor = new Color(0.9f, 0.2f, 0.1f),
                LightRange = 5f,
                TextureName = "kenney_scorch_02" // War banner
            });

            // Sniper - precision focus
            Register(new AbilityVFXConfig
            {
                Id = "spark_sniper",
                PrimaryColor = new Color(0.2f, 0.8f, 0.3f),
                SecondaryColor = new Color(0.1f, 0.6f, 0.2f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 8f,
                ParticleLifetime = 2f,
                ParticleSize = 0.04f,
                StartSpeed = 0.2f,
                ShapeType = ParticleSystemShapeType.Circle,
                ShapeRadius = 0.2f,
                TextureName = "kenney_spark_04" // Targeting dot
            });

            // Shadow Strike - darkness cloak
            Register(new AbilityVFXConfig
            {
                Id = "spark_shadow_strike",
                PrimaryColor = new Color(0.2f, 0.1f, 0.3f),
                SecondaryColor = new Color(0.1f, 0.05f, 0.15f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 20f,
                ParticleLifetime = 0.8f,
                ParticleSize = 0.1f,
                StartSpeed = 0.5f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1f,
                TextureName = "kenney_smoke_08" // Shadow mist
            });

            // Archmage - arcane mastery aura
            Register(new AbilityVFXConfig
            {
                Id = "spark_archmage",
                PrimaryColor = new Color(0.6f, 0.4f, 1f),
                SecondaryColor = new Color(0.8f, 0.6f, 1f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 22f,
                ParticleLifetime = 1.5f,
                ParticleSize = 0.1f,
                StartSpeed = 0.8f,
                Gravity = -0.15f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1.2f,
                LightIntensity = 1f,
                LightColor = new Color(0.6f, 0.4f, 1f),
                LightRange = 5f,
                TextureName = "kenney_magic_02" // Arcane magic
            });

            // Battle Mage - combat magic
            Register(new AbilityVFXConfig
            {
                Id = "spark_battle_mage",
                PrimaryColor = new Color(1f, 0.4f, 0.6f),
                SecondaryColor = new Color(0.8f, 0.2f, 0.4f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 25f,
                ParticleLifetime = 0.8f,
                ParticleSize = 0.08f,
                StartSpeed = 1.2f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 0.8f,
                LightIntensity = 0.7f,
                LightColor = new Color(1f, 0.4f, 0.5f),
                LightRange = 4f,
                TextureName = "kenney_spark_07" // Combat sparks
            });

            // Pyromancer - fire mastery
            Register(new AbilityVFXConfig
            {
                Id = "spark_pyromancer",
                PrimaryColor = new Color(1f, 0.5f, 0.1f),
                SecondaryColor = new Color(1f, 0.2f, 0.05f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 30f,
                ParticleLifetime = 1f,
                ParticleSize = 0.12f,
                StartSpeed = 1f,
                Gravity = -0.3f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1f,
                LightIntensity = 1.2f,
                LightColor = new Color(1f, 0.5f, 0.1f),
                LightRange = 6f,
                TextureName = "kenney_flame_04" // Fire aura
            });

            // Cryomancer - frost mastery
            Register(new AbilityVFXConfig
            {
                Id = "spark_cryomancer",
                PrimaryColor = new Color(0.6f, 0.85f, 1f),
                SecondaryColor = new Color(0.3f, 0.6f, 0.9f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 25f,
                ParticleLifetime = 1.2f,
                ParticleSize = 0.1f,
                StartSpeed = 0.6f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1f,
                LightIntensity = 0.8f,
                LightColor = new Color(0.6f, 0.8f, 1f),
                LightRange = 5f,
                TextureName = "kenney_smoke_09" // Frost mist
            });

            // Paladin - holy warrior
            Register(new AbilityVFXConfig
            {
                Id = "spark_paladin",
                PrimaryColor = new Color(1f, 0.95f, 0.7f),
                SecondaryColor = new Color(1f, 0.85f, 0.5f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 18f,
                ParticleLifetime = 1.5f,
                ParticleSize = 0.12f,
                StartSpeed = 0.5f,
                Gravity = -0.2f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1.2f,
                LightIntensity = 1f,
                LightColor = new Color(1f, 0.95f, 0.7f),
                LightRange = 6f,
                TextureName = "kenney_light_03" // Holy light
            });

            // Bastion - ultimate defense
            Register(new AbilityVFXConfig
            {
                Id = "spark_bastion",
                PrimaryColor = new Color(0.5f, 0.6f, 0.7f),
                SecondaryColor = new Color(0.3f, 0.4f, 0.5f),
                Duration = -1f,
                Loop = true,
                ContinuousEmission = 20f,
                ParticleLifetime = 1f,
                ParticleSize = 0.15f,
                StartSpeed = 0.3f,
                ShapeType = ParticleSystemShapeType.Sphere,
                ShapeRadius = 1.5f,
                LightIntensity = 0.5f,
                LightColor = new Color(0.5f, 0.6f, 0.7f),
                LightRange = 4f,
                TextureName = "kenney_circle_05" // Shield ring
            });
        }

        #endregion
    }

    /// <summary>
    /// Configuration for ability VFX.
    /// </summary>
    public class AbilityVFXConfig
    {
        public string Id;
        public Color PrimaryColor = Color.white;
        public Color SecondaryColor = Color.gray;
        public float Duration = 1f;
        public bool Loop = false;

        // Emission
        public int BurstCount = 30;
        public float ContinuousEmission = 0f;

        // Particles
        public float ParticleLifetime = 1f;
        public float ParticleSize = 0.1f;
        public float StartSpeed = 5f;
        public float Gravity = 0f;
        public int MaxParticles = 100;

        // Shape
        public ParticleSystemShapeType ShapeType = ParticleSystemShapeType.Sphere;
        public float ShapeRadius = 0.5f;
        public float ShapeAngle = 25f;
        public float ShapeArc = 360f;

        // Noise
        public float NoiseStrength = 0f;
        public float NoiseFrequency = 1f;

        // Size over lifetime
        public AnimationCurve SizeOverLifetime = null;

        // Light
        public float LightIntensity = 0f;
        public Color LightColor = Color.white;
        public float LightRange = 5f;

        // Rendering
        public ParticleSystemRenderMode RenderMode = ParticleSystemRenderMode.Billboard;
        public string TextureName = null;
    }

    /// <summary>
    /// Component to fade out a light over time.
    /// </summary>
    internal class LightFader : MonoBehaviour
    {
        private Light _light;
        private float _duration;
        private float _startIntensity;
        private float _elapsed;

        public void Initialize(float duration)
        {
            _light = GetComponent<Light>();
            _duration = duration;
            _startIntensity = _light?.intensity ?? 0f;
        }

        private void Update()
        {
            if (_light == null) return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            _light.intensity = Mathf.Lerp(_startIntensity, 0f, t);
        }
    }
}
