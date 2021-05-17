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
    public class MapTabEffectsViewModel : ViewModelBase
    {
        public MapTabEffectsViewModel()
        {
            Statuses = new CollectionBoxViewModel(new StringConv(typeof(MapStatus), new object[0]));
            Statuses.OnMemberChanged += Statuses_Changed;
            Statuses.OnEditItem += Statuses_EditItem;

            MapEffect = new ClassBoxViewModel(new StringConv(typeof(ActiveEffect), new object[0]));
            MapEffect.OnMemberChanged += MapEffect_Changed;
            MapEffect.OnEditItem += MapEffect_Edit;

            CheckEvents = new CollectionBoxViewModel(new StringConv(typeof(SingleCharEvent), new object[0]));
            CheckEvents.OnMemberChanged += CheckEvents_Changed;
            CheckEvents.OnEditItem += Events_EditItem;

        }



        public CollectionBoxViewModel Statuses { get; set; }
        public ClassBoxViewModel MapEffect { get; set; }
        public CollectionBoxViewModel CheckEvents { get; set; }


        public void MapEffect_Changed()
        {
            ZoneManager.Instance.CurrentMap.MapEffect = MapEffect.GetObject<ActiveEffect>();
        }

        public void MapEffect_Edit(object element, ClassBoxViewModel.EditElementOp op)
        {
            DataEditForm frmData = new DataEditForm();
            frmData.Title = element.ToString();

            DataEditor.LoadClassControls(frmData.ControlPanel, "MapEffect", typeof(ActiveEffect), new object[0] { }, element, true);

            frmData.SelectedOKEvent += () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, "MapEffect", typeof(ActiveEffect), new object[0] { }, true);
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

        public void Statuses_Changed()
        {
            Dictionary<int, MapStatus> statuses = new Dictionary<int, MapStatus>();
            List<MapStatus> states = Statuses.GetList<List<MapStatus>>();
            for (int ii = 0; ii < states.Count; ii++)
                statuses[states[ii].ID] = states[ii];
            ZoneManager.Instance.CurrentMap.Status = statuses;
        }

        //TODO: move these events into ListEditor; they were generic enough to warrant copy+pasting
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
                    await MessageBox.Show(form.MapEditForm, "Cannot add duplicate IDs.", "Entry already exists.", MessageBox.MessageBoxButtons.Ok);
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

            form.MapEditForm.RegisterChild(frmData);
            frmData.Show();
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

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            form.MapEditForm.RegisterChild(frmData);
            frmData.Show();
        }

        public void CheckEvents_Changed()
        {
            ZoneManager.Instance.CurrentMap.CheckEvents = CheckEvents.GetList<List<SingleCharEvent>>();
        }

        public void LoadMapEffects()
        {
            List<MapStatus> states = new List<MapStatus>();
            foreach (MapStatus state in ZoneManager.Instance.CurrentMap.Status.Values)
                states.Add(state);
            Statuses.LoadFromList(states);
            MapEffect.LoadFromSource(ZoneManager.Instance.CurrentMap.MapEffect);
            CheckEvents.LoadFromList(ZoneManager.Instance.CurrentMap.CheckEvents);

        }


    }
}
