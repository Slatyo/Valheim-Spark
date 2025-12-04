using System.Collections;
using System.Collections.Generic;
using Spark.API;
using Spark.Core;
using Spark.Core.Configs;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Creates procedural effects like lightning bolts and beams.
    /// </summary>
    internal static class ProceduralEffects
    {
        /// <summary>
        /// Creates a lightning bolt effect.
        /// </summary>
        public static void CreateLightningBolt(Vector3 start, Vector3 end, LightningConfig config)
        {
            if (!SparkConfig.EffectsEnabled) return;

            var boltGo = new GameObject("SparkLightningBolt");
            var lineRenderer = boltGo.AddComponent<LineRenderer>();

            // Configure line renderer
            lineRenderer.material = ShaderUtils.CreateParticleMaterial(config.Color);
            lineRenderer.startColor = config.Color;
            lineRenderer.endColor = config.Color;
            lineRenderer.startWidth = config.Width;
            lineRenderer.endWidth = config.Width * 0.5f;

            // Generate jagged points
            var points = GenerateLightningPoints(start, end, config.Segments, config.JaggedAmount);
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);

            // Add branches
            if (config.Branches > 0)
            {
                for (int i = 0; i < config.Branches; i++)
                {
                    int branchStart = Random.Range(1, points.Length - 2);
                    Vector3 branchEnd = points[branchStart] + Random.insideUnitSphere * Vector3.Distance(start, end) * 0.3f;
                    CreateLightningBranch(boltGo.transform, points[branchStart], branchEnd, config);
                }
            }

            // Add light flash
            var light = boltGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = config.Color;
            light.intensity = 2f;
            light.range = Vector3.Distance(start, end) * 0.5f;
            light.transform.position = (start + end) / 2f;

            // Auto-destroy after duration
            var destroyer = boltGo.AddComponent<TimedDestroyer>();
            destroyer.Initialize(config.Duration);

            // Play sound
            if (config.PlaySound)
            {
                AudioManager.PlaySound("lightning_strike", (start + end) / 2f, new AudioConfig
                {
                    Category = SoundCategory.Impact,
                    PitchVariation = 0.15f
                });
            }
        }

        private static void CreateLightningBranch(Transform parent, Vector3 start, Vector3 end, LightningConfig config)
        {
            var branchGo = new GameObject("Branch");
            branchGo.transform.SetParent(parent);

            var lineRenderer = branchGo.AddComponent<LineRenderer>();
            lineRenderer.material = ShaderUtils.CreateParticleMaterial(config.Color);
            lineRenderer.startColor = new Color(config.Color.r, config.Color.g, config.Color.b, 0.7f);
            lineRenderer.endColor = new Color(config.Color.r, config.Color.g, config.Color.b, 0.3f);
            lineRenderer.startWidth = config.Width * 0.6f;
            lineRenderer.endWidth = config.Width * 0.1f;

            var points = GenerateLightningPoints(start, end, config.Segments / 2, config.JaggedAmount * 1.5f);
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }

        private static Vector3[] GenerateLightningPoints(Vector3 start, Vector3 end, int segments, float jag)
        {
            var points = new Vector3[segments + 1];
            points[0] = start;
            points[segments] = end;

            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            if (perpendicular.magnitude < 0.1f)
                perpendicular = Vector3.Cross(direction, Vector3.right).normalized;

            for (int i = 1; i < segments; i++)
            {
                float t = i / (float)segments;
                Vector3 basePos = Vector3.Lerp(start, end, t);
                Vector3 offset = perpendicular * Random.Range(-jag, jag) * distance;
                offset += Vector3.Cross(perpendicular, direction) * Random.Range(-jag, jag) * distance;
                points[i] = basePos + offset;
            }

            return points;
        }

        /// <summary>
        /// Creates chain lightning between targets.
        /// </summary>
        public static void CreateChainLightning(Vector3 origin, List<Vector3> targets, ChainConfig config)
        {
            if (!SparkConfig.EffectsEnabled || targets.Count == 0) return;

            var chainGo = new GameObject("SparkChainLightning");
            var coroutine = chainGo.AddComponent<ChainLightningCoroutine>();
            coroutine.StartChain(origin, targets, config);
        }

        /// <summary>
        /// Creates a continuous beam effect.
        /// </summary>
        public static SparkBeamHandle CreateBeam(Vector3 origin, Vector3 target, BeamConfig config)
        {
            if (!SparkConfig.EffectsEnabled) return null;

            var beamGo = new GameObject("SparkBeam");

            var lineRenderer = beamGo.AddComponent<LineRenderer>();
            lineRenderer.material = ShaderUtils.CreateParticleMaterial();

            Color color = config.ColorOverride ?? ElementDefinitions.Get(config.Element).PrimaryColor;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = config.Width;
            lineRenderer.endWidth = config.Width;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, target);

            // Add impact effect at end
            if (config.ImpactEffect)
            {
                var impactGo = new GameObject("Impact");
                impactGo.transform.SetParent(beamGo.transform);
                impactGo.transform.position = target;

                var ps = impactGo.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startColor = color;
                main.startLifetime = 0.3f;
                main.startSize = config.Width * 3;
                main.maxParticles = 20;

                var emission = ps.emission;
                emission.rateOverTime = 30;

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = config.Width;

                ps.Play();
            }

            // Play sound
            if (config.PlaySound)
            {
                AudioManager.PlayAttached("beam_loop", beamGo, loop: true);
            }

            return new SparkBeamHandle
            {
                BeamObject = beamGo,
                LineRenderer = lineRenderer
            };
        }

        /// <summary>
        /// Creates a ground crack effect.
        /// </summary>
        public static void CreateGroundCrack(Vector3 position, Vector3 direction, float length)
        {
            if (!SparkConfig.EffectsEnabled) return;

            var crackGo = new GameObject("SparkGroundCrack");
            crackGo.transform.position = position;

            // Create line renderer for crack
            var lineRenderer = crackGo.AddComponent<LineRenderer>();
            lineRenderer.material = ShaderUtils.CreateParticleMaterial(new Color(0.3f, 0.2f, 0.1f));
            lineRenderer.startColor = new Color(0.3f, 0.2f, 0.1f);
            lineRenderer.endColor = new Color(0.3f, 0.2f, 0.1f, 0f);
            lineRenderer.startWidth = 0.2f;
            lineRenderer.endWidth = 0.05f;

            // Generate crack points
            int segments = (int)(length * 3);
            lineRenderer.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)(segments - 1);
                Vector3 pos = position + direction.normalized * length * t;
                pos.y = position.y; // Keep on ground
                pos += Vector3.Cross(direction, Vector3.up).normalized * Random.Range(-0.1f, 0.1f);
                lineRenderer.SetPosition(i, pos);
            }

            // Add dust particles
            var dustGo = new GameObject("Dust");
            dustGo.transform.SetParent(crackGo.transform);
            dustGo.transform.localPosition = Vector3.zero;

            var ps = dustGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.5f, 0.4f, 0.3f, 0.5f);
            main.startLifetime = 1.5f;
            main.startSize = 0.3f;
            main.startSpeed = 0.5f;
            main.gravityModifier = -0.1f;
            main.maxParticles = 50;

            var emission = ps.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 30));

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(length, 0.1f, 0.2f);
            shape.rotation = Quaternion.LookRotation(direction).eulerAngles;

            ps.Play();

            // Auto-destroy
            var destroyer = crackGo.AddComponent<TimedDestroyer>();
            destroyer.Initialize(3f);
        }
    }
}
