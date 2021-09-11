using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class StaticAnimEditor : TestableEditor<StaticAnim>
    {
        protected override void RunTest(StaticAnim data)
        {
            Character player = DungeonScene.Instance.FocusedCharacter;
            StaticAnim emitter = (StaticAnim)data.CloneIEmittable();
            emitter.SetupEmitted(player.MapLoc, 0, player.CharDir);
            DungeonScene.Instance.CreateAnim(emitter, DrawLayer.Normal);
        }
    }
}
