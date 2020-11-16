using ReactiveUI;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class GroundTabPropertiesViewModel : ViewModelBase
    {
        public GroundTabPropertiesViewModel()
        {
            ScrollEdges = new ObservableCollection<string>();
            for (int ii = 0; ii <= (int)Map.ScrollEdge.Clamp; ii++)
                ScrollEdges.Add(((Map.ScrollEdge)ii).ToLocal());

            Music = new ObservableCollection<string>();
            reloadMusic();
        }


        public string MapName
        {
            get
            {
                lock (GameBase.lockObj)
                {
                    return ZoneManager.Instance.CurrentGround.Name.DefaultText;
                }
            }
            set
            {
                lock (GameBase.lockObj)
                {
                    this.RaiseAndSetIfChanged(ref ZoneManager.Instance.CurrentGround.Name.DefaultText, value);
                }
            }
        }

        public ObservableCollection<string> ScrollEdges { get; }

        public int ChosenScroll
        {
            get
            {
                lock (GameBase.lockObj)
                {
                    return (int)ZoneManager.Instance.CurrentGround.EdgeView;
                }
            }
            set
            {
                lock (GameBase.lockObj)
                {
                    int scroll = (int)ZoneManager.Instance.CurrentGround.EdgeView;
                    this.RaiseAndSetIfChanged(ref scroll, value);
                    ZoneManager.Instance.CurrentGround.EdgeView = (Map.ScrollEdge)scroll;
                }
            }
        }


        public ObservableCollection<string> Music { get; }

        private int chosenMusic;
        public int ChosenMusic
        {
            get { return chosenMusic; }
            set
            {
                this.RaiseAndSetIfChanged(ref chosenMusic, value);
                musicChanged();
            }
        }

        public void btnReloadMusic_Click()
        {
            reloadMusic();
        }

        private void reloadMusic()
        {
            lock (GameBase.lockObj)
            {
                Music.Clear();

                string[] files = Directory.GetFiles(DataManager.MUSIC_PATH, "*.ogg", SearchOption.TopDirectoryOnly);

                Music.Add("None");
                for (int ii = 0; ii < files.Length; ii++)
                {
                    string song = files[ii].Substring((DataManager.MUSIC_PATH).Length);
                    Music.Add(song);
                    if (song == ZoneManager.Instance.CurrentGround.Music)
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
                    ZoneManager.Instance.CurrentGround.Music = "";
                }
                else
                {
                    string fileName = (string)Music[chosenMusic];
                    ZoneManager.Instance.CurrentGround.Music = fileName;
                }

                GameManager.Instance.BGM(ZoneManager.Instance.CurrentGround.Music, false);
            }
        }
    }
}
