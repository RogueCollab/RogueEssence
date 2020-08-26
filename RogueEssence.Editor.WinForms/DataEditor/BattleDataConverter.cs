using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;

namespace RogueEssence.Dev
{
    public class BattleDataConverter : TestableConverter<BattleData>
    {
        protected override void btnTest_Click(object sender, EventArgs e, BattleData obj)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;

                BattleData data = new BattleData();
                SaveClassControls(data, (TableLayoutPanel)((Button)sender).Parent);

                DungeonScene.Instance.PendingDevEvent = DungeonScene.Instance.ProcessEndAnim(player, player, data);
            }
        }
    }
}
