namespace Spark.Core.Configs
{
    /// <summary>
    /// Configuration for full elemental weapon effects (particles + glow + trail + light).
    /// </summary>
    public class WeaponEffectConfig
    {
        /// <summary>Particle intensity (0-2). Default: 1.0</summary>
        public float ParticleIntensity { get; set; } = 1.0f;

        /// <summary>Glow intensity (0-2). Default: 0.5</summary>
        public float GlowIntensity { get; set; } = 0.5f;

        /// <summary>Enable trail effect. Default: true</summary>
        public bool TrailEnabled { get; set; } = true;

        /// <summary>Enable point light. Default: true</summary>
        public bool LightEnabled { get; set; } = true;

        /// <summary>Light range. Default: 2.0</summary>
        public float LightRange { get; set; } = 2.0f;

        /// <summary>Enable ambient sound loop. Default: true</summary>
        public bool SoundEnabled { get; set; } = true;
    }
}
