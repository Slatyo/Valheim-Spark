using UnityEngine;

namespace Spark.Core.Configs
{
    /// <summary>
    /// Configuration for persistent status effect visuals.
    /// </summary>
    public class StatusConfig
    {
        /// <summary>Status type.</summary>
        public StatusType Type { get; set; }

        /// <summary>Effect intensity (0-2). Default: 1.0</summary>
        public float Intensity { get; set; } = 1.0f;

        /// <summary>Attachment point on character. Default: Body</summary>
        public AttachPoint AttachPoint { get; set; } = AttachPoint.Body;

        /// <summary>Optional color override.</summary>
        public Color? ColorOverride { get; set; }

        /// <summary>Scale multiplier. Default: 1.0</summary>
        public float Scale { get; set; } = 1.0f;

        /// <summary>Play ambient sound. Default: true</summary>
        public bool PlaySound { get; set; } = true;
    }
}
