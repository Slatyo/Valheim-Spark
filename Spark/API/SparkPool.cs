using Spark.Core;

namespace Spark.API
{
    /// <summary>
    /// API for managing effect pooling.
    /// </summary>
    public static class SparkPool
    {
        /// <summary>
        /// Pre-warms the pool for an element type.
        /// </summary>
        /// <param name="element">Element type.</param>
        /// <param name="count">Number of instances to pre-create.</param>
        public static void Prewarm(Element element, int count)
        {
            if (count <= 0) return;
            EffectPool.Prewarm(element, count);
        }

        /// <summary>
        /// Clears all pooled effects.
        /// </summary>
        public static void Clear()
        {
            EffectPool.Clear();
        }

        /// <summary>
        /// Gets statistics about the pool.
        /// </summary>
        /// <returns>Pool statistics.</returns>
        public static PoolStats GetStats()
        {
            return EffectPool.GetStats();
        }
    }

    /// <summary>
    /// Statistics about the effect pool.
    /// </summary>
    public class PoolStats
    {
        /// <summary>Total pooled particle systems.</summary>
        public int TotalPooled { get; set; }

        /// <summary>Currently active effects.</summary>
        public int ActiveEffects { get; set; }

        /// <summary>Total pooled audio sources.</summary>
        public int TotalAudioSources { get; set; }

        /// <summary>Currently playing audio.</summary>
        public int ActiveAudio { get; set; }
    }
}
