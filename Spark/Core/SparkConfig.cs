using BepInEx.Configuration;
using Spark.Core.Configs;

namespace Spark.Core
{
    /// <summary>
    /// Global configuration for Spark effects.
    /// </summary>
    public static class SparkConfig
    {
        // Config entries
        private static ConfigEntry<float> _qualityMultiplier;
        private static ConfigEntry<int> _maxParticleSystems;
        private static ConfigEntry<int> _maxParticlesPerSystem;
        private static ConfigEntry<int> _maxLights;
        private static ConfigEntry<float> _masterVolume;
        private static ConfigEntry<bool> _effectsEnabled;
        private static ConfigEntry<bool> _audioEnabled;

        // LOD config entries
        private static ConfigEntry<float> _lodFullQuality;
        private static ConfigEntry<float> _lodReduced;
        private static ConfigEntry<float> _lodMinimal;
        private static ConfigEntry<float> _lodCull;

        /// <summary>Global quality multiplier (0-2).</summary>
        public static float QualityMultiplier
        {
            get => _qualityMultiplier?.Value ?? 1f;
            set { if (_qualityMultiplier != null) _qualityMultiplier.Value = value; }
        }

        /// <summary>Maximum active particle systems.</summary>
        public static int MaxActiveParticleSystems
        {
            get => _maxParticleSystems?.Value ?? 50;
            set { if (_maxParticleSystems != null) _maxParticleSystems.Value = value; }
        }

        /// <summary>Maximum particles per system.</summary>
        public static int MaxParticlesPerSystem
        {
            get => _maxParticlesPerSystem?.Value ?? 200;
            set { if (_maxParticlesPerSystem != null) _maxParticlesPerSystem.Value = value; }
        }

        /// <summary>Maximum active point lights.</summary>
        public static int MaxActiveLights
        {
            get => _maxLights?.Value ?? 20;
            set { if (_maxLights != null) _maxLights.Value = value; }
        }

        /// <summary>Master audio volume (0-1).</summary>
        public static float MasterVolume
        {
            get => _masterVolume?.Value ?? 1f;
            set { if (_masterVolume != null) _masterVolume.Value = value; }
        }

        /// <summary>Global effects enabled.</summary>
        public static bool EffectsEnabled
        {
            get => _effectsEnabled?.Value ?? true;
            set { if (_effectsEnabled != null) _effectsEnabled.Value = value; }
        }

        /// <summary>Global audio enabled.</summary>
        public static bool AudioEnabled
        {
            get => _audioEnabled?.Value ?? true;
            set { if (_audioEnabled != null) _audioEnabled.Value = value; }
        }

        /// <summary>LOD configuration.</summary>
        public static LODConfig LOD { get; set; } = new LODConfig();

        /// <summary>
        /// Initializes configuration from BepInEx config.
        /// </summary>
        public static void Initialize(ConfigFile config)
        {
            // Quality settings
            _qualityMultiplier = config.Bind("Quality", "QualityMultiplier", 1.0f,
                new ConfigDescription("Global quality multiplier (0.1-2.0)", new AcceptableValueRange<float>(0.1f, 2f)));

            _maxParticleSystems = config.Bind("Quality", "MaxParticleSystems", 50,
                new ConfigDescription("Maximum active particle systems", new AcceptableValueRange<int>(10, 200)));

            _maxParticlesPerSystem = config.Bind("Quality", "MaxParticlesPerSystem", 200,
                new ConfigDescription("Maximum particles per system", new AcceptableValueRange<int>(50, 500)));

            _maxLights = config.Bind("Quality", "MaxLights", 20,
                new ConfigDescription("Maximum active point lights", new AcceptableValueRange<int>(5, 50)));

            // Audio settings
            _masterVolume = config.Bind("Audio", "MasterVolume", 1.0f,
                new ConfigDescription("Master audio volume", new AcceptableValueRange<float>(0f, 1f)));

            _audioEnabled = config.Bind("Audio", "Enabled", true,
                "Enable audio effects");

            // General settings
            _effectsEnabled = config.Bind("General", "EffectsEnabled", true,
                "Enable visual effects");

            // LOD settings
            _lodFullQuality = config.Bind("LOD", "FullQualityDistance", 15f,
                new ConfigDescription("Distance for full quality effects", new AcceptableValueRange<float>(5f, 50f)));

            _lodReduced = config.Bind("LOD", "ReducedDistance", 30f,
                new ConfigDescription("Distance for reduced quality (50% particles)", new AcceptableValueRange<float>(10f, 100f)));

            _lodMinimal = config.Bind("LOD", "MinimalDistance", 50f,
                new ConfigDescription("Distance for minimal quality (glow only)", new AcceptableValueRange<float>(20f, 150f)));

            _lodCull = config.Bind("LOD", "CullDistance", 100f,
                new ConfigDescription("Distance to cull effects entirely", new AcceptableValueRange<float>(50f, 300f)));

            // Initialize LOD config object
            LOD = new LODConfig
            {
                FullQualityDistance = _lodFullQuality.Value,
                ReducedDistance = _lodReduced.Value,
                MinimalDistance = _lodMinimal.Value,
                CullDistance = _lodCull.Value
            };

            Plugin.Log?.LogInfo("Spark configuration initialized");
        }
    }
}
