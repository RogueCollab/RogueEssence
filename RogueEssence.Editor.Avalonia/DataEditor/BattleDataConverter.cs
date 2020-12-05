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
    public class BattleDataConverter : TestableConverter<BattleData>
    {
        protected override void btnTest_Click(object sender, RoutedEventArgs e, BattleData obj)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;

                BattleData data = new BattleData();
                SaveClassControls(data, (StackPanel)((Button)sender).Parent);

                DungeonScene.Instance.PendingDevEvent = DungeonScene.Instance.ProcessEndAnim(player, player, data);
            }
        }
    }
}
