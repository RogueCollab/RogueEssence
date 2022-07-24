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

        }



        public CollectionBoxViewModel Statuses { get; set; }
        public ClassBoxViewModel MapEffect { get; set; }


        public void MapEffect_Changed()
        {
            ZoneManager.Instance.CurrentMap.MapEffect = MapEffect.GetObject<ActiveEffect>();
        }

        public void MapEffect_Edit(object element, ClassBoxViewModel.EditElementOp op)
        {
            string elementName = "MapEffect";
            DataEditForm frmData = new DataEditRootForm();
            frmData.Title = DataEditor.GetWindowTitle(ZoneManager.Instance.CurrentMap.AssetName, elementName, element, typeof(ActiveEffect), new object[0]);

            DataEditor.LoadClassControls(frmData.ControlPanel, ZoneManager.Instance.CurrentMap.AssetName, null, elementName, typeof(ActiveEffect), new object[0], element, true, new Type[0]);
            DataEditor.TrackTypeSize(frmData, typeof(ActiveEffect));

            frmData.SelectedOKEvent += async () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, typeof(ActiveEffect), new object[0], true, new Type[0]);
                op(element);
                return true;
            };

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            form.MapEditForm.RegisterChild(frmData);
            frmData.Show();
        }

        public void Statuses_Changed()
        {
            Dictionary<string, MapStatus> statuses = new Dictionary<string, MapStatus>();
            List<MapStatus> states = Statuses.GetList<List<MapStatus>>();
            for (int ii = 0; ii < states.Count; ii++)
                statuses[states[ii].ID] = states[ii];
            ZoneManager.Instance.CurrentMap.Status = statuses;
        }

        //TODO: move these events into ListEditor; they were generic enough to warrant copy+pasting
        public void Statuses_EditItem(int index, object element, CollectionBoxViewModel.EditElementOp op)
        {
            string elementName = "Statuses[" + index + "]";
            DataEditForm frmData = new DataEditRootForm();
            frmData.Title = DataEditor.GetWindowTitle(ZoneManager.Instance.CurrentMap.AssetName, elementName, element, typeof(MapStatus), new object[0]);

            DataEditor.LoadClassControls(frmData.ControlPanel, ZoneManager.Instance.CurrentMap.AssetName, null, elementName, typeof(MapStatus), new object[0], element, true, new Type[0]);
            DataEditor.TrackTypeSize(frmData, typeof(MapStatus));

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            frmData.SelectedOKEvent += async () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, typeof(MapStatus), new object[0], true, new Type[0]);

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
                {
                    await MessageBox.Show(frmData, "Cannot add duplicate IDs.", "Entry already exists.", MessageBox.MessageBoxButtons.Ok);
                    return false;
                }
                else
                {
                    op(index, element);
                    return true;
                }
            };

            form.MapEditForm.RegisterChild(frmData);
            frmData.Show();
        }

        public void Events_EditItem(int index, object element, CollectionBoxViewModel.EditElementOp op)
        {
            string elementName = "Events[" + index + "]";
            DataEditForm frmData = new DataEditRootForm();
            frmData.Title = DataEditor.GetWindowTitle(ZoneManager.Instance.CurrentMap.AssetName, elementName, element, typeof(SingleCharEvent), new object[0]);

            DataEditor.LoadClassControls(frmData.ControlPanel, ZoneManager.Instance.CurrentMap.AssetName, null, elementName, typeof(SingleCharEvent), new object[0], element, true, new Type[0]);
            DataEditor.TrackTypeSize(frmData, typeof(SingleCharEvent));

            frmData.SelectedOKEvent += async () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, typeof(SingleCharEvent), new object[0], true, new Type[0]);
                op(index, element);
                return true;
            };

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            form.MapEditForm.RegisterChild(frmData);
            frmData.Show();
        }

        public void LoadMapEffects()
        {
            List<MapStatus> states = new List<MapStatus>();
            foreach (MapStatus state in ZoneManager.Instance.CurrentMap.Status.Values)
                states.Add(state);
            Statuses.LoadFromList(states);
            MapEffect.LoadFromSource(ZoneManager.Instance.CurrentMap.MapEffect);

        }


    }
}
