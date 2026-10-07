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

        /// <summary>
        /// Harmony asks this before patching: the press field is looked up once here. If a game update removed it, the
        /// hook isn't installed at all (one log line) instead of failing on every physics step.
        /// </summary>
        private static bool Prepare()
        {
            if (s_secondaryAttack != null) return true;
            try
            {
                s_secondaryAttack = AccessTools.FieldRefAccess<Character, bool>("m_secondaryAttack");
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Middle mouse can't pick spirits: the game's press field wasn't found (a game update?): {e.Message}");
                return false;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.SetControls))]
        [HarmonyPostfix]
        private static void SetControls(Player __instance)
        {
            try
            {
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
