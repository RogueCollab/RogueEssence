using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class InteractableMenu : MenuBase, IInteractable, ILabeled
    {
        const int INPUT_WAIT = 30;
        const int INPUT_GAP = 6;

        public virtual bool IsCheckpoint { get { return false; } }
        public bool Inactive {
            get;
            set;
        }
        public bool BlockPrevious { get; set; }

        public virtual string Label { get; set; } = "";

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

    }
}
