namespace Spark.Core
{
    /// <summary>
    /// Types of one-shot impact effects.
    /// </summary>
    public enum ImpactType
    {
        /// <summary>Fire hit - flames burst.</summary>
        FireHit,

        /// <summary>Frost hit - ice shatter.</summary>
        FrostHit,

        /// <summary>Lightning strike - electric burst.</summary>
        LightningStrike,

        /// <summary>Poison splash - toxic burst.</summary>
        PoisonSplash,

        /// <summary>Spirit hit - ethereal burst.</summary>
        SpiritHit,

        /// <summary>Shadow hit - void burst.</summary>
        ShadowHit,

        /// <summary>Arcane hit - magic burst.</summary>
        ArcaneHit,

        /// <summary>Physical hit - generic impact.</summary>
        PhysicalHit,

        /// <summary>Critical hit - enhanced impact.</summary>
        CriticalHit
    }
}
