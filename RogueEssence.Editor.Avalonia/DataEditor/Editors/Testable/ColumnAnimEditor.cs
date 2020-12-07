using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class ColumnAnimEditor : TestableEditor<ColumnAnim>
    {
        protected override void RunTest(ColumnAnim data)
        {
            Character player = DungeonScene.Instance.FocusedCharacter;
            data.SetupEmitted(player.MapLoc, 0, player.CharDir);
            DungeonScene.Instance.CreateAnim(data, DrawLayer.Normal);
        }
    }
}
