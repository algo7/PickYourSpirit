using System;
using System.Collections.Generic;

namespace PickYourSpirit
{
    /// <summary>
    /// The pick as plain rules over the staff's spirit prefab names (no Unity), so they're unit tested. A pick is a
    /// prefab name, or null for Random (the game's own roll).
    /// </summary>
    internal static class SpiritRules
    {
        /// <summary>The mod's own word for the press (English only; spirit names come from the game).</summary>
        public const string HintLabel = "Next spirit";

        /// <summary>The controller hint's key part, as in the game's own label (KeyHintsBase.prefab).</summary>
        public const string GamepadKey = " <mspace=0.6em> $KEY_SecondaryAttack</mspace>";

        /// <summary>The game's own labels of the secondary-attack hint, put back when the staff is put away.</summary>
        public const string VanillaKeyboardHint = "$settings_secondaryattack";
        public const string VanillaGamepadHint = VanillaKeyboardHint + GamepadKey;

        /// <summary>The controller hint while the Spirit Caller is out.</summary>
        public const string SpiritGamepadHint = HintLabel + GamepadKey;

        /// <summary>What a press cycles through after Random: each name once, in the staff's order; nulls and blanks skipped.</summary>
        public static List<string> Choices(IEnumerable<string> names)
        {
            var choices = new List<string>();
            if (names == null) return choices;
            foreach (var name in names)
                if (!string.IsNullOrEmpty(name) && !choices.Contains(name)) choices.Add(name);
            return choices;
        }

        /// <summary>The pick after one press: Random → first → … → last → Random. A pick that isn't a choice → Random.</summary>
        public static string Next(string pick, IReadOnlyList<string> choices)
        {
            if (choices == null || choices.Count == 0) return null;
            if (pick == null) return choices[0];
            var i = IndexOf(choices, pick);
            return i >= 0 && i + 1 < choices.Count ? choices[i + 1] : null;
        }

        /// <summary>
        /// Where the pick is in a cast's spawn list (names aligned with the list; null for a missing prefab): its first
        /// index, or -1 (Random, or not in this list: the cast stays random).
        /// </summary>
        public static int Resolve(string pick, IReadOnlyList<string> names)
        {
            if (pick == null || names == null) return -1;
            return IndexOf(names, pick);
        }

        /// <summary>The centre message. spirit: the spirit's name as the game has it ($token or plain), null for Random.</summary>
        public static string Message(string spirit) => $"{HintLabel}: {spirit ?? "Random"}";

        private static int IndexOf(IReadOnlyList<string> names, string name)
        {
            for (var i = 0; i < names.Count; i++)
                if (string.Equals(names[i], name, StringComparison.Ordinal)) return i;
            return -1;
        }
    }
}
