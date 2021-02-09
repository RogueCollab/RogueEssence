using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class BaseEmitterEditor : TestableEditor<EndingEmitter>
    {
        protected override void RunTest(EndingEmitter data)
        {
            Character player = DungeonScene.Instance.FocusedCharacter;
            data.SetupEmit(player.MapLoc, player.MapLoc, player.CharDir);
            DungeonScene.Instance.CreateAnim(data, DrawLayer.NoDraw);
        }
    }
}
