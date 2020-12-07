using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class BattleFXEditor : TestableEditor<BattleFX>
    {
        protected override void RunTest(BattleFX data)
        {
            Character player = DungeonScene.Instance.FocusedCharacter;
            DungeonScene.Instance.PendingDevEvent = DungeonScene.Instance.ProcessBattleFX(player, player, data);
        }
    }
}
