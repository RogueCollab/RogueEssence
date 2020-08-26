using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;

namespace RogueEssence.Dev
{
    public class SkillDataConverter : TestableConverter<SkillData>
    {
        protected override void btnTest_Click(object sender, EventArgs e, SkillData obj)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;

                SkillData data = new SkillData();
                SaveClassControls(data, (TableLayoutPanel)((Button)sender).Parent);

                DungeonScene.Instance.PendingDevEvent = player.MockCharAction(data);
            }
        }
    }
}
