using System.Collections.Generic;
using UnityEngine;

namespace Spark.Core
{
    /// <summary>
    /// Manages pooling of audio sources.
    /// </summary>
    public static class AudioPool
    {
        private static readonly Queue<AudioSource> AvailableSources = new Queue<AudioSource>();
        private static readonly List<AudioSource> ActiveSources = new List<AudioSource>();
        private static GameObject _poolContainer;
        private static int _maxSources = 32;

        /// <summary>
        /// Initializes the audio pool.
        /// </summary>
        public static void Initialize()
        {
            _poolContainer = new GameObject("SparkAudioPool");
            Object.DontDestroyOnLoad(_poolContainer);

            // Pre-create some audio sources
            for (int i = 0; i < 10; i++)
            {
                var source = CreateAudioSource();
                AvailableSources.Enqueue(source);
            }

            Plugin.Log?.LogDebug("Audio pool initialized");
        }

        /// <summary>
        /// Gets an audio source from the pool or creates a new one.
        /// </summary>
        public static AudioSource Get()
        {
            if (!SparkConfig.AudioEnabled) return null;

            // Check limit
            if (ActiveSources.Count >= _maxSources)
            {
                Plugin.Log?.LogDebug("Max audio sources reached");
                return null;
            }

            AudioSource source;
            if (AvailableSources.Count > 0)
            {
                source = AvailableSources.Dequeue();
            }
            else
            {
                source = CreateAudioSource();
            }

            source.gameObject.SetActive(true);
            ActiveSources.Add(source);
            return source;
        }

        /// <summary>
        /// Returns an audio source to the pool.
        /// </summary>
        public static void Return(AudioSource source)
        {
            if (source == null) return;

            source.Stop();
            source.clip = null;
            source.transform.SetParent(_poolContainer.transform);
            source.gameObject.SetActive(false);
            ActiveSources.Remove(source);
            AvailableSources.Enqueue(source);
        }

        private static AudioSource CreateAudioSource()
        {
            var go = new GameObject("SparkAudioSource");
            go.transform.SetParent(_poolContainer.transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f; // 3D by default
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1f;
            source.maxDistance = 30f;
            go.SetActive(false);
            return source;
        }

        /// <summary>
        /// Updates the pool - returns finished sources.
        /// </summary>
        public static void Update()
        {
            for (int i = ActiveSources.Count - 1; i >= 0; i--)
            {
                var source = ActiveSources[i];
                if (source == null)
                {
                    ActiveSources.RemoveAt(i);
                    continue;
                }

                // Return non-looping sources that finished playing
                if (!source.isPlaying && !source.loop)
                {
                    Return(source);
                }
            }
        }

        /// <summary>
        /// Gets count of pooled sources.
        /// </summary>
        public static int GetPooledCount() => AvailableSources.Count;

        /// <summary>
        /// Gets count of active sources.
        /// </summary>
        public static int GetActiveCount() => ActiveSources.Count;

        /// <summary>
        /// Cleans up the pool.
        /// </summary>
        public static void Cleanup()
        {
            while (AvailableSources.Count > 0)
            {
                var source = AvailableSources.Dequeue();
                if (source != null && source.gameObject != null)
                    Object.Destroy(source.gameObject);
            }

            foreach (var source in ActiveSources)
            {
                if (source != null && source.gameObject != null)
                    Object.Destroy(source.gameObject);
            }
            ActiveSources.Clear();

            if (_poolContainer != null)
                Object.Destroy(_poolContainer);
        }
    }
}
