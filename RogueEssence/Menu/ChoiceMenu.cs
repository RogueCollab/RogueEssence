using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class ChoiceMenu : InteractableMenu
    {
        protected const int CURSOR_FLASH_TIME = 24;

        public List<IMenuElement> NonChoices;
        public List<IChoosable> Choices;

        public ulong PrevTick;
        public abstract Loc PickerPos { get; }

        public ChoiceMenu()
        {
            NonChoices = new List<IMenuElement>();
            Choices = new List<IChoosable>();
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            foreach (IChoosable choice in Choices)
                yield return choice;
            foreach (IMenuElement nonChoice in NonChoices)
                yield return nonChoice;
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw cursor
            if (((GraphicsManager.TotalFrameTick - PrevTick) / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0 || Inactive)
                GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(PickerPos.X, PickerPos.Y), 0, 0);

        }
    }
}
