using UnityEngine;

namespace Spark.Core.Configs
{
    /// <summary>
    /// Configuration for one-shot impact effects.
    /// </summary>
    public class ImpactConfig
    {
        /// <summary>Element for color/style.</summary>
        public Element Element { get; set; } = Element.None;

        /// <summary>Scale multiplier. Default: 1.0</summary>
        public float Scale { get; set; } = 1.0f;

        /// <summary>Play associated sound. Default: true</summary>
        public bool PlaySound { get; set; } = true;

        /// <summary>Optional color override.</summary>
        public Color? ColorOverride { get; set; }

        /// <summary>Optional rotation to face direction.</summary>
        public Quaternion? Rotation { get; set; }
    }

    /// <summary>
    /// Configuration for explosion effects.
    /// </summary>
    public class ExplosionConfig
    {
        /// <summary>Element for color/style.</summary>
        public Element Element { get; set; } = Element.Fire;

        /// <summary>Explosion radius. Default: 5.0</summary>
        public float Radius { get; set; } = 5.0f;

        /// <summary>Duration of the effect. Default: 1.0</summary>
        public float Duration { get; set; } = 1.0f;

        /// <summary>Play associated sound. Default: true</summary>
        public bool PlaySound { get; set; } = true;

        /// <summary>Shake camera. Default: true</summary>
        public bool CameraShake { get; set; } = true;

        /// <summary>Camera shake intensity. Default: 1.0</summary>
        public float ShakeIntensity { get; set; } = 1.0f;
    }
}
