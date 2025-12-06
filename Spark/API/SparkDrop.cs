using Spark.Internal;
using UnityEngine;

namespace Spark.API
{
    /// <summary>
    /// API for item drop visual effects (rarity glows, loot beams).
    /// </summary>
    public static class SparkDrop
    {
        // Rarity colors matching Veneer's VeneerColors
        private static readonly Color CommonColor = new Color(0.7f, 0.7f, 0.7f);      // Gray
        private static readonly Color UncommonColor = new Color(0.12f, 0.75f, 0.12f); // Green
        private static readonly Color RareColor = new Color(0.0f, 0.44f, 0.87f);      // Blue
        private static readonly Color EpicColor = new Color(0.64f, 0.21f, 0.93f);     // Purple
        private static readonly Color LegendaryColor = new Color(1.0f, 0.5f, 0.0f);   // Orange

        /// <summary>
        /// Adds a rarity-based effect to a dropped item.
        /// Creates floating particles, ground glow ring, and pulsing light.
        /// Effect scales with rarity - higher rarity = more noticeable.
        /// </summary>
        /// <param name="itemDrop">The ItemDrop component on the world object.</param>
        /// <param name="rarity">Rarity tier (0=Common, 1=Uncommon, 2=Rare, 3=Epic, 4=Legendary).</param>
        /// <returns>Effect handle, or null if failed.</returns>
        public static SparkEffectHandle AddRarityGlow(ItemDrop itemDrop, int rarity)
        {
            if (itemDrop == null)
            {
                Plugin.Log?.LogWarning("SparkDrop.AddRarityGlow: itemDrop is null");
                return null;
            }

            // Don't add effects to Common items
            if (rarity <= 0)
            {
                Plugin.Log?.LogDebug($"SparkDrop.AddRarityGlow: Skipping rarity {rarity}");
                return null;
            }

            var color = GetRarityColor(rarity);

            Plugin.Log?.LogInfo($"SparkDrop.AddRarityGlow: Adding effect to {itemDrop.name}, rarity={rarity}");

            // Create the effect container
            var effectGo = new GameObject("SparkItemDropEffect");
            effectGo.transform.SetParent(itemDrop.transform, false);
            effectGo.transform.localPosition = Vector3.zero;

            // Add the new 3D item drop effect
            var effect = effectGo.AddComponent<ItemDropEffect>();
            effect.Initialize(color, rarity);

            var handle = new SparkEffectHandle
            {
                EffectObject = effectGo,
                EffectId = System.Guid.NewGuid().ToString()
            };

            EffectTracker.RegisterEffect(itemDrop.gameObject, handle);

            Plugin.Log?.LogDebug($"SparkDrop.AddRarityGlow: Effect created successfully");
            return handle;
        }

        /// <summary>
        /// Removes all Spark effects from a dropped item.
        /// </summary>
        public static void RemoveEffects(ItemDrop itemDrop)
        {
            if (itemDrop == null) return;
            EffectTracker.RemoveAllEffects(itemDrop.gameObject);
        }

        /// <summary>
        /// Gets the color for a rarity tier.
        /// </summary>
        public static Color GetRarityColor(int rarity)
        {
            return rarity switch
            {
                0 => CommonColor,
                1 => UncommonColor,
                2 => RareColor,
                3 => EpicColor,
                4 => LegendaryColor,
                _ => rarity > 4 ? LegendaryColor : CommonColor
            };
        }
    }
}
