using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class ShootingEmitterConverter : TestableConverter<ShootingEmitter>
    {
        protected override void btnTest_Click(object sender, RoutedEventArgs e, ShootingEmitter obj)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;

                ShootingEmitter data = (ShootingEmitter)obj.Clone();
                SaveWindowControls(data, (StackPanel)((Button)sender).Parent);
                data.SetupEmit(player.MapLoc, player.CharDir, 4 * GraphicsManager.TileSize + GraphicsManager.TileSize / 2, 10 * GraphicsManager.TileSize);
                DungeonScene.Instance.CreateAnim(data, DrawLayer.NoDraw);
            }
        }
    }
}
