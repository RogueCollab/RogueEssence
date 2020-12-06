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
    public class SkillDataConverter : TestableConverter<SkillData>
    {
        protected override void btnTest_Click(object sender, RoutedEventArgs e, SkillData obj)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;

                SkillData data = new SkillData();
                SaveWindowControls(data, (StackPanel)((Button)sender).Parent);

                DungeonScene.Instance.PendingDevEvent = player.MockCharAction(data);
            }
        }
    }
}
