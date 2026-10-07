using System;
using HarmonyLib;

namespace PickYourSpirit
{
    /// <summary>
    /// Middle mouse (the game's secondary attack; controller: its secondary-attack button) with the Spirit Caller in hand
    /// moves to the next pick. Reads the press after Player.SetControls has applied vanilla's rules (inventory, chat,
    /// radial menu, sitting, emoting, steering), so it counts only when vanilla would. The press itself is left alone: the
    /// staff has no secondary attack, so vanilla does nothing with it. Never throws.
    /// </summary>
    [HarmonyPatch]
    internal static class ButtonPatches
    {
        private static AccessTools.FieldRef<Character, bool> s_secondaryAttack;
        private static bool s_errorLogged;

        [HarmonyPatch(typeof(Player), nameof(Player.SetControls))]
        [HarmonyPostfix]
        private static void SetControls(Player __instance)
        {
            try
            {
                if (s_secondaryAttack == null) s_secondaryAttack = AccessTools.FieldRefAccess<Character, bool>("m_secondaryAttack");
                if (__instance != Player.m_localPlayer || !s_secondaryAttack(__instance)) return;
                var weapon = __instance.GetCurrentWeapon();
                if (SpiritCaller.IsSpiritCaller(weapon)) SpiritCaller.Press(weapon);
            }
            catch (Exception e)
            {
                if (s_errorLogged) return;
                s_errorLogged = true;
                Plugin.Log.LogError($"Picking the next spirit failed (logged once): {e}");
            }
        }
    }
}
