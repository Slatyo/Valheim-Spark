using HarmonyLib;
using Spark.Internal;
using UnityEngine;

namespace Spark.Patches
{
    /// <summary>
    /// Patches for VisEquipment to handle automatic effect reapplication on holster/unholster.
    /// </summary>
    [HarmonyPatch]
    internal static class VisEquipmentPatches
    {
        /// <summary>
        /// Called when an item visual is attached (draw weapon, holster weapon, equip armor, etc.)
        /// This is where we reapply tracked effects to the new visual instance.
        /// </summary>
        [HarmonyPatch(typeof(VisEquipment), "AttachItem")]
        [HarmonyPostfix]
        static void AttachItem_Postfix(VisEquipment __instance, GameObject __result, int itemHash)
        {
            if (__result == null || itemHash == 0)
                return;

            // Get the player this VisEquipment belongs to
            var player = __instance.GetComponentInParent<Player>();
            if (player == null) player = __instance.GetComponent<Player>();

            // For now, only handle local player effects
            // TODO: Could extend to support other players/creatures
            if (player == null || player != Player.m_localPlayer)
                return;

            // Skip non-weapon items (armor, helmet, etc.) for holster handling
            // These don't get recreated on holster/unholster
            string visualName = __result.name.ToLower();
            if (IsArmorOrClothing(visualName))
                return;

            // Check if this visual already has a Spark effect
            foreach (Transform child in __result.transform)
            {
                if (child.name.StartsWith("SparkEffect"))
                    return; // Already has effect
            }

            // Look up if we have a tracked effect for this item
            var trackedEffect = ItemEffectTracker.GetTrackedEffectByHash(itemHash, player);
            if (trackedEffect != null)
            {
                ItemEffectTracker.ApplyEffectToVisual(trackedEffect, __result);
                Plugin.Log?.LogDebug($"[VisEquipmentPatch] Reapplied effect to {__result.name} (hash: {itemHash})");
            }
        }

        private static bool IsArmorOrClothing(string name)
        {
            return name.Contains("armor") ||
                   name.Contains("helmet") ||
                   name.Contains("cape") ||
                   name.Contains("chest") ||
                   name.Contains("legs") ||
                   name.Contains("shoulder") ||
                   name.Contains("body") ||
                   name.Contains("head") ||
                   name.Contains("hair") ||
                   name.Contains("beard") ||
                   name.Contains("skin");
        }
    }
}
