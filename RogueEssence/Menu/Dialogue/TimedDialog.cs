using System;

namespace RogueEssence.Menu
{
    public class TimedDialog : DialogueBox
    {
        private int time;
        private Action action;

        public TimedDialog(string message, bool sound, bool centered, int time, Action action)
            : base(message, sound, centered)
        {
            this.time = time;
            this.action = action;
        }

        public override void ProcessTextDone(InputManager input)
        {
            if (CurrentTextTime >= time)
            {
                //close this
                MenuManager.Instance.RemoveMenu();

                //do what it wants
                action();
            }
        }
    }
}
