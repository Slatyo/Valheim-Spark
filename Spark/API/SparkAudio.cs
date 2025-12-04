using Spark.Core;
using Spark.Core.Configs;
using Spark.Internal;
using UnityEngine;

namespace Spark.API
{
    /// <summary>
    /// API for audio effects.
    /// </summary>
    public static class SparkAudio
    {
        /// <summary>
        /// Plays a sound at a position.
        /// </summary>
        /// <param name="soundId">Sound identifier.</param>
        /// <param name="position">World position.</param>
        /// <param name="config">Optional audio configuration.</param>
        public static void Play(string soundId, Vector3 position, AudioConfig config = null)
        {
            if (string.IsNullOrEmpty(soundId))
            {
                Plugin.Log?.LogWarning("SparkAudio.Play: soundId is null or empty");
                return;
            }

            config ??= new AudioConfig();
            AudioManager.PlaySound(soundId, position, config);
        }

        /// <summary>
        /// Plays a sound attached to a GameObject (follows it).
        /// </summary>
        /// <param name="soundId">Sound identifier.</param>
        /// <param name="target">Target GameObject.</param>
        /// <param name="loop">Whether to loop the sound.</param>
        /// <returns>Audio handle for stopping.</returns>
        public static SparkAudioHandle PlayAttached(string soundId, GameObject target, bool loop = false)
        {
            if (string.IsNullOrEmpty(soundId) || target == null)
            {
                Plugin.Log?.LogWarning("SparkAudio.PlayAttached: invalid parameters");
                return null;
            }

            return AudioManager.PlayAttached(soundId, target, loop);
        }

        /// <summary>
        /// Plays a 2D UI sound.
        /// </summary>
        /// <param name="soundId">Sound identifier.</param>
        /// <param name="volume">Volume (0-1).</param>
        public static void PlayUI(string soundId, float volume = 1f)
        {
            if (string.IsNullOrEmpty(soundId))
            {
                Plugin.Log?.LogWarning("SparkAudio.PlayUI: soundId is null or empty");
                return;
            }

            AudioManager.PlayUI(soundId, volume);
        }

        /// <summary>
        /// Stops a specific sound on a GameObject.
        /// </summary>
        /// <param name="target">Target GameObject.</param>
        /// <param name="soundId">Sound identifier.</param>
        public static void Stop(GameObject target, string soundId)
        {
            if (target == null || string.IsNullOrEmpty(soundId)) return;
            AudioManager.StopSound(target, soundId);
        }

        /// <summary>
        /// Stops a sound by handle.
        /// </summary>
        /// <param name="handle">Audio handle.</param>
        public static void Stop(SparkAudioHandle handle)
        {
            handle?.Stop();
        }

        /// <summary>
        /// Stops all sounds on a GameObject.
        /// </summary>
        /// <param name="target">Target GameObject.</param>
        public static void StopAll(GameObject target)
        {
            if (target == null) return;
            AudioManager.StopAllSounds(target);
        }

        /// <summary>
        /// Sets volume for a sound category.
        /// </summary>
        /// <param name="category">Sound category.</param>
        /// <param name="volume">Volume (0-1).</param>
        public static void SetCategoryVolume(SoundCategory category, float volume)
        {
            AudioManager.SetCategoryVolume(category, Mathf.Clamp01(volume));
        }

        /// <summary>
        /// Gets volume for a sound category.
        /// </summary>
        /// <param name="category">Sound category.</param>
        /// <returns>Volume (0-1).</returns>
        public static float GetCategoryVolume(SoundCategory category)
        {
            return AudioManager.GetCategoryVolume(category);
        }
    }
}
