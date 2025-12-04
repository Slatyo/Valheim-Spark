using Spark.Core;
using Spark.Core.Configs;
using Spark.Internal;
using UnityEngine;

namespace Spark.API
{
    /// <summary>
    /// API for persistent status effect visuals on characters.
    /// </summary>
    public static class SparkStatus
    {
        /// <summary>
        /// Attaches a status visual to a character.
        /// </summary>
        /// <param name="character">Target Character.</param>
        /// <param name="config">Status configuration.</param>
        /// <returns>Status ID for removal.</returns>
        public static string Attach(Character character, StatusConfig config)
        {
            if (character == null)
            {
                Plugin.Log?.LogWarning("SparkStatus.Attach: character is null");
                return null;
            }

            config ??= new StatusConfig();
            return StatusManager.AttachStatus(character, config);
        }

        /// <summary>
        /// Removes a specific status by ID.
        /// </summary>
        /// <param name="character">Target Character.</param>
        /// <param name="statusId">Status ID from Attach.</param>
        public static void Remove(Character character, string statusId)
        {
            if (character == null || string.IsNullOrEmpty(statusId)) return;
            StatusManager.RemoveStatus(character, statusId);
        }

        /// <summary>
        /// Removes all status visuals from a character.
        /// </summary>
        /// <param name="character">Target Character.</param>
        public static void RemoveAll(Character character)
        {
            if (character == null) return;
            StatusManager.RemoveAllStatuses(character);
        }

        // === Preset Statuses ===

        /// <summary>Attaches burning status (flames).</summary>
        public static string AttachBurning(Character character)
        {
            return Attach(character, new StatusConfig
            {
                Type = StatusType.Burning,
                Intensity = 1f,
                AttachPoint = AttachPoint.Body
            });
        }

        /// <summary>Attaches freezing status (ice crystals).</summary>
        public static string AttachFreezing(Character character)
        {
            return Attach(character, new StatusConfig
            {
                Type = StatusType.Freezing,
                Intensity = 1f,
                AttachPoint = AttachPoint.Body
            });
        }

        /// <summary>Attaches poisoned status (toxic mist).</summary>
        public static string AttachPoisoned(Character character)
        {
            return Attach(character, new StatusConfig
            {
                Type = StatusType.Poisoned,
                Intensity = 0.8f,
                AttachPoint = AttachPoint.Body
            });
        }

        /// <summary>Attaches blessed status (holy glow).</summary>
        public static string AttachBlessed(Character character)
        {
            return Attach(character, new StatusConfig
            {
                Type = StatusType.Blessed,
                Intensity = 0.6f,
                AttachPoint = AttachPoint.Body
            });
        }

        /// <summary>Attaches cursed status (dark aura).</summary>
        public static string AttachCursed(Character character)
        {
            return Attach(character, new StatusConfig
            {
                Type = StatusType.Cursed,
                Intensity = 1f,
                AttachPoint = AttachPoint.Body
            });
        }

        /// <summary>Attaches electrified status (sparks).</summary>
        public static string AttachElectrified(Character character)
        {
            return Attach(character, new StatusConfig
            {
                Type = StatusType.Electrified,
                Intensity = 1f,
                AttachPoint = AttachPoint.Body
            });
        }
    }
}
