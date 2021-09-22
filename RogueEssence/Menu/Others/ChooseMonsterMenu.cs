using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System;

namespace RogueEssence.Menu
{
    public class ChooseMonsterMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 12;

        public SpeakerPortrait Portrait;
        private readonly OnChooseSlot chooseAction;
        private readonly Action onCancel;
        private readonly bool canMenu;

        public override bool CanMenu => canMenu;
        public override bool CanCancel => onCancel is not null;

        public ChooseMonsterMenu(string title, int startIndex, OnChooseSlot chooseAction, Action onCancel, bool canMenu = true)
        {
            this.chooseAction = chooseAction;
            this.onCancel = onCancel;
            this.canMenu = canMenu;
            List<MenuChoice> flatChoices = new();
            for (int ii = 0; ii < DataManager.Instance.StartChars.Count; ii++)
            {
                MonsterID startChar = DataManager.Instance.StartChars[ii].mon;
                string name = DataManager.Instance.GetMonster(startChar.Species).GetColoredName();
                if (DataManager.Instance.StartChars[ii].name != "")
                    name = DataManager.Instance.StartChars[ii].name;
                int index = ii;
                flatChoices.Add(new MenuTextChoice(name, () => { this.chooseAction(index); }));
            }
            List<MenuChoice[]> box = SortIntoPages(flatChoices, SLOTS_PER_PAGE);

            int totalSlots = SLOTS_PER_PAGE;
            if (box.Count == 1)
                totalSlots = box[0].Length;


            startIndex = Math.Clamp(startIndex, 0, flatChoices.Count - 1);

            Portrait = new SpeakerPortrait(new MonsterID(), new EmoteStyle(0), new Loc(200, 32), true);

            Initialize(new Loc(16, 16), 112, title, box.ToArray(), 0, 0, totalSlots, false, -1);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            if (Portrait.Speaker.Species > 0)
                Portrait.Draw(spriteBatch, new Loc());
        }

        protected override void ChoiceChanged()
        {
            Portrait.Speaker = DataManager.Instance.StartChars[CurrentChoiceTotal].mon;
            base.ChoiceChanged();
        }

        protected override void Canceled()
        {
            MenuManager.Instance.RemoveMenu();
            onCancel();
        }
    }
}
