using System;
using HarmonyLib;

namespace PickYourSpirit
{
    /// <summary>
    /// The cast: before SpawnAbility starts its spawn (which rolls m_spawnPrefab[Random]), the local player's Spirit Caller
    /// cast gets a one-entry list with the picked spirit. Only this cast's own copy of the list is replaced, never the
    /// staff's. Random, another staff or another caster: untouched. Never throws (on error: vanilla random).
    /// </summary>
    [HarmonyPatch]
    internal static class CastPatches
    {
        private static bool s_errorLogged;

        [HarmonyPatch(typeof(SpawnAbility), nameof(SpawnAbility.Setup))]
        [HarmonyPrefix]
        private static void Setup(SpawnAbility __instance, Character owner, ItemDrop.ItemData item)
        {
            try
            {
                var pick = SpiritCaller.Pick;
                if (pick == null || owner == null || owner != Player.m_localPlayer || !SpiritCaller.IsSpiritCaller(item)) return;
                var list = __instance.m_spawnPrefab;
                var i = SpiritRules.Resolve(pick, SpiritCaller.Names(list));
                if (i < 0) return;
                __instance.m_spawnPrefab = new[] { list[i] };
            }
            catch (Exception e)
            {
                if (s_errorLogged) return;
                s_errorLogged = true;
                Plugin.Log.LogError($"Applying the picked spirit failed (logged once; the cast stays random): {e}");
            }
        }
    }
}
