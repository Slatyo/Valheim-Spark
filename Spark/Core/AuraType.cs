namespace Spark.Core
{
    /// <summary>
    /// Types of visual auras that can be attached to creatures.
    /// </summary>
    public enum AuraType
    {
        /// <summary>Ring aura around the base.</summary>
        Ring,

        /// <summary>Spherical aura around the body.</summary>
        Sphere,

        /// <summary>Vertical pillar of light/energy.</summary>
        Pillar,

        /// <summary>Ground-based circle effect.</summary>
        Ground
    }

    /// <summary>
    /// Preset aura configurations.
    /// </summary>
    public enum AuraPreset
    {
        /// <summary>Enraged creature - red pulsing.</summary>
        Enraged,

        /// <summary>Frozen creature - blue ice particles.</summary>
        Frozen,

        /// <summary>Poisoned creature - green cloud.</summary>
        Poisoned,

        /// <summary>Shielded creature - golden barrier.</summary>
        Shielded,

        /// <summary>Elite creature indicator.</summary>
        Elite,

        /// <summary>Boss creature aura.</summary>
        Boss
    }
}
