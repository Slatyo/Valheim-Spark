using Spark.Core.Configs;
using Spark.Internal;
using UnityEngine;

namespace Spark.API
{
    /// <summary>
    /// API for environmental zone effects.
    /// </summary>
    public static class SparkEnvironment
    {
        /// <summary>
        /// Creates an environmental effect zone.
        /// </summary>
        /// <param name="position">Center position of the zone.</param>
        /// <param name="config">Zone configuration.</param>
        /// <returns>Zone handle for management.</returns>
        public static SparkZoneHandle CreateZone(Vector3 position, ZoneConfig config)
        {
            if (config == null)
            {
                Plugin.Log?.LogWarning("SparkEnvironment.CreateZone: config is null");
                return null;
            }

            return ZoneManager.CreateZone(position, config);
        }

        /// <summary>
        /// Destroys a zone by handle.
        /// </summary>
        /// <param name="zone">Zone handle.</param>
        public static void DestroyZone(SparkZoneHandle zone)
        {
            zone?.Destroy();
        }
    }
}
