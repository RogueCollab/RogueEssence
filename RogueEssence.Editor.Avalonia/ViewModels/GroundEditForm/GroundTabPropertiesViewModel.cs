using Avalonia.Controls;
using ReactiveUI;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.Views;
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
            for (int ii = 0; ii <= (int)Map.ScrollEdge.Wrap; ii++)
                ScrollEdges.Add(((Map.ScrollEdge)ii).ToLocal());

            BG = new ClassBoxViewModel(new StringConv(typeof(IBackgroundSprite), new object[0]));
            BG.OnMemberChanged += BG_Changed;
            BG.OnEditItem += MapBG_Edit;
            BlankBG = new TileBoxViewModel();
            BlankBG.OnMemberChanged += BlankBG_Changed;
            BlankBG.OnEditItem += AutoTile_Edit;

            Music = new ObservableCollection<string>();
            reloadMusic();

        }



        public string MapName
        {
            get
            {
                return ZoneManager.Instance.CurrentGround.Name.DefaultText;
            }
            set
            {
                lock (GameBase.lockObj)
                    this.RaiseAndSet(ref ZoneManager.Instance.CurrentGround.Name.DefaultText, value);
            }
        }

        public ObservableCollection<string> ScrollEdges { get; }

        public int ChosenScroll
        {
            get
            {
                return (int)ZoneManager.Instance.CurrentGround.EdgeView;
            }
            set
            {
                lock (GameBase.lockObj)
                {
                    ZoneManager.Instance.CurrentGround.EdgeView = (Map.ScrollEdge)value;
                    this.RaisePropertyChanged();
                }
            }
        }


        public bool NoSwitch
        {
            get { return ZoneManager.Instance.CurrentGround.NoSwitching; }
            set
            {
                ZoneManager.Instance.CurrentGround.NoSwitching = value;
                this.RaisePropertyChanged();
            }
        }


        public ClassBoxViewModel BG { get; set; }
        public TileBoxViewModel BlankBG { get; set; }

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


        public void BG_Changed()
        {
            ZoneManager.Instance.CurrentGround.Background = BG.GetObject<IBackgroundSprite>();
        }

        public void MapBG_Edit(object element, ClassBoxViewModel.EditElementOp op)
        {
            Type type = typeof(IBackgroundSprite);
            string elementName = type.Name;
            DataEditForm frmData = new DataEditRootForm();
            frmData.Title = DataEditor.GetWindowTitle(ZoneManager.Instance.CurrentGround.AssetName, elementName, element, type, new object[0]);

            DataEditor.LoadClassControls(frmData.ControlPanel, ZoneManager.Instance.CurrentGround.AssetName, null, elementName, type, new object[0], element, true, new Type[0]);

            frmData.SelectedOKEvent += async () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, type, new object[0], true, new Type[0]);
                op(element);
                return true;
            };

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            form.GroundEditForm.RegisterChild(frmData);
            frmData.Show();
        }


        public void BlankBG_Changed()
        {
            ZoneManager.Instance.CurrentGround.BlankBG = BlankBG.Tile;
        }

        public void AutoTile_Edit(AutoTile element, TileBoxViewModel.EditElementOp op)
        {
            TileEditForm frmData = new TileEditForm();
            TileEditViewModel tmv = new TileEditViewModel();
            frmData.DataContext = tmv;
            tmv.Name = element.ToString();

            //load as if eyedropping
            tmv.TileBrowser.TileSize = ZoneManager.Instance.CurrentGround.TileSize;
            tmv.AutotileBrowser.TileSize = ZoneManager.Instance.CurrentGround.TileSize;
            tmv.LoadTile(element);

            tmv.SelectedOKEvent += async () =>
            {
                element = tmv.GetTile();
                op(element);
                frmData.Close();
            };
            tmv.SelectedCancelEvent += () =>
            {
                frmData.Close();
            };

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            form.GroundEditForm.RegisterChild(frmData);
            frmData.Show();
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
                    if (song == ZoneManager.Instance.CurrentGround.Music)
                        ChosenMusic = ii+1;
                }
            }
        }

        private void musicChanged()
        {
            lock (GameBase.lockObj)
            {
                if (chosenMusic > 0)
                {
                    string fileName = (string)Music[chosenMusic];
                    ZoneManager.Instance.CurrentGround.Music = fileName;
                }
                else
                    ZoneManager.Instance.CurrentGround.Music = "";

                GameManager.Instance.BGM(ZoneManager.Instance.CurrentGround.Music, false);
            }
        }

        public void LoadMapProperties()
        {
            MapName = MapName;
            ChosenScroll = ChosenScroll;
            NoSwitch = NoSwitch;
            
            BG.LoadFromSource(ZoneManager.Instance.CurrentGround.Background);
            BlankBG.LoadFromSource(ZoneManager.Instance.CurrentGround.BlankBG);

            bool foundSong = false;
            for (int ii = 0; ii < Music.Count; ii++)
            {
                string song = Music[ii];
                if (song == ZoneManager.Instance.CurrentGround.Music)
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
