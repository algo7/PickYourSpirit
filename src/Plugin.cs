using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PickYourSpirit
{
    [BepInPlugin(Guid, Name, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Guid = "algo7.pickyourspirit";
        public const string Name = "PickYourSpirit";
        public const string PluginVersion = PluginInfo.Version; // from the git tag, generated at build time (MinVer)

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            if (Application.isBatchMode)
            {
                Log.LogInfo($"{Name} loaded (v{PluginVersion}): dedicated server, nothing to do");
                return;
            }
            var harmony = new Harmony(Guid);
            if (!TryPatch(harmony, typeof(CastPatches), "cast hook"))
            {
                Log.LogError("Picking spirits is off: without the cast hook a pick would do nothing");
                return;
            }
            TryPatch(harmony, typeof(ButtonPatches), "middle mouse hook");
            SceneManager.sceneLoaded += OnSceneLoaded;
            Log.LogInfo($"{Name} loaded (v{PluginVersion})");
        }

        private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

        /// <summary>Entering a world (or the main menu) starts on Random: nothing is saved.</summary>
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => SpiritCaller.Pick = null;

        /// <summary>True when patched; false (logged) on failure.</summary>
        private static bool TryPatch(Harmony harmony, Type patches, string what)
        {
            try
            {
                harmony.PatchAll(patches);
                return true;
            }
            catch (Exception e)
            {
                Log.LogError($"Could not install the {what}: {e}");
                return false;
            }
        }
    }
}
