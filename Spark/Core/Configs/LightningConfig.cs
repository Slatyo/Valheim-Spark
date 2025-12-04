using UnityEngine;

namespace Spark.Core.Configs
{
    /// <summary>
    /// Configuration for procedural lightning bolt effects.
    /// </summary>
    public class LightningConfig
    {
        /// <summary>Number of segments in the bolt. Default: 10</summary>
        public int Segments { get; set; } = 10;

        /// <summary>How jagged the bolt is (0-1). Default: 0.1</summary>
        public float JaggedAmount { get; set; } = 0.1f;

        /// <summary>Line width. Default: 0.02</summary>
        public float Width { get; set; } = 0.02f;

        /// <summary>Duration in seconds. Default: 0.15</summary>
        public float Duration { get; set; } = 0.15f;

        /// <summary>Number of branch bolts. Default: 2</summary>
        public int Branches { get; set; } = 2;

        /// <summary>Bolt color. Default: white/blue</summary>
        public Color Color { get; set; } = new Color(0.9f, 0.95f, 1f);

        /// <summary>Play sound. Default: true</summary>
        public bool PlaySound { get; set; } = true;
    }

    /// <summary>
    /// Configuration for chain lightning effects.
    /// </summary>
    public class ChainConfig
    {
        /// <summary>Maximum number of chain jumps. Default: 5</summary>
        public int MaxChains { get; set; } = 5;

        /// <summary>Delay between chain jumps. Default: 0.1</summary>
        public float ChainDelay { get; set; } = 0.1f;

        /// <summary>Bolt configuration for each chain.</summary>
        public LightningConfig BoltConfig { get; set; }
    }

    /// <summary>
    /// Configuration for continuous beam effects.
    /// </summary>
    public class BeamConfig
    {
        /// <summary>Element for color/style.</summary>
        public Element Element { get; set; } = Element.Arcane;

        /// <summary>Beam width. Default: 0.1</summary>
        public float Width { get; set; } = 0.1f;

        /// <summary>Optional color override.</summary>
        public Color? ColorOverride { get; set; }

        /// <summary>Enable impact effect at end. Default: true</summary>
        public bool ImpactEffect { get; set; } = true;

        /// <summary>Enable sound. Default: true</summary>
        public bool PlaySound { get; set; } = true;
    }
}
