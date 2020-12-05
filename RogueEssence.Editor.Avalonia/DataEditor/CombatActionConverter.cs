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
    public class CombatActionConverter : TestableConverter<CombatAction>
    {
        protected override void btnTest_Click(object sender, RoutedEventArgs e, CombatAction obj)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;

                CombatAction data = obj.Clone();
                SaveClassControls(data, (StackPanel)((Button)sender).Parent);

                DungeonScene.Instance.PendingDevEvent = player.MockCharAction(data, DungeonScene.Instance.MockHitLoc, DungeonScene.Instance.MockHitLoc);
            }
        }
    }
}
