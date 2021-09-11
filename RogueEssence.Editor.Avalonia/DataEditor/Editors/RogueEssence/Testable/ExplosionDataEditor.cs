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
        protected override void RunTest(ExplosionData data)
        {
            Character player = DungeonScene.Instance.FocusedCharacter;
            DungeonScene.Instance.PendingDevEvent = data.ReleaseExplosion(player.CharLoc, player, DungeonScene.Instance.MockHitLoc, DungeonScene.Instance.MockHitLoc);
        }
    }
}
