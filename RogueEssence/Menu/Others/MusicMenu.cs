using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using System.IO;

namespace RogueEssence.Menu
{
    public class MusicMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 7;

        public delegate void MusicChoice(string song);
        private MusicChoice choice;
        private List<string> unlocks;
        private List<string> files;
        SongSummary summaryMenu;

        public MusicMenu(List<string> unlockedTags, MusicChoice choice)
        {
            this.unlocks = unlockedTags;
            this.choice = choice;
            string[] pre_files = PathMod.GetModFiles(GraphicsManager.MUSIC_PATH);

            files = new List<string>();
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            flatChoices.Add(new MenuTextChoice("---", () => { choose(""); }));
            foreach (string song in pre_files)
            {
                if (!DataManager.IsNonTrivialFile(song))
                    continue;

                if (!canSeeSong(song))
                    continue;

                files.Add(song);
                flatChoices.Add(new MenuTextChoice(Path.GetFileNameWithoutExtension(song), () => { choose(Path.GetFileName(song)); }));
            }
            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);


            summaryMenu = new SongSummary(Rect.FromPoints(new Loc(8, GraphicsManager.ScreenHeight - 8 - 5 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 8, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(8, 16), 304, Text.FormatKey("MENU_MUSIC_TITLE"), choices, 0, 0, SLOTS_PER_PAGE);
        }

        private void choose(string dir)
        {
            GameManager.Instance.BGM(dir, false);
            choice(dir);
        }

        protected override void ChoiceChanged()
        {
            int totalChoice = CurrentChoiceTotal;
            summaryMenu.SetSong(totalChoice > 0 ? files[totalChoice-1] : "");
            base.ChoiceChanged();
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            summaryMenu.Draw(spriteBatch);
        }

        private bool canSeeSong(string fileName)
        {
            LoopedSong song = new LoopedSong(fileName);
            if (song.Tags.ContainsKey("SPOILER"))
            {
                string spoiler = song.Tags["SPOILER"];
                return unlocks.Contains(spoiler);
            }
            return true;
        }
    }
}
