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
        private readonly List<(MonsterID mon, string name)> choices;
        private readonly OnChooseSlot chooseAction;
        private readonly Action onCancel;
        private readonly bool canMenu;

        public override bool CanMenu => canMenu;
        public override bool CanCancel => onCancel is not null;

        public ChooseMonsterMenu(string title, List<(MonsterID mon, string name)> choices, int startIndex, OnChooseSlot chooseAction, Action onCancel, bool canMenu = true)
        {
            this.chooseAction = chooseAction;
            this.onCancel = onCancel;
            this.canMenu = canMenu;
            this.choices = choices;
            List<MenuChoice> flatChoices = choices.Select((choice, i) =>
            {
                MonsterID monster = choice.mon;
                string name = choice.name;
                if (string.IsNullOrEmpty(name))
                    name = DataManager.Instance.GetMonster(monster.Species).GetColoredName();
                int index = i;
                return (MenuChoice)new MenuTextChoice(name, () => { this.chooseAction(index); });
            }).ToList();
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
            Portrait.Speaker = choices[CurrentChoiceTotal].mon;
            base.ChoiceChanged();
        }

        protected override void Canceled()
        {
            MenuManager.Instance.RemoveMenu();
            onCancel();
        }
    }
}
