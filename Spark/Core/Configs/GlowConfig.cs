using UnityEngine;

namespace Spark.Core.Configs
{
    /// <summary>
    /// Configuration for weapon glow effects.
    /// </summary>
    public class GlowConfig
    {
        /// <summary>Glow color.</summary>
        public Color Color { get; set; } = Color.white;

        /// <summary>Glow intensity (0-2). Default: 0.8</summary>
        public float Intensity { get; set; } = 0.8f;

        /// <summary>Pulse speed (0 = static, &gt;0 = pulsing). Default: 0</summary>
        public float PulseSpeed { get; set; }

        /// <summary>Minimum intensity when pulsing. Default: 0.3</summary>
        public float PulseMinIntensity { get; set; } = 0.3f;

        /// <summary>Enable point light. Default: false</summary>
        public bool LightEnabled { get; set; }

        /// <summary>Light range. Default: 1.5</summary>
        public float LightRange { get; set; } = 1.5f;
    }
}
