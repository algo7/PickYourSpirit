using System;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace PickYourSpirit
{
    /// <summary>
    /// The key hint (bottom-right button list): with the Spirit Caller out, vanilla hides the secondary-attack line (the
    /// staff has none); this shows it and labels it "Next spirit", and puts the game's label back afterwards. Relabels
    /// only on a change, the way the game itself relabels hints (KeyHints.SetGamePadBindings). Never throws.
    /// </summary>
    [HarmonyPatch]
    internal static class HintPatches
    {
        /// <summary>The hints we last saw: the HUD is new on every world load, with the game's labels.</summary>
        private static KeyHints s_hints;
        private static bool s_relabelled;
        private static bool s_errorLogged;

        [HarmonyPatch(typeof(KeyHints), "UpdateHints")]
        [HarmonyPostfix]
        private static void UpdateHints(KeyHints __instance)
        {
            try
            {
                if (__instance != s_hints)
                {
                    s_hints = __instance;
                    s_relabelled = false;
                }
                var player = Player.m_localPlayer;
                var staffOut = player != null && __instance.m_combatHints != null && __instance.m_combatHints.activeSelf
                    && SpiritCaller.IsSpiritCaller(player.GetCurrentWeapon());
                if (staffOut)
                {
                    __instance.m_secondaryAttackKB.SetActive(true);
                    __instance.m_secondaryAttackGP.SetActive(true);
                }
                if (staffOut != s_relabelled) Relabel(__instance, staffOut);
            }
            catch (Exception e)
            {
                LogOnce($"The 'Next spirit' key hint failed (logged once): {e}");
            }
        }

        private static void Relabel(KeyHints hints, bool spirit)
        {
            s_relabelled = spirit;
            var keyboardText = hints.m_secondaryAttackKB.transform.Find("Text");
            var keyboard = keyboardText != null ? keyboardText.GetComponent<TMP_Text>() : null;
            var gamepad = hints.m_secondaryAttackGP.GetComponent<TMP_Text>();
            if (keyboard == null || gamepad == null)
            {
                LogOnce("The key hint's label wasn't found (a game update?): it keeps the game's label");
                return;
            }
            SetText(keyboard, spirit ? SpiritRules.HintLabel : SpiritRules.VanillaKeyboardHint);
            SetText(gamepad, spirit ? SpiritRules.SpiritGamepadHint : SpiritRules.VanillaGamepadHint);
        }

        /// <summary>As the game does it: forget the cached original, set the new text, localize ($tokens) and cache it.</summary>
        private static void SetText(TMP_Text text, string value)
        {
            Localization.instance.RemoveTextFromCache(text);
            text.text = value;
            Localization.instance.Localize(text.transform);
        }

        private static void LogOnce(string message)
        {
            if (s_errorLogged) return;
            s_errorLogged = true;
            Plugin.Log.LogWarning(message);
        }
    }
}
