using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class ShootingEmitterEditor : TestableEditor<ShootingEmitter>
    {
        protected override void RunTest(ShootingEmitter data)
        {
            Character player = DungeonScene.Instance.FocusedCharacter;
            ShootingEmitter emitter = (ShootingEmitter)data.Clone();
            emitter.SetupEmit(player.MapLoc, player.CharDir, 4 * GraphicsManager.TileSize + GraphicsManager.TileSize / 2, 10 * GraphicsManager.TileSize);
            DungeonScene.Instance.CreateAnim(emitter, DrawLayer.NoDraw);
        }
    }
}
