using UnityEngine;

namespace Spark.Core.Configs
{
    /// <summary>
    /// Configuration for creature aura effects.
    /// </summary>
    public class AuraConfig
    {
        /// <summary>Aura type/shape.</summary>
        public AuraType Type { get; set; } = AuraType.Ring;

        /// <summary>Aura color.</summary>
        public Color Color { get; set; } = Color.white;

        /// <summary>Aura radius. Default: 2.0</summary>
        public float Radius { get; set; } = 2.0f;

        /// <summary>Aura height (for pillars). Default: 3.0</summary>
        public float Height { get; set; } = 3.0f;

        /// <summary>Effect intensity (0-2). Default: 1.0</summary>
        public float Intensity { get; set; } = 1.0f;

        /// <summary>Enable pulsing. Default: false</summary>
        public bool Pulse { get; set; }

        /// <summary>Pulse speed. Default: 1.0</summary>
        public float PulseSpeed { get; set; } = 1.0f;

        /// <summary>Enable rotation. Default: false</summary>
        public bool Rotate { get; set; }

        /// <summary>Rotation speed. Default: 30.0</summary>
        public float RotationSpeed { get; set; } = 30.0f;

        /// <summary>Optional element for preset colors. Default: None</summary>
        public Element Element { get; set; } = Element.None;
    }
}
