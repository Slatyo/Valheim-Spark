using System.Collections.Generic;
using Spark.API;
using UnityEngine;

namespace Spark.Core
{
    /// <summary>
    /// Manages pooling of particle systems and other effect objects.
    /// </summary>
    public static class EffectPool
    {
        private static readonly Dictionary<Element, Queue<GameObject>> ElementPools = new Dictionary<Element, Queue<GameObject>>();
        private static readonly List<GameObject> ActiveEffects = new List<GameObject>();
        private static GameObject _poolContainer;

        /// <summary>
        /// Initializes the effect pool.
        /// </summary>
        public static void Initialize()
        {
            _poolContainer = new GameObject("SparkEffectPool");
            Object.DontDestroyOnLoad(_poolContainer);
            _poolContainer.SetActive(false);

            // Initialize pool queues for each element
            foreach (Element element in System.Enum.GetValues(typeof(Element)))
            {
                ElementPools[element] = new Queue<GameObject>();
            }

            Plugin.Log?.LogDebug("Effect pool initialized");
        }

        /// <summary>
        /// Pre-warms the pool for an element.
        /// </summary>
        public static void Prewarm(Element element, int count)
        {
            if (!ElementPools.ContainsKey(element)) return;

            for (int i = 0; i < count; i++)
            {
                var effect = CreateEffectObject(element);
                ReturnToPool(element, effect);
            }

            Plugin.Log?.LogDebug($"Pre-warmed {count} {element} effects");
        }

        /// <summary>
        /// Gets an effect from the pool or creates a new one.
        /// </summary>
        public static GameObject Get(Element element)
        {
            if (!SparkConfig.EffectsEnabled) return null;

            // Check active count limit
            if (ActiveEffects.Count >= SparkConfig.MaxActiveParticleSystems)
            {
                Plugin.Log?.LogDebug("Max particle systems reached, skipping effect");
                return null;
            }

            GameObject effect;
            if (ElementPools.TryGetValue(element, out var pool) && pool.Count > 0)
            {
                effect = pool.Dequeue();
                effect.transform.SetParent(null);
            }
            else
            {
                effect = CreateEffectObject(element);
            }

            effect.SetActive(true);
            ActiveEffects.Add(effect);
            return effect;
        }

        /// <summary>
        /// Returns an effect to the pool.
        /// </summary>
        public static void Return(Element element, GameObject effect)
        {
            if (effect == null) return;

            effect.SetActive(false);
            effect.transform.SetParent(_poolContainer.transform);
            ActiveEffects.Remove(effect);

            if (ElementPools.TryGetValue(element, out var pool))
            {
                pool.Enqueue(effect);
            }
        }

        /// <summary>
        /// Returns an effect to the pool (auto-detect element).
        /// </summary>
        public static void Return(GameObject effect)
        {
            if (effect == null) return;

            effect.SetActive(false);
            effect.transform.SetParent(_poolContainer.transform);
            ActiveEffects.Remove(effect);

            // Return to None pool as fallback
            if (ElementPools.TryGetValue(Element.None, out var pool))
            {
                pool.Enqueue(effect);
            }
        }

        private static void ReturnToPool(Element element, GameObject effect)
        {
            effect.SetActive(false);
            effect.transform.SetParent(_poolContainer.transform);

            if (ElementPools.TryGetValue(element, out var pool))
            {
                pool.Enqueue(effect);
            }
        }

        private static GameObject CreateEffectObject(Element element)
        {
            var go = new GameObject($"SparkEffect_{element}");

            // Add particle system component
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.maxParticles = SparkConfig.MaxParticlesPerSystem;

            return go;
        }

        /// <summary>
        /// Updates active effects (LOD, culling).
        /// </summary>
        public static void Update()
        {
            if (!SparkConfig.EffectsEnabled) return;

            var camera = Camera.main;
            if (camera == null) return;

            var cameraPos = camera.transform.position;
            var lod = SparkConfig.LOD;

            for (int i = ActiveEffects.Count - 1; i >= 0; i--)
            {
                var effect = ActiveEffects[i];
                if (effect == null)
                {
                    ActiveEffects.RemoveAt(i);
                    continue;
                }

                var distance = Vector3.Distance(effect.transform.position, cameraPos);

                // Apply LOD
                if (distance > lod.CullDistance)
                {
                    effect.SetActive(false);
                }
                else if (distance > lod.MinimalDistance)
                {
                    // Minimal quality - could reduce particles here
                    effect.SetActive(true);
                }
                else
                {
                    effect.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Clears all pooled effects.
        /// </summary>
        public static void Clear()
        {
            foreach (var pool in ElementPools.Values)
            {
                while (pool.Count > 0)
                {
                    var effect = pool.Dequeue();
                    if (effect != null)
                        Object.Destroy(effect);
                }
            }

            foreach (var effect in ActiveEffects)
            {
                if (effect != null)
                    Object.Destroy(effect);
            }
            ActiveEffects.Clear();

            Plugin.Log?.LogDebug("Effect pool cleared");
        }

        /// <summary>
        /// Cleans up the pool.
        /// </summary>
        public static void Cleanup()
        {
            Clear();
            if (_poolContainer != null)
                Object.Destroy(_poolContainer);
        }

        /// <summary>
        /// Gets pool statistics.
        /// </summary>
        public static PoolStats GetStats()
        {
            int totalPooled = 0;
            foreach (var pool in ElementPools.Values)
            {
                totalPooled += pool.Count;
            }

            return new PoolStats
            {
                TotalPooled = totalPooled,
                ActiveEffects = ActiveEffects.Count,
                TotalAudioSources = AudioPool.GetPooledCount(),
                ActiveAudio = AudioPool.GetActiveCount()
            };
        }
    }
}
