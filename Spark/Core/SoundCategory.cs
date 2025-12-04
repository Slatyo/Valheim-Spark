namespace Spark.Core
{
    /// <summary>
    /// Categories for audio effects, allowing per-category volume control.
    /// </summary>
    public enum SoundCategory
    {
        /// <summary>Impact sounds - hits, explosions.</summary>
        Impact,

        /// <summary>Ambient sounds - loops, environmental.</summary>
        Ambient,

        /// <summary>UI sounds - clicks, notifications.</summary>
        UI,

        /// <summary>Ability sounds - spell casts, abilities.</summary>
        Ability,

        /// <summary>Pickup sounds - item pickups, loot.</summary>
        Pickup,

        /// <summary>Creature sounds - roars, reactions.</summary>
        Creature
    }
}
