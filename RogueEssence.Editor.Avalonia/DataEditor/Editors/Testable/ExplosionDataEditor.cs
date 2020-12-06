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
    public class ExplosionDataEditor : TestableEditor<ExplosionData>
    {
        protected override void btnTest_Click(object sender, RoutedEventArgs e, ExplosionData obj)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;

                ExplosionData data = new ExplosionData();
                SaveWindowControls(data, (StackPanel)((Button)sender).Parent);

                DungeonScene.Instance.PendingDevEvent = data.ReleaseExplosion(player.CharLoc, player, DungeonScene.Instance.MockHitLoc, DungeonScene.Instance.MockHitLoc);
            }
        }
    }
}
