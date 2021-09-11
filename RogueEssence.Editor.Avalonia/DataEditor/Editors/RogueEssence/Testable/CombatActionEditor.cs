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
    public class CombatActionEditor : TestableEditor<CombatAction>
    {
        protected override void RunTest(CombatAction data)
        {
            Character player = DungeonScene.Instance.FocusedCharacter;
            DungeonScene.Instance.PendingDevEvent = player.MockCharAction(data, DungeonScene.Instance.MockHitLoc, DungeonScene.Instance.MockHitLoc);
        }
    }
}
