using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Shader utilities with fallback support for Valheim runtime.
    /// </summary>
    internal static class ShaderUtils
    {
        private static Shader _cachedParticleShader;
        private static Material _cachedParticleMaterial;
        private static bool _shaderSearched;

        /// <summary>
        /// Gets a particle shader with fallback options.
        /// </summary>
        public static Shader GetParticleShader()
        {
            if (_shaderSearched && _cachedParticleShader != null)
                return _cachedParticleShader;

            _shaderSearched = true;

            // Try standard particle shaders first
            _cachedParticleShader = Shader.Find("Particles/Standard Unlit");
            if (_cachedParticleShader != null) return _cachedParticleShader;

            _cachedParticleShader = Shader.Find("Sprites/Default");
            if (_cachedParticleShader != null) return _cachedParticleShader;

            _cachedParticleShader = Shader.Find("Unlit/Color");
            if (_cachedParticleShader != null) return _cachedParticleShader;

            _cachedParticleShader = Shader.Find("Unlit/Texture");
            if (_cachedParticleShader != null) return _cachedParticleShader;

            // Try to get shader from a vanilla VFX prefab
            _cachedParticleShader = GetShaderFromVanillaPrefab();
            if (_cachedParticleShader != null) return _cachedParticleShader;

            // Last resort - get any shader from loaded materials
            _cachedParticleShader = GetAnyLoadedShader();

            return _cachedParticleShader;
        }

        /// <summary>
        /// Gets a reusable particle material.
        /// </summary>
        public static Material GetParticleMaterial()
        {
            if (_cachedParticleMaterial != null)
                return _cachedParticleMaterial;

            var shader = GetParticleShader();
            if (shader == null)
            {
                Plugin.Log?.LogWarning("Could not find any suitable shader for particles");
                return null;
            }

            _cachedParticleMaterial = new Material(shader);
            SetupAdditiveMaterial(_cachedParticleMaterial);
            return _cachedParticleMaterial;
        }

        /// <summary>
        /// Creates a new material with the particle shader.
        /// </summary>
        public static Material CreateParticleMaterial()
        {
            var shader = GetParticleShader();
            if (shader == null)
            {
                Plugin.Log?.LogWarning("Could not find any suitable shader for new material");
                return null;
            }

            var material = new Material(shader);
            SetupAdditiveMaterial(material);
            return material;
        }

        /// <summary>
        /// Creates a new material with the particle shader and a specific color.
        /// </summary>
        public static Material CreateParticleMaterial(Color color)
        {
            var material = CreateParticleMaterial();
            if (material != null)
            {
                material.SetColor("_Color", color);
                material.SetColor("_TintColor", color);
            }
            return material;
        }

        private static void SetupAdditiveMaterial(Material material)
        {
            if (material == null) return;

            // Set up additive blending for glow effect
            if (material.HasProperty("_SrcBlend"))
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend"))
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            if (material.HasProperty("_ZWrite"))
                material.SetInt("_ZWrite", 0);

            material.renderQueue = 3000;
        }

        private static Shader GetShaderFromVanillaPrefab()
        {
            // Try to get shader from vanilla VFX prefabs
            string[] vanillaPrefabs = {
                "vfx_FireballHit",
                "vfx_ice_hit",
                "fx_guardstone_activate",
                "vfx_spawn"
            };

            if (ZNetScene.instance == null)
                return null;

            foreach (var prefabName in vanillaPrefabs)
            {
                var prefab = ZNetScene.instance.GetPrefab(prefabName);
                if (prefab == null) continue;

                var ps = prefab.GetComponentInChildren<ParticleSystem>();
                if (ps == null) continue;

                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.shader != null)
                {
                    Plugin.Log?.LogInfo($"Found shader from vanilla prefab: {prefabName}");
                    return renderer.sharedMaterial.shader;
                }
            }

            return null;
        }

        private static Shader GetAnyLoadedShader()
        {
            // Find any particle system in the scene and grab its shader
            var allParticles = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var ps in allParticles)
            {
                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.shader != null)
                {
                    Plugin.Log?.LogInfo($"Found shader from existing particle: {ps.name}");
                    return renderer.sharedMaterial.shader;
                }
            }

            return null;
        }

        /// <summary>
        /// Clears cached shader references. Call when cleaning up.
        /// </summary>
        public static void ClearCache()
        {
            if (_cachedParticleMaterial != null)
            {
                Object.Destroy(_cachedParticleMaterial);
                _cachedParticleMaterial = null;
            }
            _cachedParticleShader = null;
            _shaderSearched = false;
        }
    }
}
