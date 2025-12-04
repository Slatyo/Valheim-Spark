using UnityEngine;

namespace Spark.Core.Configs
{
    /// <summary>
    /// Configuration for weapon trail effects.
    /// </summary>
    public class TrailConfig
    {
        /// <summary>Trail color.</summary>
        public Color Color { get; set; } = Color.white;

        /// <summary>Trail width. Default: 0.1</summary>
        public float Width { get; set; } = 0.1f;

        /// <summary>Trail duration in seconds. Default: 0.3</summary>
        public float Duration { get; set; } = 0.3f;

        /// <summary>Optional element for color/style. Default: None</summary>
        public Element Element { get; set; } = Element.None;

        /// <summary>Minimum velocity to show trail. Default: 1.0</summary>
        public float MinVelocity { get; set; } = 1.0f;

        /// <summary>Use gradient fade. Default: true</summary>
        public bool UseFade { get; set; } = true;
    }
}
