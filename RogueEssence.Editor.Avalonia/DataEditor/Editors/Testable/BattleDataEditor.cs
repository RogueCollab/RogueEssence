using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RogueEssence.Dev
{
    public class BattleDataEditor : TestableEditor<BattleData>
    {
        protected override void RunTest(BattleData data)
        {
            Character player = DungeonScene.Instance.FocusedCharacter;
            DungeonScene.Instance.PendingDevEvent = DungeonScene.Instance.ProcessEndAnim(player, player, data);
        }
    }
}
