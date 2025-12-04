using UnityEngine;

namespace Spark.Core.Configs
{
    /// <summary>
    /// Configuration for elemental particle effects.
    /// </summary>
    public class ElementalConfig
    {
        /// <summary>Effect intensity (0-2). Default: 1.0</summary>
        public float Intensity { get; set; } = 1.0f;

        /// <summary>Effect scale multiplier. Default: 1.0</summary>
        public float Scale { get; set; } = 1.0f;

        /// <summary>Enable trail effect. Default: false</summary>
        public bool TrailEnabled { get; set; }

        /// <summary>Enable point light. Default: true</summary>
        public bool LightEnabled { get; set; } = true;

        /// <summary>Light range in units. Default: 2.0</summary>
        public float LightRange { get; set; } = 2.0f;

        /// <summary>Optional color override (null = use element default).</summary>
        public Color? ColorOverride { get; set; }

        /// <summary>Emission rate multiplier. Default: 1.0</summary>
        public float EmissionMultiplier { get; set; } = 1.0f;
    }

    /// <summary>
    /// Internal configuration for element visuals (colors, textures, etc.)
    /// </summary>
    public class ElementalEffectDefinition
    {
        /// <summary>Primary particle color.</summary>
        public Color PrimaryColor { get; set; }

        /// <summary>Secondary/gradient color.</summary>
        public Color SecondaryColor { get; set; }

        /// <summary>Base emission rate (particles/sec).</summary>
        public float EmissionRate { get; set; }

        /// <summary>Particle lifetime in seconds.</summary>
        public float ParticleLifetime { get; set; }

        /// <summary>Particle size.</summary>
        public float ParticleSize { get; set; } = 0.1f;

        /// <summary>Particle start speed.</summary>
        public float StartSpeed { get; set; } = 0.1f;

        /// <summary>Gravity modifier (negative = rises).</summary>
        public float GravityModifier { get; set; }

        /// <summary>Noise/turbulence strength.</summary>
        public float NoiseStrength { get; set; }

        /// <summary>Noise frequency.</summary>
        public float NoiseFrequency { get; set; } = 1f;

        /// <summary>Light color.</summary>
        public Color LightColor { get; set; }

        /// <summary>Light intensity.</summary>
        public float LightIntensity { get; set; }

        /// <summary>Light range.</summary>
        public float LightRange { get; set; } = 1.5f;

        /// <summary>Impact sound ID.</summary>
        public string ImpactSound { get; set; }

        /// <summary>Ambient loop sound ID.</summary>
        public string AmbientSound { get; set; }
    }
}
