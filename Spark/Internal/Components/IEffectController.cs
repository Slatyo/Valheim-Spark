using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Interface for all Spark elemental effect controllers.
    /// Provides a consistent API for initializing effects based on calculated bounds.
    /// </summary>
    public interface IEffectController
    {
        /// <summary>
        /// Initialize the effect with bounds information.
        /// </summary>
        /// <param name="bounds">The calculated bounds of the target object.</param>
        void Initialize(SparkBounds bounds);

        /// <summary>
        /// Set the effect intensity. Higher values = more particles, faster effects, etc.
        /// </summary>
        /// <param name="intensity">Intensity multiplier (no upper limit).</param>
        void SetIntensity(float intensity);

        /// <summary>
        /// Whether the effect is currently active/playing.
        /// </summary>
        bool IsActive { get; }
    }

    /// <summary>
    /// Optional interface for effects that need custom behavior per target type.
    /// Implement this when the default bounds-based behavior isn't sufficient.
    /// </summary>
    public interface ITargetTypeAdapter
    {
        /// <summary>
        /// Apply custom configuration based on target type.
        /// Called after Initialize() if implemented.
        /// </summary>
        /// <param name="targetType">The detected type of the target object.</param>
        void AdaptToTargetType(BoundsTargetType targetType);
    }
}
