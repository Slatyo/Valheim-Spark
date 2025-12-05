using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;
using Spark.Commands;
using Spark.Core;
using Spark.Internal;

namespace Spark
{
    /// <summary>
    /// Spark - Shared VFX and Audio Effects Framework for Valheim Mod Ecosystem.
    /// Provides elemental particles, weapon effects, creature auras, impacts, and audio.
    /// </summary>
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("com.slatyo.munin")]
    [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Minor)]
    public class Plugin : BaseUnityPlugin
    {
        /// <summary>Plugin GUID for BepInEx.</summary>
        public const string PluginGUID = "com.slatyo.spark";
        /// <summary>Plugin display name.</summary>
        public const string PluginName = "Spark";
        /// <summary>Plugin version.</summary>
        public const string PluginVersion = "1.0.0";

        /// <summary>Logger instance for Spark.</summary>
        public static ManualLogSource Log { get; private set; }

        /// <summary>Plugin instance.</summary>
        public static Plugin Instance { get; private set; }

        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            Log.LogInfo($"{PluginName} v{PluginVersion} is loading...");

            // Initialize configuration
            SparkConfig.Initialize(Config);

            // Initialize core systems
            EffectPool.Initialize();
            AudioPool.Initialize();
            TextureLoader.Initialize();

            // Initialize Harmony patches (if any needed)
            _harmony = new Harmony(PluginGUID);
            _harmony.PatchAll();

            // Register test commands
            SparkCommands.Register();

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded successfully");
        }

        private void OnDestroy()
        {
            SparkCommands.Unregister();
            _harmony?.UnpatchSelf();
            EffectPool.Cleanup();
            AudioPool.Cleanup();
            TextureLoader.Cleanup();
            ShaderUtils.ClearCache();
        }

        private void Update()
        {
            // Update active effects (LOD, culling, etc.)
            EffectPool.Update();
        }
    }
}
