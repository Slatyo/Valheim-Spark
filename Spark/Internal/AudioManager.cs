using System.Collections.Generic;
using Spark.API;
using Spark.Core;
using Spark.Core.Configs;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Manages audio playback.
    /// </summary>
    internal static class AudioManager
    {
        private static readonly Dictionary<SoundCategory, float> CategoryVolumes = new Dictionary<SoundCategory, float>();
        private static readonly Dictionary<string, AudioClip> LoadedClips = new Dictionary<string, AudioClip>();
        private static readonly Dictionary<int, List<SparkAudioHandle>> AttachedSounds = new Dictionary<int, List<SparkAudioHandle>>();

        static AudioManager()
        {
            // Initialize default volumes
            foreach (SoundCategory category in System.Enum.GetValues(typeof(SoundCategory)))
            {
                CategoryVolumes[category] = 1f;
            }
        }

        /// <summary>
        /// Plays a sound at a position.
        /// </summary>
        public static void PlaySound(string soundId, Vector3 position, AudioConfig config)
        {
            if (!SparkConfig.AudioEnabled) return;

            var clip = GetClip(soundId);
            if (clip == null) return;

            var source = AudioPool.Get();
            if (source == null) return;

            source.transform.position = position;
            source.clip = clip;
            source.volume = config.Volume * GetCategoryVolume(config.Category) * SparkConfig.MasterVolume;
            source.pitch = config.Pitch + Random.Range(-config.PitchVariation, config.PitchVariation);
            source.maxDistance = config.MaxDistance;
            source.spatialBlend = config.SpatialBlend;
            source.loop = false;
            source.Play();
        }

        /// <summary>
        /// Plays a sound attached to a GameObject.
        /// </summary>
        public static SparkAudioHandle PlayAttached(string soundId, GameObject target, bool loop)
        {
            if (!SparkConfig.AudioEnabled || target == null) return null;

            var clip = GetClip(soundId);
            if (clip == null) return null;

            var source = AudioPool.Get();
            if (source == null) return null;

            source.transform.SetParent(target.transform);
            source.transform.localPosition = Vector3.zero;
            source.clip = clip;
            source.volume = SparkConfig.MasterVolume;
            source.loop = loop;
            source.Play();

            var handle = new SparkAudioHandle
            {
                Source = source,
                SoundId = soundId
            };

            // Track attached sounds
            int id = target.GetInstanceID();
            if (!AttachedSounds.TryGetValue(id, out var list))
            {
                list = new List<SparkAudioHandle>();
                AttachedSounds[id] = list;
            }
            list.Add(handle);

            return handle;
        }

        /// <summary>
        /// Plays a 2D UI sound.
        /// </summary>
        public static void PlayUI(string soundId, float volume)
        {
            if (!SparkConfig.AudioEnabled) return;

            var clip = GetClip(soundId);
            if (clip == null) return;

            var source = AudioPool.Get();
            if (source == null) return;

            source.spatialBlend = 0f; // 2D
            source.clip = clip;
            source.volume = volume * GetCategoryVolume(SoundCategory.UI) * SparkConfig.MasterVolume;
            source.loop = false;
            source.Play();
        }

        /// <summary>
        /// Stops a specific sound on a target.
        /// </summary>
        public static void StopSound(GameObject target, string soundId)
        {
            if (target == null) return;

            int id = target.GetInstanceID();
            if (AttachedSounds.TryGetValue(id, out var list))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].SoundId == soundId)
                    {
                        list[i].Stop();
                        if (list[i].Source != null)
                        {
                            list[i].Source.transform.SetParent(null);
                            AudioPool.Return(list[i].Source);
                        }
                        list.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Stops all sounds on a target.
        /// </summary>
        public static void StopAllSounds(GameObject target)
        {
            if (target == null) return;

            int id = target.GetInstanceID();
            if (AttachedSounds.TryGetValue(id, out var list))
            {
                foreach (var handle in list)
                {
                    handle.Stop();
                    if (handle.Source != null)
                    {
                        handle.Source.transform.SetParent(null);
                        AudioPool.Return(handle.Source);
                    }
                }
                list.Clear();
            }
        }

        /// <summary>
        /// Sets volume for a category.
        /// </summary>
        public static void SetCategoryVolume(SoundCategory category, float volume)
        {
            CategoryVolumes[category] = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// Gets volume for a category.
        /// </summary>
        public static float GetCategoryVolume(SoundCategory category)
        {
            return CategoryVolumes.TryGetValue(category, out var vol) ? vol : 1f;
        }

        private static AudioClip GetClip(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return null;

            if (LoadedClips.TryGetValue(soundId, out var clip))
                return clip;

            // TODO: Load from resources or external files
            // For now, return null - clips would be loaded from BepInEx/config/Spark/Audio/
            Plugin.Log?.LogDebug($"Audio clip not found: {soundId}");
            return null;
        }

        /// <summary>
        /// Registers an audio clip.
        /// </summary>
        public static void RegisterClip(string soundId, AudioClip clip)
        {
            if (!string.IsNullOrEmpty(soundId) && clip != null)
            {
                LoadedClips[soundId] = clip;
            }
        }
    }
}
