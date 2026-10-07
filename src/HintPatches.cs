using System;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace PickYourSpirit
{
    /// <summary>
    /// The key hint (bottom-right button list): with the Spirit Caller out, vanilla hides the secondary-attack line (the
    /// staff has none); this shows it and labels it "Next spirit", and puts the game's label back afterwards. Relabels
    /// only on a change, the way the game itself relabels hints (KeyHints.SetGamePadBindings). The line is shown only
    /// once the relabel worked. If the label isn't found or anything fails, the game's label is put back and the hint is
    /// left to the game for the rest of the session (logged once). Never throws.
    /// </summary>
    [HarmonyPatch]
    internal static class HintPatches
    {
        /// <summary>The hints we last saw: the HUD is new on every world load, with the game's labels.</summary>
        private static KeyHints s_hints;
        private static bool s_relabelled;

        /// <summary>Set once the hint can't be handled: from then on it's the game's own.</summary>
        private static bool s_off;

        [HarmonyPatch(typeof(KeyHints), "UpdateHints")]
        [HarmonyPostfix]
        private static void UpdateHints(KeyHints __instance)
        {
            if (s_off) return;
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
                if (staffOut != s_relabelled) Relabel(__instance, staffOut);
                if (staffOut && !s_off)
                {
                    __instance.m_secondaryAttackKB.SetActive(true);
                    __instance.m_secondaryAttackGP.SetActive(true);
                }
            }
            catch (Exception e)
            {
                TurnOff(__instance, $"The 'Next spirit' key hint failed; the hint is left to the game from now on: {e}");
            }
        }

        private static void Relabel(KeyHints hints, bool spirit)
        {
            if (!Labels(hints, out var keyboard, out var gamepad))
            {
                TurnOff(hints, "The key hint's label wasn't found (a game update?): the hint is left to the game");
                return;
            }
            SetText(keyboard, spirit ? SpiritRules.HintLabel : SpiritRules.VanillaKeyboardHint);
            SetText(gamepad, spirit ? SpiritRules.SpiritGamepadHint : SpiritRules.VanillaGamepadHint);
            s_relabelled = spirit;
        }

        /// <summary>The keyboard and controller labels of the secondary-attack hint (KeyHintsBase.prefab), if both exist.</summary>
        private static bool Labels(KeyHints hints, out TMP_Text keyboard, out TMP_Text gamepad)
        {
            var keyboardText = hints.m_secondaryAttackKB != null ? hints.m_secondaryAttackKB.transform.Find("Text") : null;
            keyboard = keyboardText != null ? keyboardText.GetComponent<TMP_Text>() : null;
            gamepad = hints.m_secondaryAttackGP != null ? hints.m_secondaryAttackGP.GetComponent<TMP_Text>() : null;
            return keyboard != null && gamepad != null;
        }

        /// <summary>As the game does it: forget the cached original, set the new text, localize ($tokens) and cache it.</summary>
        private static void SetText(TMP_Text text, string value)
        {
            Localization.instance.RemoveTextFromCache(text);
            text.text = value;
            Localization.instance.Localize(text.transform);
        }

        /// <summary>Stop for the session: log once, and put the game's label back if ours is showing (best effort).</summary>
        private static void TurnOff(KeyHints hints, string message)
        {
            s_off = true;
            Plugin.Log.LogWarning(message);
            if (!s_relabelled) return;
            try
            {
                if (Labels(hints, out var keyboard, out var gamepad))
                {
                    SetText(keyboard, SpiritRules.VanillaKeyboardHint);
                    SetText(gamepad, SpiritRules.VanillaGamepadHint);
                }
            }
            catch (Exception)
            {
                // Already logged above; the label may stay "Next spirit" for this HUD.
            }
            s_relabelled = false;
        }
    }
}
