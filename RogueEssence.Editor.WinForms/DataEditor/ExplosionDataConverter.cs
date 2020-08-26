using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;

namespace RogueEssence.Dev
{
    public class ExplosionDataConverter : TestableConverter<ExplosionData>
    {
        protected override void btnTest_Click(object sender, EventArgs e, ExplosionData obj)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;

                ExplosionData data = new ExplosionData();
                SaveClassControls(data, (TableLayoutPanel)((Button)sender).Parent);

                DungeonScene.Instance.PendingDevEvent = data.ReleaseExplosion(player.CharLoc, player, DungeonScene.Instance.MockHitLoc, DungeonScene.Instance.MockHitLoc);
            }
        }
    }
}
