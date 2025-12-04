using UnityEngine;

namespace Spark.Core.Configs
{
    /// <summary>
    /// Configuration for environmental zone effects.
    /// </summary>
    public class ZoneConfig
    {
        /// <summary>Zone type.</summary>
        public ZoneType Type { get; set; }

        /// <summary>Zone radius. Default: 5.0</summary>
        public float Radius { get; set; } = 5.0f;

        /// <summary>Zone height. Default: 2.0</summary>
        public float Height { get; set; } = 2.0f;

        /// <summary>Duration in seconds (0 = permanent). Default: 0</summary>
        public float Duration { get; set; }

        /// <summary>Effect intensity (0-2). Default: 1.0</summary>
        public float Intensity { get; set; } = 1.0f;

        /// <summary>Optional color override.</summary>
        public Color? ColorOverride { get; set; }

        /// <summary>Play ambient sound. Default: true</summary>
        public bool PlaySound { get; set; } = true;
    }
}
