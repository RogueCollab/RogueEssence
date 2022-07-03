using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class DexMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 14;

        SummaryMenu summaryMenu;
        SpeakerPortrait portrait;
        
        public DexMenu()
        {
            int lastEntry = -1;
            int seen = 0;
            int befriended = 0;
            for (int ii = 1; ii < DataManager.Instance.DataIndices[DataManager.DataType.Monster].Count; ii++)
            {
                if (DataManager.Instance.Save.GetMonsterUnlock(ii) > GameProgress.UnlockState.None)
                {
                    lastEntry = ii;
                    seen++;
                    if (DataManager.Instance.Save.GetMonsterUnlock(ii) == GameProgress.UnlockState.Completed)
                        befriended++;
                }
            }

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 1; ii <= lastEntry; ii++)
            {
                if (DataManager.Instance.Save.GetMonsterUnlock(ii) > GameProgress.UnlockState.None)
                {
                    Color color = (DataManager.Instance.Save.GetMonsterUnlock(ii) == GameProgress.UnlockState.Completed) ? Color.White : Color.Gray;
                    
                    //name
                    MenuText dexNum = new MenuText(ii.ToString("D3"), new Loc(2, 1), color);
                    //TODO: String Assets
                    MenuText dexName = new MenuText(DataManager.Instance.DataIndices[DataManager.DataType.Monster].Entries[ii.ToString()].Name.ToLocal(), new Loc(24, 1), color);
                    flatChoices.Add(new MenuElementChoice(() => { choose(ii); }, true, dexNum, dexName));
                }
                else
                {
                    //???
                    MenuText dexNum = new MenuText(ii.ToString("D3"), new Loc(2, 1), Color.Gray);
                    MenuText dexName = new MenuText("???", new Loc(24, 1), Color.Gray);
                    flatChoices.Add(new MenuElementChoice(() => { choose(ii); }, true, dexNum, dexName));
                }
            }
            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            summaryMenu = new SummaryMenu(new Rect(new Loc(208, 16), new Loc(96, LINE_HEIGHT * 2 + GraphicsManager.MenuBG.TileHeight * 2)));
            MenuText seenText = new MenuText(Text.FormatKey("MENU_DEX_SEEN", seen), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            summaryMenu.Elements.Add(seenText);
            MenuText befriendedText = new MenuText(Text.FormatKey("MENU_DEX_CAUGHT", befriended), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT));
            summaryMenu.Elements.Add(befriendedText);

            portrait = new SpeakerPortrait(new MonsterID(), new EmoteStyle(0), new Loc(232, 72), true);

            Initialize(new Loc(0, 0), 208, Text.FormatKey("MENU_DEX_TITLE"), choices, 0, 0, SLOTS_PER_PAGE);
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);
            
            summaryMenu.Draw(spriteBatch);

            if (portrait.Speaker.Species > 0)
                portrait.Draw(spriteBatch, new Loc());

        }

        protected override void ChoiceChanged()
        {
            int totalChoice = CurrentChoiceTotal + 1;
            if (DataManager.Instance.Save.GetMonsterUnlock(totalChoice) > GameProgress.UnlockState.None)
                portrait.Speaker = new MonsterID(totalChoice, 0, -1, Gender.Unknown);
            else
                portrait.Speaker = new MonsterID();
            base.ChoiceChanged();
        }

        private void choose(int species)
        {
            
        }

    }
}
