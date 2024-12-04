using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using System.IO;
using System;

namespace RogueEssence.Menu
{
    public class MusicMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 7;

        public delegate void MusicChoice(string song);
        private MusicChoice choice;
        private List<string> spoiledSongs;
        private List<(string file, LoopedSong song)> songs;
        private SongSummary summaryMenu;

        public MusicMenu(bool hardMod, List<string> spoiledSongs, MusicChoice choice)
        {
            this.spoiledSongs = spoiledSongs;
            this.choice = choice;
            string[] pre_files;
            if (hardMod)
                pre_files = PathMod.GetHardModFiles(GraphicsManager.MUSIC_PATH);
            else
                pre_files = PathMod.GetModFiles(GraphicsManager.MUSIC_PATH);

            //file list will be all songs
            //tag list will be all their tags
            //go through all files and get their tags

            songs = new List<(string, LoopedSong)>();
            foreach (string fileName in pre_files)
            {
                if (!DataManager.IsNonTrivialFile(fileName))
                    continue;

                try
                {
                    LoopedSong song = new LoopedSong(fileName);
                    songs.Add((fileName, song));
                }
                catch (Exception ex)
                {
                    //skip any that don't load tags
                }

            }

            //sort them based on DISCNUMBER and TRACK
            songs.Sort(DiscTrackSort);

            //add to menu with blank first item
            songs.Insert(0, ("", null));


            //add menu text without extension, or ??? if cannot see song, with choose being ""
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            foreach ((string file, LoopedSong song) song in songs)
            {
                if (song.file == "")
                    flatChoices.Add(new MenuTextChoice("---", () => { choose(""); }));
                else if (!canSeeSong(song.file))
                    flatChoices.Add(new MenuTextChoice("???", () => { choose(""); }));
                else
                    flatChoices.Add(new MenuTextChoice(Path.GetFileNameWithoutExtension(song.file), () => { choose(Path.GetFileName(song.file)); }));
            }
            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);


            summaryMenu = new SongSummary(Rect.FromPoints(new Loc(8, GraphicsManager.ScreenHeight - 8 - 5 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 8, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(8, 16), 304, Text.FormatKey("MENU_MUSIC_TITLE"), choices, 0, 0, SLOTS_PER_PAGE);
        }

        private int DiscTrackSort((string file, LoopedSong song) item1, (string file, LoopedSong song) item2)
        {
            int disc1 = int.MaxValue;
            int disc2 = int.MaxValue;
            if (item1.song.Tags.ContainsKey("DISCNUMBER"))
                disc1 = int.Parse(item1.song.Tags["DISCNUMBER"][0]);
            if (item2.song.Tags.ContainsKey("DISCNUMBER"))
                disc2 = int.Parse(item2.song.Tags["DISCNUMBER"][0]);

            int cmp = Math.Sign(disc1 - disc2);
            if (cmp != 0)
                return cmp;

            int track1 = Int32.MaxValue;
            int track2 = Int32.MaxValue;
            if (item1.song.Tags.ContainsKey("TRACKNUMBER"))
                track1 = int.Parse(item1.song.Tags["TRACKNUMBER"][0]);
            if (item2.song.Tags.ContainsKey("TRACKNUMBER"))
                track2 = int.Parse(item2.song.Tags["TRACKNUMBER"][0]);

            cmp = Math.Sign(track1 - track2);
            if (cmp != 0)
                return cmp;
            int stringcmp = String.Compare(item1.song.Name, item2.song.Name);
            return stringcmp;
        }

        private void choose(string dir)
        {
            GameManager.Instance.BGM(dir, false);
            choice(dir);
        }

        protected override void ChoiceChanged()
        {
            //SongSummary will be passed the tags
            int totalChoice = CurrentChoiceTotal;
            if (!canSeeSong(songs[totalChoice].file))
                summaryMenu.SetSong(null);
            else
                summaryMenu.SetSong(songs[totalChoice].song);
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

        private bool canSeeSong(string songfile)
        {
            string songName = Path.GetFileName(songfile);
            return !spoiledSongs.Contains(songName);
        }
    }
}
