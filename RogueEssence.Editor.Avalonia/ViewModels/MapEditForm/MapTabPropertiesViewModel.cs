using Avalonia.Controls;
using ReactiveUI;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class MapTabPropertiesViewModel : ViewModelBase
    {
        public MapTabPropertiesViewModel()
        {
            ScrollEdges = new ObservableCollection<string>();
            for (int ii = 0; ii <= (int)Map.ScrollEdge.Clamp; ii++)
                ScrollEdges.Add(((Map.ScrollEdge)ii).ToLocal());
            BGs = new ObservableCollection<string>();
            BGs.Add("---");
            string[] dirs = PathMod.GetModFiles(GraphicsManager.CONTENT_PATH + "BG/");
            for (int ii = 0; ii < dirs.Length; ii++)
            {
                string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                BGs.Add(filename);
            }

            Music = new ObservableCollection<string>();
            reloadMusic();

        }



        public string MapName
        {
            get
            {
                return ZoneManager.Instance.CurrentMap.Name.DefaultText;
            }
            set
            {
                lock (GameBase.lockObj)
                    this.RaiseAndSet(ref ZoneManager.Instance.CurrentMap.Name.DefaultText, value);
            }
        }

        public ObservableCollection<string> ScrollEdges { get; }

        public int ChosenScroll
        {
            get
            {
                return (int)ZoneManager.Instance.CurrentMap.EdgeView;
            }
            set
            {
                lock (GameBase.lockObj)
                {
                    ZoneManager.Instance.CurrentMap.EdgeView = (Map.ScrollEdge)value;
                    this.RaisePropertyChanged();
                }
            }
        }


        public ObservableCollection<string> BGs { get; }

        public int ChosenBG
        {
            get
            {
                int chosenAnim = BGs.IndexOf(ZoneManager.Instance.CurrentMap.BGAnim.AnimIndex);
                if (chosenAnim == -1)
                    chosenAnim = 0;
                return chosenAnim;
            }
            set
            {
                ZoneManager.Instance.CurrentMap.BGAnim.AnimIndex = value > 0 ? BGs[value] : "";
                this.RaisePropertyChanged();
            }
        }

        public int BGFrameTime
        {
            get
            {
                return ZoneManager.Instance.CurrentMap.BGAnim.FrameTime;
            }
            set
            {
                ZoneManager.Instance.CurrentMap.BGAnim.FrameTime = value;
                this.RaisePropertyChanged();
            }
        }

        public int BGMoveX
        {
            get
            {
                return ZoneManager.Instance.CurrentMap.BGMovement.X;
            }
            set
            {
                ZoneManager.Instance.CurrentMap.BGMovement.X = value;
                this.RaisePropertyChanged();
            }
        }

        public int BGMoveY
        {
            get
            {
                return ZoneManager.Instance.CurrentMap.BGMovement.Y;
            }
            set
            {
                ZoneManager.Instance.CurrentMap.BGMovement.Y = value;
                this.RaisePropertyChanged();
            }
        }


        public ObservableCollection<string> Music { get; }

        private int chosenMusic;
        public int ChosenMusic
        {
            get { return chosenMusic; }
            set
            {
                this.SetIfChanged(ref chosenMusic, value);
                musicChanged();
            }
        }

        public void btnReloadMusic_Click()
        {
            reloadMusic();
        }

        private void reloadMusic()
        {
            if (Design.IsDesignMode)
                return;
            lock (GameBase.lockObj)
            {
                Music.Clear();

                string[] files = PathMod.GetModFiles(GraphicsManager.MUSIC_PATH, "*.ogg");

                Music.Add("None");
                for (int ii = 0; ii < files.Length; ii++)
                {
                    string song = Path.GetFileName(files[ii]);
                    Music.Add(song);
                    if (song == ZoneManager.Instance.CurrentMap.Music)
                        ChosenMusic = ii;
                }
            }
        }

        private void musicChanged()
        {
            lock (GameBase.lockObj)
            {
                if (chosenMusic <= 0)
                {
                    ZoneManager.Instance.CurrentMap.Music = "";
                }
                else
                {
                    string fileName = (string)Music[chosenMusic];
                    ZoneManager.Instance.CurrentMap.Music = fileName;
                }

                GameManager.Instance.BGM(ZoneManager.Instance.CurrentMap.Music, false);
            }
        }

        public void LoadMapProperties()
        {
            MapName = MapName;
            ChosenScroll = ChosenScroll;
            ChosenBG = ChosenBG;
            BGFrameTime = BGFrameTime;
            BGMoveX = BGMoveX;
            BGMoveY = BGMoveY;


            bool foundSong = false;
            for (int ii = 0; ii < Music.Count; ii++)
            {
                string song = Music[ii];
                if (song == ZoneManager.Instance.CurrentMap.Music)
                {
                    ChosenMusic = ii;
                    foundSong = true;
                    break;
                }
            }
            if (!foundSong)
                ChosenMusic = -1;
        }


    }
}
