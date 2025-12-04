using System.Collections.Generic;
using Spark.Core;
using Spark.Core.Configs;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Manages persistent status effect visuals.
    /// </summary>
    internal static class StatusManager
    {
        private static readonly Dictionary<int, Dictionary<string, GameObject>> CharacterStatuses = new Dictionary<int, Dictionary<string, GameObject>>();

        /// <summary>
        /// Attaches a status effect visual.
        /// </summary>
        public static string AttachStatus(Character character, StatusConfig config)
        {
            if (character == null) return null;

            string statusId = System.Guid.NewGuid().ToString();
            var statusGo = CreateStatusObject(character.gameObject, config);

            int characterId = character.GetInstanceID();
            if (!CharacterStatuses.TryGetValue(characterId, out var statuses))
            {
                statuses = new Dictionary<string, GameObject>();
                CharacterStatuses[characterId] = statuses;
            }

            statuses[statusId] = statusGo;

            // Play ambient sound if applicable
            if (config.PlaySound)
            {
                string soundId = GetStatusSound(config.Type);
                if (!string.IsNullOrEmpty(soundId))
                {
                    AudioManager.PlayAttached(soundId, statusGo, loop: true);
                }
            }

            return statusId;
        }

        /// <summary>
        /// Removes a specific status.
        /// </summary>
        public static void RemoveStatus(Character character, string statusId)
        {
            if (character == null || string.IsNullOrEmpty(statusId)) return;

            int characterId = character.GetInstanceID();
            if (CharacterStatuses.TryGetValue(characterId, out var statuses))
            {
                if (statuses.TryGetValue(statusId, out var statusGo))
                {
                    Object.Destroy(statusGo);
                    statuses.Remove(statusId);
                }
            }
        }

        /// <summary>
        /// Removes all statuses from a character.
        /// </summary>
        public static void RemoveAllStatuses(Character character)
        {
            if (character == null) return;

            int characterId = character.GetInstanceID();
            if (CharacterStatuses.TryGetValue(characterId, out var statuses))
            {
                foreach (var statusGo in statuses.Values)
                {
                    if (statusGo != null)
                        Object.Destroy(statusGo);
                }
                statuses.Clear();
                CharacterStatuses.Remove(characterId);
            }
        }

        private static GameObject CreateStatusObject(GameObject parent, StatusConfig config)
        {
            var statusGo = new GameObject($"SparkStatus_{config.Type}");
            statusGo.transform.SetParent(parent.transform);

            // Position based on attach point
            statusGo.transform.localPosition = GetAttachOffset(config.AttachPoint);

            Element element = GetElementForStatus(config.Type);
            var definition = ElementDefinitions.Get(element);
            Color color = config.ColorOverride ?? definition.PrimaryColor;

            // Create particles
            var ps = statusGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startLifetime = definition.ParticleLifetime;
            main.startSize = definition.ParticleSize * config.Scale;
            main.gravityModifier = definition.GravityModifier;
            main.maxParticles = (int)(50 * SparkConfig.QualityMultiplier);

            var emission = ps.emission;
            emission.rateOverTime = definition.EmissionRate * config.Intensity * 0.5f * SparkConfig.QualityMultiplier;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = GetShapeScale(config.AttachPoint) * config.Scale;

            var noise = ps.noise;
            if (definition.NoiseStrength > 0)
            {
                noise.enabled = true;
                noise.strength = definition.NoiseStrength;
                noise.frequency = definition.NoiseFrequency;
            }

            var renderer = statusGo.GetComponent<ParticleSystemRenderer>();
            renderer.material = ShaderUtils.CreateParticleMaterial();

            ps.Play();
            return statusGo;
        }

        private static Vector3 GetAttachOffset(AttachPoint point)
        {
            return point switch
            {
                AttachPoint.Head => new Vector3(0, 1.8f, 0),
                AttachPoint.Body => new Vector3(0, 1f, 0),
                AttachPoint.Feet => new Vector3(0, 0.1f, 0),
                AttachPoint.Weapon => Vector3.zero, // Handled elsewhere
                _ => new Vector3(0, 1f, 0)
            };
        }

        private static Vector3 GetShapeScale(AttachPoint point)
        {
            return point switch
            {
                AttachPoint.Head => new Vector3(0.3f, 0.3f, 0.3f),
                AttachPoint.Body => new Vector3(0.5f, 1f, 0.3f),
                AttachPoint.Feet => new Vector3(0.5f, 0.1f, 0.5f),
                _ => new Vector3(0.5f, 0.5f, 0.5f)
            };
        }

        private static Element GetElementForStatus(StatusType type)
        {
            return type switch
            {
                StatusType.Burning => Element.Fire,
                StatusType.Freezing => Element.Frost,
                StatusType.Poisoned => Element.Poison,
                StatusType.Blessed => Element.Spirit,
                StatusType.Cursed => Element.Shadow,
                StatusType.Electrified => Element.Lightning,
                StatusType.Fear => Element.Shadow,
                StatusType.Regenerating => Element.Spirit,
                _ => Element.None
            };
        }

        private static string GetStatusSound(StatusType type)
        {
            return type switch
            {
                StatusType.Burning => "burning_loop",
                StatusType.Freezing => "freezing_loop",
                StatusType.Electrified => "electric_loop",
                _ => null
            };
        }
    }
}
