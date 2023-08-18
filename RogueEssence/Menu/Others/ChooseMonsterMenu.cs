using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System;
using System.Linq;

namespace RogueEssence.Menu
{
    public class ChooseMonsterMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 12;

        public SpeakerPortrait Portrait;
        private readonly List<StartChar> choices;
        private readonly OnChooseSlot chooseAction;
        private readonly Action onCancel;
        private readonly bool canMenu;

        public override bool CanMenu => canMenu;
        public override bool CanCancel => onCancel is not null;

        public ChooseMonsterMenu(string title, List<StartChar> choices, int startIndex, OnChooseSlot chooseAction, Action onCancel, bool canMenu = true, int slotsPerPage = SLOTS_PER_PAGE)
        {
            this.chooseAction = chooseAction;
            this.onCancel = onCancel;
            this.canMenu = canMenu;
            this.choices = choices;
            List<MenuChoice> flatChoices = choices.Select((choice, i) =>
            {
                MonsterID monster = choice.ID;
                string name = choice.Name;
                if (string.IsNullOrEmpty(name))
                {
                    EntrySummary summary = DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(monster.Species);
                    name = summary.GetColoredName();
                }
                int index = i;
                return (MenuChoice)new MenuTextChoice(name, () => { this.chooseAction(index); });
            }).ToList();
            IChoosable[][] box = SortIntoPages(flatChoices.ToArray(), slotsPerPage);

            int totalSlots = slotsPerPage;
            if (box.Length == 1)
                totalSlots = box[0].Length;


            startIndex = Math.Clamp(startIndex, 0, flatChoices.Count - 1);

            Portrait = new SpeakerPortrait(MonsterID.Invalid, new EmoteStyle(0), new Loc(200, 32), true);

            Initialize(new Loc(16, 16), 112, title, box, 0, 0, totalSlots, false, -1);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            if (!String.IsNullOrEmpty(Portrait.Speaker.Species))
                Portrait.Draw(spriteBatch, new Loc());
        }

        protected override void ChoiceChanged()
        {
            Portrait.Speaker = choices[CurrentChoiceTotal].ID;
            base.ChoiceChanged();
        }

        protected override void Canceled()
        {
            MenuManager.Instance.RemoveMenu();
            onCancel();
        }
    }
}
