using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class CircleSquareEmitterEditor : TestableEditor<CircleSquareEmitter>
    {
        protected override void RunTest(CircleSquareEmitter data)
        {
            Character player = DungeonScene.Instance.FocusedCharacter;
            data.SetupEmit(player.MapLoc, player.CharDir, Hitbox.AreaLimit.Full, 2 * GraphicsManager.TileSize + GraphicsManager.TileSize / 2, 10 * GraphicsManager.TileSize);
            DungeonScene.Instance.CreateAnim(data, DrawLayer.NoDraw);
        }
    }
}
