using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System.Collections.Generic;

namespace RogueEssence.Menu
{
    public class ModDiffDialog : QuestionDialog
    {
        ModDiffSummary summaryMenu;

        public ModDiffDialog(string message, List<ModDiff> diff, bool sound, DialogueChoice[] choices, int defaultChoice, int cancelChoice)
            : base(message, sound, false, false, choices, defaultChoice, cancelChoice)
        {
            summaryMenu = new ModDiffSummary();
            summaryMenu.SetDiff(diff);
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
