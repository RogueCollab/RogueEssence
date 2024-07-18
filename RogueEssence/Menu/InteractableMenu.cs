using Microsoft.Xna.Framework.Graphics;
using RogueElements;
using System.Collections.Generic;

namespace RogueEssence.Menu
{
    public abstract class InteractableMenu : MenuBase, IInteractable
    {
        const int INPUT_WAIT = 30;
        const int INPUT_GAP = 6;

        public virtual bool IsCheckpoint { get { return false; } }
        public bool Inactive { get; set; }
        public bool BlockPrevious { get; set; }

        public List<SummaryMenu> SummaryMenus { get; set; }

        public InteractableMenu()
        {
            SummaryMenus = new List<SummaryMenu>();
        }

        public abstract void Update(InputManager input);

        public void ProcessActions(FrameTick elapsedTime) { }

        public static bool IsInputting(InputManager input, params Dir8[] dirs)
        {
            bool choseDir = false;
            bool prevDir = false;
            foreach (Dir8 allowedDir in dirs)
            {
                if (input.Direction == allowedDir)
                    choseDir = true;
                if (input.PrevDirection == allowedDir)
                    prevDir = true;
            }

            bool atAdd = false;
            if (input.InputTime >= INPUT_WAIT)
            {
                if ((input.InputTime - input.AddedInputTime) / INPUT_GAP < input.InputTime / INPUT_GAP)
                    atAdd = true;
            }
            return (choseDir && (!prevDir || atAdd));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            foreach (SummaryMenu menu in SummaryMenus)
                menu.Draw(spriteBatch);
        }

        public int GetSummaryIndexByLabel(string label)
        {
            return GetSummaryIndicesByLabel(label)[label];
        }
        public virtual Dictionary<string, int> GetSummaryIndicesByLabel(params string[] labels)
        {
            return SearchLabels(labels, SummaryMenus);
        }
    }
}
