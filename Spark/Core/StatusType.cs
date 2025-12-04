namespace Spark.Core
{
    /// <summary>
    /// Types of persistent status effect visuals.
    /// </summary>
    public enum StatusType
    {
        /// <summary>On fire - burning flames.</summary>
        Burning,

        /// <summary>Freezing - ice crystals forming.</summary>
        Freezing,

        /// <summary>Poisoned - toxic mist.</summary>
        Poisoned,

        /// <summary>Blessed - holy glow.</summary>
        Blessed,

        /// <summary>Cursed - dark aura.</summary>
        Cursed,

        /// <summary>Electrified - sparking.</summary>
        Electrified,

        /// <summary>Fear - visual indicator.</summary>
        Fear,

        /// <summary>Regenerating - healing effect.</summary>
        Regenerating
    }

    /// <summary>
    /// Attachment points for status effects.
    /// </summary>
    public enum AttachPoint
    {
        /// <summary>Attached to body center.</summary>
        Body,

        /// <summary>Attached to head.</summary>
        Head,

        /// <summary>Attached to feet.</summary>
        Feet,

        /// <summary>Attached to weapon.</summary>
        Weapon
    }
}
