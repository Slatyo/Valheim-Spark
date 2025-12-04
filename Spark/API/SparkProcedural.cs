using System.Collections.Generic;
using Spark.Core.Configs;
using Spark.Internal;
using UnityEngine;

namespace Spark.API
{
    /// <summary>
    /// API for procedurally generated effects (lightning, beams, ground cracks).
    /// </summary>
    public static class SparkProcedural
    {
        /// <summary>
        /// Creates a lightning bolt between two points.
        /// </summary>
        /// <param name="start">Start position.</param>
        /// <param name="end">End position.</param>
        /// <param name="config">Optional lightning configuration.</param>
        public static void LightningBolt(Vector3 start, Vector3 end, LightningConfig config = null)
        {
            config ??= new LightningConfig();
            ProceduralEffects.CreateLightningBolt(start, end, config);
        }

        /// <summary>
        /// Creates an attached lightning effect that continuously spawns bolts from a source.
        /// Uses path-tracing approach with ground detection and chained lightning.
        /// </summary>
        /// <param name="source">Source transform to attach to.</param>
        /// <param name="sourceLength">Length of the source object (for random start points).</param>
        /// <param name="sourceCenter">Center offset of the source.</param>
        /// <param name="lengthAxis">Axis of source length (0=X, 1=Y, 2=Z).</param>
        /// <returns>The lightning controller component.</returns>
        public static LightningEffectController AttachLightningEffect(Transform source, float sourceLength = 1f,
            Vector3? sourceCenter = null, int lengthAxis = 1)
        {
            if (source == null)
            {
                Plugin.Log?.LogWarning("SparkProcedural.AttachLightningEffect: source is null");
                return null;
            }

            var lightningGo = new GameObject("SparkLightningEffect");
            lightningGo.transform.SetParent(source, false);

            var controller = lightningGo.AddComponent<LightningEffectController>();
            controller.Initialize(source, sourceLength, sourceCenter ?? Vector3.zero, lengthAxis);

            return controller;
        }

        /// <summary>
        /// Creates a standalone lightning effect at a position.
        /// </summary>
        /// <param name="position">World position for the lightning.</param>
        /// <returns>The lightning controller component.</returns>
        public static LightningEffectController CreateLightningEffect(Vector3 position)
        {
            var lightningGo = new GameObject("SparkLightningEffect");
            lightningGo.transform.position = position;

            var controller = lightningGo.AddComponent<LightningEffectController>();
            controller.Initialize(position);

            return controller;
        }

        /// <summary>
        /// Creates an attached fire effect with dynamic flames.
        /// Uses world-space particles with velocity inheritance for realistic trailing.
        /// </summary>
        /// <param name="source">Source transform to attach to.</param>
        /// <param name="sourceLength">Length of the source object (for particle shape).</param>
        /// <param name="sourceCenter">Center offset of the source.</param>
        /// <param name="lengthAxis">Axis of source length (0=X, 1=Y, 2=Z).</param>
        /// <returns>The fire controller component.</returns>
        public static FireEffectController AttachFireEffect(Transform source, float sourceLength = 1f,
            Vector3? sourceCenter = null, int lengthAxis = 1)
        {
            if (source == null)
            {
                Plugin.Log?.LogWarning("SparkProcedural.AttachFireEffect: source is null");
                return null;
            }

            var fireGo = new GameObject("SparkFireEffect");
            fireGo.transform.SetParent(source, false);

            var controller = fireGo.AddComponent<FireEffectController>();
            controller.Initialize(sourceLength, sourceCenter ?? Vector3.zero, lengthAxis);

            return controller;
        }

        /// <summary>
        /// Creates chain lightning that jumps between targets.
        /// </summary>
        /// <param name="origin">Origin position.</param>
        /// <param name="targets">List of target positions.</param>
        /// <param name="config">Optional chain configuration.</param>
        public static void ChainLightning(Vector3 origin, List<Vector3> targets, ChainConfig config = null)
        {
            if (targets == null || targets.Count == 0)
            {
                Plugin.Log?.LogWarning("SparkProcedural.ChainLightning: no targets provided");
                return;
            }

            config ??= new ChainConfig();
            ProceduralEffects.CreateChainLightning(origin, targets, config);
        }

        /// <summary>
        /// Creates a continuous beam effect.
        /// </summary>
        /// <param name="origin">Beam origin.</param>
        /// <param name="target">Beam target.</param>
        /// <param name="config">Beam configuration.</param>
        /// <returns>Beam handle for updating/destroying.</returns>
        public static SparkBeamHandle CreateBeam(Vector3 origin, Vector3 target, BeamConfig config)
        {
            if (config == null)
            {
                Plugin.Log?.LogWarning("SparkProcedural.CreateBeam: config is null");
                return null;
            }

            return ProceduralEffects.CreateBeam(origin, target, config);
        }

        /// <summary>
        /// Creates a ground crack/fissure effect.
        /// </summary>
        /// <param name="position">Start position.</param>
        /// <param name="direction">Direction of the crack.</param>
        /// <param name="length">Length of the crack.</param>
        public static void GroundCrack(Vector3 position, Vector3 direction, float length)
        {
            ProceduralEffects.CreateGroundCrack(position, direction, length);
        }
    }
}
