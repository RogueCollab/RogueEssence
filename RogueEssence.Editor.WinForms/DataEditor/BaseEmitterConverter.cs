using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class BaseEmitterConverter : TestableConverter<EndingEmitter>
    {
        protected override void btnTest_Click(object sender, EventArgs e, EndingEmitter obj)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;

                EndingEmitter data = (EndingEmitter)obj.Clone();
                SaveClassControls(data, (TableLayoutPanel)((Button)sender).Parent);
                data.SetupEmit(player.MapLoc, player.MapLoc, player.CharDir);
                DungeonScene.Instance.CreateAnim(data, DrawLayer.NoDraw);
            }
        }
    }
}
