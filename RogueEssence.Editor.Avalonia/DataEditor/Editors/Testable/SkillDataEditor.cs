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
    public class SkillDataEditor : TestableEditor<SkillData>
    {
        protected override void RunTest(SkillData data)
        {
            Character player = DungeonScene.Instance.FocusedCharacter;
            DungeonScene.Instance.PendingDevEvent = player.MockCharAction(data);
        }
    }
}
