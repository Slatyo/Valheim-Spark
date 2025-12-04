using Spark.Core;
using Spark.Core.Configs;

namespace Spark.API
{
    /// <summary>
    /// API for runtime settings.
    /// </summary>
    public static class SparkSettings
    {
        /// <summary>
        /// Global quality multiplier (0-2). Affects particle counts, etc.
        /// </summary>
        public static float QualityMultiplier
        {
            get => SparkConfig.QualityMultiplier;
            set => SparkConfig.QualityMultiplier = value;
        }

        /// <summary>
        /// LOD configuration.
        /// </summary>
        public static LODConfig LOD
        {
            get => SparkConfig.LOD;
            set => SparkConfig.LOD = value ?? new LODConfig();
        }

        /// <summary>
        /// Maximum active particle systems.
        /// </summary>
        public static int MaxActiveParticleSystems
        {
            get => SparkConfig.MaxActiveParticleSystems;
            set => SparkConfig.MaxActiveParticleSystems = value;
        }

        /// <summary>
        /// Maximum particles per system.
        /// </summary>
        public static int MaxParticlesPerSystem
        {
            get => SparkConfig.MaxParticlesPerSystem;
            set => SparkConfig.MaxParticlesPerSystem = value;
        }

        /// <summary>
        /// Maximum active point lights.
        /// </summary>
        public static int MaxActiveLights
        {
            get => SparkConfig.MaxActiveLights;
            set => SparkConfig.MaxActiveLights = value;
        }

        /// <summary>
        /// Master audio volume (0-1).
        /// </summary>
        public static float MasterVolume
        {
            get => SparkConfig.MasterVolume;
            set => SparkConfig.MasterVolume = value;
        }

        /// <summary>
        /// Enables or disables all effects globally.
        /// </summary>
        public static bool EffectsEnabled
        {
            get => SparkConfig.EffectsEnabled;
            set => SparkConfig.EffectsEnabled = value;
        }

        /// <summary>
        /// Enables or disables all audio globally.
        /// </summary>
        public static bool AudioEnabled
        {
            get => SparkConfig.AudioEnabled;
            set => SparkConfig.AudioEnabled = value;
        }
    }
}
