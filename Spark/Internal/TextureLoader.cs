using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Loads particle textures from the plugin's Assets folder.
    /// </summary>
    internal static class TextureLoader
    {
        private static readonly Dictionary<string, Texture2D> LoadedTextures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Material> TextureMaterials = new Dictionary<string, Material>();
        private static MethodInfo _loadImageMethod;
        private static bool _initialized;
        private static string _assetsPath;

        /// <summary>
        /// Initialize the texture loader - call from Plugin.Awake().
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            // Find assets path relative to plugin DLL
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var pluginDir = Path.GetDirectoryName(assemblyLocation);
            _assetsPath = Path.Combine(pluginDir, "Assets", "Particles");

            // Get LoadImage method via reflection
            var imageConversionType = Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule");
            if (imageConversionType != null)
            {
                _loadImageMethod = imageConversionType.GetMethod("LoadImage",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(Texture2D), typeof(byte[]) },
                    null);
            }

            if (_loadImageMethod == null)
            {
                Plugin.Log?.LogWarning("TextureLoader: Could not find ImageConversion.LoadImage method - using fallback textures");
            }

            _initialized = true;

            // Pre-load common textures
            LoadAllTextures();
        }

        /// <summary>
        /// Load all PNG textures from the assets folder.
        /// </summary>
        private static void LoadAllTextures()
        {
            if (!Directory.Exists(_assetsPath))
            {
                Plugin.Log?.LogWarning($"TextureLoader: Assets folder not found at {_assetsPath}");
                return;
            }

            var pngFiles = Directory.GetFiles(_assetsPath, "*.png");
            Plugin.Log?.LogInfo($"TextureLoader: Found {pngFiles.Length} texture files");

            foreach (var filePath in pngFiles)
            {
                var textureName = Path.GetFileNameWithoutExtension(filePath);
                LoadTexture(textureName, filePath);
            }
        }

        private static void LoadTexture(string name, string filePath)
        {
            if (LoadedTextures.ContainsKey(name)) return;
            if (_loadImageMethod == null) return;

            try
            {
                var bytes = File.ReadAllBytes(filePath);
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.name = name;

                var result = (bool)_loadImageMethod.Invoke(null, new object[] { texture, bytes });

                if (result)
                {
                    texture.filterMode = FilterMode.Bilinear;
                    texture.wrapMode = TextureWrapMode.Clamp;
                    LoadedTextures[name] = texture;
                    Plugin.Log?.LogDebug($"TextureLoader: Loaded {name}");
                }
                else
                {
                    Plugin.Log?.LogWarning($"TextureLoader: Failed to load {name}");
                    UnityEngine.Object.Destroy(texture);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"TextureLoader: Error loading {name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a loaded texture by name (without extension).
        /// </summary>
        public static Texture2D GetTexture(string name)
        {
            if (LoadedTextures.TryGetValue(name, out var texture))
                return texture;
            return null;
        }

        /// <summary>
        /// Get or create a material with the specified texture.
        /// </summary>
        public static Material GetMaterial(string textureName, Color? tint = null)
        {
            string key = tint.HasValue ? $"{textureName}_{tint.Value}" : textureName;

            if (TextureMaterials.TryGetValue(key, out var existingMat))
                return existingMat;

            var texture = GetTexture(textureName);
            var material = ShaderUtils.CreateParticleMaterial(tint ?? Color.white);

            if (material != null && texture != null)
            {
                material.mainTexture = texture;
            }

            TextureMaterials[key] = material;
            return material;
        }

        /// <summary>
        /// Check if a texture is loaded.
        /// </summary>
        public static bool HasTexture(string name)
        {
            return LoadedTextures.ContainsKey(name);
        }

        /// <summary>
        /// Get all loaded texture names.
        /// </summary>
        public static IEnumerable<string> GetLoadedTextureNames()
        {
            return LoadedTextures.Keys;
        }

        /// <summary>
        /// Cleanup all loaded textures.
        /// </summary>
        public static void Cleanup()
        {
            foreach (var texture in LoadedTextures.Values)
            {
                if (texture != null)
                    UnityEngine.Object.Destroy(texture);
            }
            LoadedTextures.Clear();

            foreach (var material in TextureMaterials.Values)
            {
                if (material != null)
                    UnityEngine.Object.Destroy(material);
            }
            TextureMaterials.Clear();

            _initialized = false;
        }
    }
}
