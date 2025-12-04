using Spark.Core;
using Spark.Core.Configs;
using Spark.Internal;
using UnityEngine;

namespace Spark.API
{
    /// <summary>
    /// API for one-shot impact effects (hits, explosions).
    /// </summary>
    public static class SparkImpact
    {
        /// <summary>
        /// Spawns a preset impact effect at a position.
        /// </summary>
        /// <param name="position">World position.</param>
        /// <param name="type">Impact type.</param>
        public static void Spawn(Vector3 position, ImpactType type)
        {
            ImpactManager.SpawnImpact(position, type);
        }

        /// <summary>
        /// Spawns a custom impact effect at a position.
        /// </summary>
        /// <param name="position">World position.</param>
        /// <param name="config">Impact configuration.</param>
        public static void Spawn(Vector3 position, ImpactConfig config)
        {
            if (config == null)
            {
                Plugin.Log?.LogWarning("SparkImpact.Spawn: config is null");
                return;
            }

            ImpactManager.SpawnCustomImpact(position, config);
        }

        /// <summary>
        /// Spawns an explosion effect.
        /// </summary>
        /// <param name="position">World position.</param>
        /// <param name="config">Explosion configuration.</param>
        public static void Explosion(Vector3 position, ExplosionConfig config)
        {
            if (config == null)
            {
                Plugin.Log?.LogWarning("SparkImpact.Explosion: config is null");
                return;
            }

            ImpactManager.SpawnExplosion(position, config);
        }

        /// <summary>
        /// Spawns an elemental impact at a position.
        /// </summary>
        /// <param name="position">World position.</param>
        /// <param name="element">Element type.</param>
        /// <param name="scale">Scale multiplier.</param>
        public static void SpawnElemental(Vector3 position, Element element, float scale = 1f)
        {
            Spawn(position, new ImpactConfig
            {
                Element = element,
                Scale = scale,
                PlaySound = true
            });
        }
    }
}
