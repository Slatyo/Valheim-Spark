namespace Spark.Core.Configs
{
    /// <summary>
    /// Configuration for playing audio.
    /// </summary>
    public class AudioConfig
    {
        /// <summary>Volume (0-1). Default: 1.0</summary>
        public float Volume { get; set; } = 1.0f;

        /// <summary>Pitch multiplier. Default: 1.0</summary>
        public float Pitch { get; set; } = 1.0f;

        /// <summary>Maximum audible distance. Default: 30.0</summary>
        public float MaxDistance { get; set; } = 30.0f;

        /// <summary>Sound category for volume control. Default: Impact</summary>
        public SoundCategory Category { get; set; } = SoundCategory.Impact;

        /// <summary>Random pitch variation (+/- this value). Default: 0</summary>
        public float PitchVariation { get; set; }

        /// <summary>Spatial blend (0 = 2D, 1 = 3D). Default: 1.0</summary>
        public float SpatialBlend { get; set; } = 1.0f;
    }
}
