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

            Statuses = new CollectionBoxViewModel();
            Statuses.OnMemberChanged += Statuses_Changed;
            Statuses.OnEditItem += Statuses_EditItem;
            PrepareEvents = new CollectionBoxViewModel();
            PrepareEvents.OnMemberChanged += PrepareEvents_Changed;
            PrepareEvents.OnEditItem += Events_EditItem;
            StartEvents = new CollectionBoxViewModel();
            StartEvents.OnMemberChanged += StartEvents_Changed;
            StartEvents.OnEditItem += Events_EditItem;
            CheckEvents = new CollectionBoxViewModel();
            CheckEvents.OnMemberChanged += CheckEvents_Changed;
            CheckEvents.OnEditItem += Events_EditItem;
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
            get { return ZoneManager.Instance.CurrentMap.BGAnim.FrameTime; }
            set
            {
                ZoneManager.Instance.CurrentMap.BGAnim.FrameTime = value;
                this.RaisePropertyChanged();
            }
        }

        public int BGMoveX
        {
            get { return ZoneManager.Instance.CurrentMap.BGMovement.X; }
            set
            {
                ZoneManager.Instance.CurrentMap.BGMovement.X = value;
                this.RaisePropertyChanged();
            }
        }

        public int BGMoveY
        {
            get { return ZoneManager.Instance.CurrentMap.BGMovement.Y; }
            set
            {
                ZoneManager.Instance.CurrentMap.BGMovement.Y = value;
                this.RaisePropertyChanged();
            }
        }

        public CollectionBoxViewModel Statuses { get; set; }
        public CollectionBoxViewModel PrepareEvents { get; set; }
        public CollectionBoxViewModel StartEvents { get; set; }
        public CollectionBoxViewModel CheckEvents { get; set; }


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

        public void Statuses_Changed()
        {
            Dictionary<int, MapStatus> statuses = new Dictionary<int, MapStatus>();
            List<MapStatus> states = Statuses.GetList<List<MapStatus>>();
            for (int ii = 0; ii < states.Count; ii++)
                statuses[states[ii].ID] = states[ii];
            ZoneManager.Instance.CurrentMap.Status = statuses;
        }

        public void Statuses_EditItem(int index, object element, CollectionBoxViewModel.EditElementOp op)
        {
            DataEditForm frmData = new DataEditForm();
            if (element == null)
                frmData.Title = "New Status";
            else
                frmData.Title = element.ToString();

            DataEditor.LoadClassControls(frmData.ControlPanel, "(Statuses) [" + index + "]", typeof(MapStatus), new object[0] { }, element, true);

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            frmData.SelectedOKEvent += async () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, "Statuses", typeof(MapStatus), new object[0] { }, true);

                bool itemExists = false;

                List<MapStatus> states = (List<MapStatus>)Statuses.GetList(typeof(List<MapStatus>));
                for (int ii = 0; ii < states.Count; ii++)
                {
                    if (ii != index)
                    {
                        if (states[ii].ID == ((MapStatus)element).ID)
                            itemExists = true;
                    }
                }

                if (itemExists)
                    await MessageBox.Show((Window)form.MapEditor, "Cannot add duplicate IDs.", "Entry already exists.", MessageBox.MessageBoxButtons.Ok);
                else
                {
                    op(index, element);
                    frmData.Close();
                }
            };
            frmData.SelectedCancelEvent += () =>
            {
                frmData.Close();
            };

            //form.MapEditor.RegisterChild(frmData);
            frmData.Show();
        }

        public void PrepareEvents_Changed()
        {
            ZoneManager.Instance.CurrentMap.PrepareEvents = PrepareEvents.GetList<List<SingleCharEvent>>();
        }

        public void Events_EditItem(int index, object element, CollectionBoxViewModel.EditElementOp op)
        {
            string name = "Events";
            DataEditForm frmData = new DataEditForm();
            if (element == null)
                frmData.Title = name + "/" + "New Status";
            else
                frmData.Title = name + "/" + element.ToString();

            DataEditor.LoadClassControls(frmData.ControlPanel, "(List) " + name + "[" + index + "]", typeof(SingleCharEvent), new object[0], element, true);

            frmData.SelectedOKEvent += () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, name, typeof(SingleCharEvent), new object[0], true);
                op(index, element);
                frmData.Close();
            };
            frmData.SelectedCancelEvent += () =>
            {
                frmData.Close();
            };

            //DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            //form.MapEditor.RegisterChild(frmData);
            frmData.Show();
        }

        public void StartEvents_Changed()
        {
            ZoneManager.Instance.CurrentMap.StartEvents = StartEvents.GetList<List<SingleCharEvent>>();
        }

        public void CheckEvents_Changed()
        {
            ZoneManager.Instance.CurrentMap.CheckEvents = CheckEvents.GetList<List<SingleCharEvent>>();
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
            ChosenScroll = ChosenScroll;
            ChosenBG = ChosenBG;
            BGFrameTime = BGFrameTime;
            BGMoveX = BGMoveX;
            BGMoveY = BGMoveY;

            List<MapStatus> states = new List<MapStatus>();
            foreach (MapStatus state in ZoneManager.Instance.CurrentMap.Status.Values)
                states.Add(state);
            Statuses.LoadFromList(states);
            PrepareEvents.LoadFromList(ZoneManager.Instance.CurrentMap.PrepareEvents);
            StartEvents.LoadFromList(ZoneManager.Instance.CurrentMap.StartEvents);
            CheckEvents.LoadFromList(ZoneManager.Instance.CurrentMap.CheckEvents);

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
