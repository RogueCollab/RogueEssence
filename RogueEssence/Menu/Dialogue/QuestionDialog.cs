using System;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using System.Collections.Generic;

namespace RogueEssence.Menu
{
    public class QuestionDialog : DialogueBox
    {
        private DialogueChoiceMenu dialogueChoices;

        public QuestionDialog(string message, bool sound, DialogueChoice[] choices, int defaultChoice,
            int cancelChoice)
            : base(message, sound)
        {
            dialogueChoices = new DialogueChoiceMenu(choices, defaultChoice, cancelChoice, Bounds.Y);
        }

        public override void ProcessTextDone(InputManager input)
        {
            //choice menu needs a special setting for always making the cursor flash
            //make the singlestripmenu store callbacks for when a choice gets selected?
            //then, its initialize method must be public
            //or, just make a special menu window for dialogue questions.  DialogueChoiceMenu?
            dialogueChoices.Update(input);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            if (Text.Finished)
                dialogueChoices.Draw(spriteBatch);

        }
    }

    public class DialogueChoiceMenu : VertChoiceMenu
    {
        private Action[] results;
        private int cancelChoice;

        public DialogueChoiceMenu(DialogueChoice[] choices, int defaultChoice, int cancelChoice, int startY)
        {
            MenuTextChoice[] menu_choices = new MenuTextChoice[choices.Length];
            results = new Action[choices.Length];
            for (int ii = 0; ii < choices.Length; ii++)
            {
                int index = ii;
                menu_choices[ii] = new MenuTextChoice(choices[ii].Choice, () => { choose(index); }, choices[ii].Enabled, choices[ii].Enabled ? Color.White : Color.Red);
                results[ii] = choices[ii].Result;
            }
            int question_width = CalculateChoiceLength(menu_choices, 0);
            Initialize(new Loc(GraphicsManager.ScreenWidth - DialogueBox.SIDE_BUFFER - question_width, startY - (choices.Length * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2) - QUESTION_SPACE),
                question_width, menu_choices, defaultChoice);

            this.cancelChoice = cancelChoice;
        }

        private void choose(int choice)
        {
            MenuManager.Instance.RemoveMenu();

            if (results[choice] != null)
                results[choice]();
        }

        protected override void MenuPressed() { }
        protected override void ChoseMultiIndex(List<int> slots) { }

        protected override void Canceled()
        {
            if (cancelChoice > -1)
                choose(cancelChoice);
        }
    }


    public class DialogueChoice
    {
        public string Choice;
        public Action Result;
        public bool Enabled;

        public DialogueChoice(string choice, Action result)
        : this(choice, result, true) { }
        public DialogueChoice(string choice, Action result, bool enable)
        {
            Choice = choice;
            Result = result;
            Enabled = enable;
        }
    }
}
