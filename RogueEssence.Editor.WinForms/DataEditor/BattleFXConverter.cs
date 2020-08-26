using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class BattleFXConverter : TestableConverter<BattleFX>
    {
        protected override void btnTest_Click(object sender, EventArgs e, BattleFX obj)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;

                BattleFX data = new BattleFX();
                SaveClassControls(data, (TableLayoutPanel)((Button)sender).Parent);

                DungeonScene.Instance.PendingDevEvent = DungeonScene.Instance.ProcessBattleFX(player, player, data);
            }
        }
    }
}
