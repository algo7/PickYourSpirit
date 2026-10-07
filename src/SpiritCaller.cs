using System.Collections.Generic;
using UnityEngine;

namespace PickYourSpirit
{
    /// <summary>The Spirit Caller in game: recognising it, its spirit list and this session's pick. Main thread only.</summary>
    internal static class SpiritCaller
    {
        /// <summary>The staff's item name (m_shared.m_name): on every copy of the item, however it was made.</summary>
        public const string ItemName = "$item_staff_spiritcaller";

        /// <summary>This session's pick: a spirit prefab name, or null for Random. Plugin resets it on every scene load.</summary>
        public static string Pick;

        private static bool s_warnedNoList;

        public static bool IsSpiritCaller(ItemDrop.ItemData item) =>
            item != null && item.m_shared != null && item.m_shared.m_name == ItemName;

        /// <summary>The spirits the staff's cast can summon (its SpawnAbility's list), or null if it has none.</summary>
        public static GameObject[] SpawnList(ItemDrop.ItemData staff)
        {
            var projectile = staff?.m_shared?.m_attack?.m_attackProjectile;
            if (projectile == null) return null;
            var ability = projectile.GetComponent<SpawnAbility>();
            return ability != null ? ability.m_spawnPrefab : null;
        }

        /// <summary>The prefabs' names, aligned with the list (null for a missing prefab).</summary>
        public static List<string> Names(GameObject[] list)
        {
            var names = new List<string>();
            if (list == null) return names;
            foreach (var prefab in list) names.Add(prefab != null ? prefab.name : null);
            return names;
        }

        /// <summary>Middle mouse with the staff in hand: move to the next pick and show it.</summary>
        public static void Press(ItemDrop.ItemData staff)
        {
            var list = SpawnList(staff);
            var choices = SpiritRules.Choices(Names(list));
            if (choices.Count == 0)
            {
                if (s_warnedNoList) return;
                s_warnedNoList = true;
                Plugin.Log.LogWarning("The Spirit Caller has no spirit list (a game update?): its spirits stay random");
                return;
            }
            Pick = SpiritRules.Next(Pick, choices);
            if (MessageHud.instance == null) return;
            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, SpiritRules.Message(ShownName(list, Pick)), log: false);
        }

        /// <summary>The picked spirit's name as the game has it (a $token the message localizes), or its prefab name.</summary>
        private static string ShownName(GameObject[] list, string pick)
        {
            if (pick == null) return null;
            foreach (var prefab in list)
            {
                if (prefab == null || prefab.name != pick) continue;
                var character = prefab.GetComponent<Character>();
                return character != null && !string.IsNullOrEmpty(character.m_name) ? character.m_name : pick;
            }
            return pick;
        }
    }
}
