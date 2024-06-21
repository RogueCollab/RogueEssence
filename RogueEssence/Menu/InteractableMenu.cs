using Microsoft.Xna.Framework.Graphics;
using RogueElements;
using System.Collections.Generic;
using System.Linq;

namespace RogueEssence.Menu
{
    public abstract class InteractableMenu : MenuBase, IInteractable
    {
        const int INPUT_WAIT = 30;
        const int INPUT_GAP = 6;

        public virtual bool IsCheckpoint { get { return false; } }
        public bool Inactive {
            get;
            set;
        }
        public bool BlockPrevious { get; set; }

        public List<SummaryMenu> SummaryMenus { get; set; } = new();

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
        public virtual int GetSummaryIndexByLabel(string label)
        {
            if (GetSummaryIndexesByLabel(label).TryGetValue(label, out int ret)) return ret;
            return -1;
        }
        public virtual Dictionary<string, int> GetSummaryIndexesByLabel(params string[] labels)
        {
            Dictionary<string, int> poss = new();
            List<string> labelList = labels.ToList();
            foreach (string label in labels)
                poss.Add(label, -1);

            for (int ii = 0; ii < SummaryMenus.Count; ii++)
            {
                bool found = false;
                ILabeled summary = SummaryMenus[ii];
                if (summary.HasLabel())
                {
                    for (int kk = 0; kk < labelList.Count; kk++)
                    {
                        string label = labelList[kk];
                        if (summary.Label == label)
                        {
                            found = true;
                            poss[label] = ii;
                            labelList.RemoveAt(kk);
                            break;
                        }
                    }
                }
                if (found && labelList.Count == 0) break;
            }
            return poss;
        }
    }
}
