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
    public class MapTabPropertiesViewModel : ViewModelBase
    {
        public MapTabPropertiesViewModel()
        {
            Sights = new ObservableCollection<string>();
            for (int ii = 0; ii <= (int)Map.SightRange.Blind; ii++)
                Sights.Add(((Map.SightRange)ii).ToLocal());

            Elements = new ObservableCollection<string>();
            string[] element_names = DataManager.Instance.DataIndices[DataManager.DataType.Element].GetLocalStringArray();
            for (int ii = 0; ii < element_names.Length; ii++)
                Elements.Add(ii.ToString("D2") + ": " + element_names[ii]);

            ScrollEdges = new ObservableCollection<string>();
            for (int ii = 0; ii <= (int)Map.ScrollEdge.Clamp; ii++)
                ScrollEdges.Add(((Map.ScrollEdge)ii).ToLocal());

            BG = new ClassBoxViewModel();
            BG.OnMemberChanged += BG_Changed;
            BG.OnEditItem += MapBG_Edit;
            BlankBG = new ClassBoxViewModel();
            BlankBG.OnMemberChanged += BlankBG_Changed;
            BlankBG.OnEditItem += AutoTile_Edit;
            FloorBG = new ClassBoxViewModel();
            FloorBG.OnMemberChanged += FloorBG_Changed;
            FloorBG.OnEditItem += AutoTile_Edit;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            TextureMap = new DictionaryBoxViewModel(form.MapEditForm);
            TextureMap.OnMemberChanged += TextureMap_Changed;
            TextureMap.OnEditKey += TextureMap_EditKey;
            TextureMap.OnEditItem += TextureMap_EditItem;

            Music = new ObservableCollection<string>();
            reloadMusic();

        }



        public string MapName
        {
            get { return ZoneManager.Instance.CurrentMap.Name.DefaultText; }
            set
            {
                lock (GameBase.lockObj)
                    this.RaiseAndSet(ref ZoneManager.Instance.CurrentMap.Name.DefaultText, value);
            }
        }

        public ObservableCollection<string> Sights { get; }

        public int ChosenTileSight
        {
            get { return (int)ZoneManager.Instance.CurrentMap.TileSight; }
            set
            {
                ZoneManager.Instance.CurrentMap.TileSight = (Map.SightRange)value;
                this.RaisePropertyChanged();
            }
        }
        public int ChosenCharSight
        {
            get { return (int)ZoneManager.Instance.CurrentMap.CharSight; }
            set
            {
                ZoneManager.Instance.CurrentMap.CharSight = (Map.SightRange)value;
                this.RaisePropertyChanged();
            }
        }

        public bool NoRescue
        {
            get { return ZoneManager.Instance.CurrentMap.NoRescue; }
            set
            {
                ZoneManager.Instance.CurrentMap.NoRescue = value;
                this.RaisePropertyChanged();
            }
        }

        public bool NoSwitch
        {
            get { return ZoneManager.Instance.CurrentMap.NoSwitching; }
            set
            {
                ZoneManager.Instance.CurrentMap.NoSwitching = value;
                this.RaisePropertyChanged();
            }
        }


        public ObservableCollection<string> Elements { get; }

        public int ChosenElement
        {
            get { return ZoneManager.Instance.CurrentMap.Element; }
            set
            {
                ZoneManager.Instance.CurrentMap.Element = value;
                this.RaisePropertyChanged();
            }
        }


        public ObservableCollection<string> ScrollEdges { get; }

        public int ChosenScroll
        {
            get { return (int)ZoneManager.Instance.CurrentMap.EdgeView; }
            set
            {
                lock (GameBase.lockObj)
                {
                    ZoneManager.Instance.CurrentMap.EdgeView = (Map.ScrollEdge)value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public ClassBoxViewModel BG { get; set; }
        public ClassBoxViewModel BlankBG { get; set; }
        public ClassBoxViewModel FloorBG { get; set; }

        public DictionaryBoxViewModel TextureMap { get; set; }

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
            ZoneManager.Instance.CurrentMap.Background = BG.GetObject<MapBG>();
        }

        public void MapBG_Edit(object element, ClassBoxViewModel.EditElementOp op)
        {
            DataEditForm frmData = new DataEditForm();
            frmData.Title = element.ToString();

            DataEditor.LoadClassControls(frmData.ControlPanel, "MapBG", typeof(MapBG), new object[0] { }, element, true);

            frmData.SelectedOKEvent += () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, "MapBG", typeof(MapBG), new object[0] { }, true);
                op(element);
                frmData.Close();
            };
            frmData.SelectedCancelEvent += () =>
            {
                frmData.Close();
            };

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            form.MapEditForm.RegisterChild(frmData);
            frmData.Show();
        }

        public void BlankBG_Changed()
        {
            ZoneManager.Instance.CurrentMap.BlankBG = BlankBG.GetObject<AutoTile>();
        }

        public void FloorBG_Changed()
        {
            ZoneManager.Instance.CurrentMap.FloorBG = FloorBG.GetObject<AutoTile>();
        }

        public void AutoTile_Edit(object element, ClassBoxViewModel.EditElementOp op)
        {
            DataEditForm frmData = new DataEditForm();
            frmData.Title = element.ToString();

            DataEditor.LoadClassControls(frmData.ControlPanel, "Autotile", typeof(AutoTile), new object[0] { }, element, true);

            frmData.SelectedOKEvent += () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, "Autotile", typeof(AutoTile), new object[0] { }, true);
                op(element);
                frmData.Close();
            };
            frmData.SelectedCancelEvent += () =>
            {
                frmData.Close();
            };

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            form.MapEditForm.RegisterChild(frmData);
            frmData.Show();
        }


        public void TextureMap_Changed()
        {
            ZoneManager.Instance.CurrentMap.TextureMap = TextureMap.GetDict<Dictionary<int, AutoTile>>();
        }

        public void TextureMap_EditKey(object key, object element, DictionaryBoxViewModel.EditElementOp op)
        {
            DataEditForm frmKey = new DataEditForm();
            if (element == null)
                frmKey.Title = "New Key";
            else
                frmKey.Title = element.ToString();

            DataEditor.LoadClassControls(frmKey.ControlPanel, "(TextureMap) <New Key>", typeof(int), new object[0] { }, null, true);

            frmKey.SelectedOKEvent += () =>
            {
                key = DataEditor.SaveClassControls(frmKey.ControlPanel, "TextureMap", typeof(int), new object[0] { }, true);
                op(key, element);
                frmKey.Close();
            };
            frmKey.SelectedCancelEvent += () =>
            {
                frmKey.Close();
            };

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            form.MapEditForm.RegisterChild(frmKey);
            frmKey.Show();
        }

        public void TextureMap_EditItem(object key, object element, DictionaryBoxViewModel.EditElementOp op)
        {
            DataEditForm frmData = new DataEditForm();
            if (element == null)
                frmData.Title = "New Autotile";
            else
                frmData.Title = element.ToString();

            DataEditor.LoadClassControls(frmData.ControlPanel, "(TextureMap) [" + key.ToString() + "]", typeof(AutoTile), new object[0] { }, element, true);

            frmData.SelectedOKEvent += () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, "TextureMap", typeof(AutoTile), new object[0] { }, true);
                op(key, element);
                frmData.Close();
            };
            frmData.SelectedCancelEvent += () =>
            {
                frmData.Close();
            };

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            form.MapEditForm.RegisterChild(frmData);
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
                    if (song == ZoneManager.Instance.CurrentMap.Music)
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
                    ZoneManager.Instance.CurrentMap.Music = fileName;
                }
                else
                    ZoneManager.Instance.CurrentMap.Music = "";

                GameManager.Instance.BGM(ZoneManager.Instance.CurrentMap.Music, false);
            }
        }

        public void LoadMapProperties()
        {
            MapName = MapName;
            ChosenTileSight = ChosenTileSight;
            ChosenCharSight = ChosenCharSight;
            NoRescue = NoRescue;
            NoSwitch = NoSwitch;
            ChosenElement = ChosenElement;
            ChosenScroll = ChosenScroll;

            BG.LoadFromSource(ZoneManager.Instance.CurrentMap.Background);
            BlankBG.LoadFromSource(ZoneManager.Instance.CurrentMap.BlankBG);
            FloorBG.LoadFromSource(ZoneManager.Instance.CurrentMap.FloorBG);
            TextureMap.LoadFromDict(ZoneManager.Instance.CurrentMap.TextureMap);

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
