using System.Collections.Generic;
using PickYourSpirit;

internal static partial class Tests
{
    // The Spirit Caller's list in Valheim l-1.0.16 (staff_SpiritCaller_spawn's SpawnAbility).
    private static readonly string[] Staff = { "Bjorn_spiritcaller", "Moose_spiritcaller", "Wolf_spiritcaller", "Boar_spiritcaller" };

    private static void Test_Choices_StaffOrder()
    {
        Eq(string.Join(",", Staff), string.Join(",", SpiritRules.Choices(Staff)), "the staff's own order");
    }

    private static void Test_Choices_EachSpiritOnceNoBlanks()
    {
        // A mod could weight the roll by repeating a spirit, or a prefab could be missing (null name).
        var choices = SpiritRules.Choices(new[] { "Wolf_spiritcaller", null, "Wolf_spiritcaller", "", "Boar_spiritcaller" });
        Eq("Wolf_spiritcaller,Boar_spiritcaller", string.Join(",", choices), "repeats and blanks dropped");
        Eq(0, SpiritRules.Choices(null).Count, "no list: no choices");
    }

    private static void Test_Next_CyclesThroughEveryoneAndBackToRandom()
    {
        var choices = SpiritRules.Choices(Staff);
        string pick = null;
        var seen = new List<string>();
        for (var i = 0; i < 5; i++)
        {
            pick = SpiritRules.Next(pick, choices);
            seen.Add(pick ?? "Random");
        }
        Eq("Bjorn_spiritcaller,Moose_spiritcaller,Wolf_spiritcaller,Boar_spiritcaller,Random", string.Join(",", seen), "five presses");
    }

    private static void Test_Next_WeightedListStillReachesEveryone()
    {
        var choices = SpiritRules.Choices(new[] { "Wolf_spiritcaller", "Wolf_spiritcaller", "Boar_spiritcaller" });
        var pick = SpiritRules.Next(null, choices);
        Eq("Wolf_spiritcaller", pick, "first press");
        pick = SpiritRules.Next(pick, choices);
        Eq("Boar_spiritcaller", pick, "second press: not stuck on the repeated wolf");
        Eq(null, SpiritRules.Next(pick, choices), "third press: Random");
    }

    private static void Test_Next_UnknownPickGoesToRandom()
    {
        Eq(null, SpiritRules.Next("Troll_Summoned", SpiritRules.Choices(Staff)), "a pick the staff doesn't have (other staff, update)");
    }

    private static void Test_Next_NoChoicesStaysRandom()
    {
        Eq(null, SpiritRules.Next(null, new List<string>()), "empty list");
        Eq(null, SpiritRules.Next("Wolf_spiritcaller", new List<string>()), "empty list, old pick");
        Eq(null, SpiritRules.Next(null, null), "no list");
    }

    private static void Test_Resolve_FindsThePickInTheCastsList()
    {
        Eq(2, SpiritRules.Resolve("Wolf_spiritcaller", Staff), "wolf is third");
        Eq(1, SpiritRules.Resolve("Wolf_spiritcaller", new[] { null, "Wolf_spiritcaller" }), "index stays aligned past a missing prefab");
        Eq(0, SpiritRules.Resolve("Wolf_spiritcaller", new[] { "Wolf_spiritcaller", "Wolf_spiritcaller" }), "repeated: the first");
    }

    private static void Test_Resolve_RandomOrMissingLeavesTheCastAlone()
    {
        Eq(-1, SpiritRules.Resolve(null, Staff), "Random");
        Eq(-1, SpiritRules.Resolve("Troll_Summoned", Staff), "not in this cast's list");
        Eq(-1, SpiritRules.Resolve("Wolf_spiritcaller", null), "no list");
        Eq(-1, SpiritRules.Resolve("wolf_spiritcaller", Staff), "names match exactly (prefab names)");
    }

    private static void Test_Message_NamesThePick()
    {
        Eq("Next spirit: Random", SpiritRules.Message(null), "Random");
        Eq("Next spirit: $spiritcaller_wolf", SpiritRules.Message("$spiritcaller_wolf"), "the game's token, localized by the message HUD");
    }

    private static void Test_Hint_LabelsMatchTheGame()
    {
        // The game's own strings (KeyHintsBase.prefab): restored after the staff is put away.
        Eq("$settings_secondaryattack", SpiritRules.VanillaKeyboardHint, "keyboard");
        Eq("$settings_secondaryattack <mspace=0.6em> $KEY_SecondaryAttack</mspace>", SpiritRules.VanillaGamepadHint, "controller");
        Eq("Next spirit <mspace=0.6em> $KEY_SecondaryAttack</mspace>", SpiritRules.SpiritGamepadHint, "controller, staff out");
    }
}
