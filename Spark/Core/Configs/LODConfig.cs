namespace Spark.Core.Configs
{
    /// <summary>
    /// Level-of-detail configuration for effects.
    /// </summary>
    public class LODConfig
    {
        /// <summary>Distance for full quality effects. Default: 15</summary>
        public float FullQualityDistance { get; set; } = 15f;

        /// <summary>Distance for reduced quality (50% particles). Default: 30</summary>
        public float ReducedDistance { get; set; } = 30f;

        /// <summary>Distance for minimal quality (glow only). Default: 50</summary>
        public float MinimalDistance { get; set; } = 50f;

        /// <summary>Distance to cull effects entirely. Default: 100</summary>
        public float CullDistance { get; set; } = 100f;
    }
}
