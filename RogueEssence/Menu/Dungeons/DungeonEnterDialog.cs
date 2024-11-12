using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class DungeonEnterDialog : QuestionDialog
    {
        DungeonSummary summaryMenu;

        public DungeonEnterDialog(string message, string zoneName, ZoneLoc dest, bool sound, DialogueChoice[] choices, int defaultChoice, int cancelChoice)
            : base(message, sound, false, false, choices, defaultChoice, cancelChoice)
        {
            summaryMenu = new DungeonSummary(new Rect(new Loc(8, 8), new Loc(144, GraphicsManager.MenuBG.TileHeight * 2 + VERT_SPACE * 7)));
            summaryMenu.SetDungeon(zoneName, dest.ID, DataManager.Instance.Save.GetDungeonUnlock(dest.ID) == GameProgress.UnlockState.Completed, true, false);
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw the summary
            summaryMenu.Draw(spriteBatch);
        }
    }
    
}
