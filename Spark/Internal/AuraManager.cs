using System.Collections.Generic;
using Spark.Core;
using Spark.Core.Configs;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Manages creature auras.
    /// </summary>
    internal static class AuraManager
    {
        private static readonly Dictionary<int, Dictionary<string, GameObject>> CreatureAuras = new Dictionary<int, Dictionary<string, GameObject>>();

        /// <summary>
        /// Attaches an aura to a creature.
        /// </summary>
        public static string AttachAura(Character creature, AuraConfig config)
        {
            if (creature == null) return null;

            string auraId = System.Guid.NewGuid().ToString();
            var auraGo = CreateAuraObject(creature.gameObject, config);

            int creatureId = creature.GetInstanceID();
            if (!CreatureAuras.TryGetValue(creatureId, out var auras))
            {
                auras = new Dictionary<string, GameObject>();
                CreatureAuras[creatureId] = auras;
            }

            auras[auraId] = auraGo;
            return auraId;
        }

        /// <summary>
        /// Removes a specific aura.
        /// </summary>
        public static void RemoveAura(Character creature, string auraId)
        {
            if (creature == null || string.IsNullOrEmpty(auraId)) return;

            int creatureId = creature.GetInstanceID();
            if (CreatureAuras.TryGetValue(creatureId, out var auras))
            {
                if (auras.TryGetValue(auraId, out var auraGo))
                {
                    Object.Destroy(auraGo);
                    auras.Remove(auraId);
                }
            }
        }

        /// <summary>
        /// Removes all auras from a creature.
        /// </summary>
        public static void RemoveAllAuras(Character creature)
        {
            if (creature == null) return;

            int creatureId = creature.GetInstanceID();
            if (CreatureAuras.TryGetValue(creatureId, out var auras))
            {
                foreach (var auraGo in auras.Values)
                {
                    if (auraGo != null)
                        Object.Destroy(auraGo);
                }
                auras.Clear();
                CreatureAuras.Remove(creatureId);
            }
        }

        private static GameObject CreateAuraObject(GameObject parent, AuraConfig config)
        {
            var auraGo = new GameObject($"SparkAura_{config.Type}");
            auraGo.transform.SetParent(parent.transform);
            auraGo.transform.localPosition = Vector3.zero;

            Color color = config.Color;
            if (config.Element != Element.None)
            {
                color = ElementDefinitions.Get(config.Element).PrimaryColor;
            }

            switch (config.Type)
            {
                case AuraType.Ring:
                    CreateRingAura(auraGo, config, color);
                    break;
                case AuraType.Sphere:
                    CreateSphereAura(auraGo, config, color);
                    break;
                case AuraType.Pillar:
                    CreatePillarAura(auraGo, config, color);
                    break;
                case AuraType.Ground:
                    CreateGroundAura(auraGo, config, color);
                    break;
            }

            // Add animator for pulse/rotate
            if (config.Pulse || config.Rotate)
            {
                var animator = auraGo.AddComponent<AuraAnimator>();
                animator.Initialize(config.Pulse, config.PulseSpeed, config.Rotate, config.RotationSpeed);
            }

            return auraGo;
        }

        private static void CreateRingAura(GameObject go, AuraConfig config, Color color)
        {
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startLifetime = 1f;
            main.startSize = 0.1f;
            main.maxParticles = (int)(50 * SparkConfig.QualityMultiplier);

            var emission = ps.emission;
            emission.rateOverTime = 30 * config.Intensity * SparkConfig.QualityMultiplier;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = config.Radius;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = ShaderUtils.CreateParticleMaterial();
        }

        private static void CreateSphereAura(GameObject go, AuraConfig config, Color color)
        {
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startLifetime = 1.5f;
            main.startSize = 0.08f;
            main.maxParticles = (int)(100 * SparkConfig.QualityMultiplier);

            var emission = ps.emission;
            emission.rateOverTime = 40 * config.Intensity * SparkConfig.QualityMultiplier;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = config.Radius;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = ShaderUtils.CreateParticleMaterial();
        }

        private static void CreatePillarAura(GameObject go, AuraConfig config, Color color)
        {
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startLifetime = config.Height / 2f;
            main.startSize = 0.15f;
            main.startSpeed = 2f;
            main.maxParticles = (int)(150 * SparkConfig.QualityMultiplier);

            var emission = ps.emission;
            emission.rateOverTime = 50 * config.Intensity * SparkConfig.QualityMultiplier;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = config.Radius * 0.3f;

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.y = config.Height;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = ShaderUtils.CreateParticleMaterial();
        }

        private static void CreateGroundAura(GameObject go, AuraConfig config, Color color)
        {
            go.transform.localPosition = new Vector3(0, 0.1f, 0);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startLifetime = 2f;
            main.startSize = 0.2f;
            main.gravityModifier = -0.05f;
            main.maxParticles = (int)(80 * SparkConfig.QualityMultiplier);

            var emission = ps.emission;
            emission.rateOverTime = 25 * config.Intensity * SparkConfig.QualityMultiplier;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = config.Radius;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = ShaderUtils.CreateParticleMaterial();
        }
    }
}
