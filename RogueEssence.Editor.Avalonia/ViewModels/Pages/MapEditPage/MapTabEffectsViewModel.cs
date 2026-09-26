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
using RogueEssence.Dev.Utility;

namespace RogueEssence.Dev.ViewModels
{
    public class MapTabEffectsViewModel : ViewModelBase
    {
        private EditorContext _context;
        private EditorPageViewModel _parent;
        
        public MapTabEffectsViewModel(EditorContext context, EditorPageViewModel parent)
        {
            _context = context;
            _parent = parent;
            Statuses = new CollectionBoxViewModel(_context.DialogService, new StringConv(typeof(MapStatus), new object[0]));
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

        public void MapEffect_Edit(object element, bool advancedEdit, ClassBoxViewModel.EditElementOp op)
        {
            string elementName = "MapEffect";

            NodeBase node = _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(elementName, _parent.Node, _parent.Icon);
            _parent.Node.AddNodeIfNotExists(node);
            NodeHelper.ExpandParents(node, true);

            ReflectedDataPageViewModel newEditor = _context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
            newEditor.SetPageTitle(elementName, _parent.Node.Icon);
            newEditor.SetRootPage(true);
            newEditor.SetRemoveNode(true);

            newEditor.OnLoadAction = stack =>
            {
                DataEditor.LoadClassControls(stack, ZoneManager.Instance.CurrentMap.AssetName, null, elementName, typeof(ActiveEffect), new object[0], element, true, new Type[0], advancedEdit);
            };

            newEditor.OnOKAction = async stack =>
            {
                element = DataEditor.SaveClassControls(stack, elementName, typeof(ActiveEffect), new object[0], true, new Type[0], advancedEdit);
                op(element);
                return true;
            };

            _context.TabEvents.AddChildPage(_parent, newEditor);
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
        public void Statuses_EditItem(int index, object element, bool advancedEdit, CollectionBoxViewModel.EditElementOp op)
        {
            string elementName = "Statuses[" + index + "]";
            // string title = DataEditor.GetWindowTitle(ZoneManager.Instance.CurrentMap.AssetName, elementName, element, typeof(MapStatus), new object[0]);

            NodeBase node = _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(elementName, _parent.Node, _parent.Icon);
            _parent.Node.AddNodeIfNotExists(node);
            NodeHelper.ExpandParents(node, true);

            ReflectedDataPageViewModel newEditor = _context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
            newEditor.SetPageTitle(elementName, _parent.Node.Icon);
            newEditor.SetRootPage(true);
            newEditor.SetRemoveNode(true);

            newEditor.OnLoadAction = stack =>
            {
                DataEditor.LoadClassControls(stack, ZoneManager.Instance.CurrentMap.AssetName, null, elementName, typeof(MapStatus), new object[0], element, true, new Type[0], advancedEdit);
            };

            newEditor.OnOKAction = async stack =>
            {
                element = DataEditor.SaveClassControls(stack, elementName, typeof(MapStatus), new object[0], true, new Type[0], advancedEdit);

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
                    await MessageBoxWindowView.Show(_context.DialogService, "Cannot add duplicate IDs.", "Entry already exists.", MessageBoxWindowView.MessageBoxButtons.Ok);
                    return false;
                }

                op(index, element);
                return true;
            };

            _context.TabEvents.AddChildPage(_parent, newEditor);
        }

        public void Events_EditItem(int index, object element, bool advancedEdit, CollectionBoxViewModel.EditElementOp op)
        {
            string elementName = "Events[" + index + "]";
            string title = DataEditor.GetWindowTitle(ZoneManager.Instance.CurrentMap.AssetName, elementName, element, typeof(SingleCharEvent), new object[0]);

            NodeBase node = _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(elementName,  _parent.Node, _parent.Icon);
            _parent.Node.AddNodeIfNotExists(node);
            NodeHelper.ExpandParents(node, true);

            ReflectedDataPageViewModel newEditor = _context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
            newEditor.SetPageTitle(title, _parent.Node.Icon);
            newEditor.SetRootPage(true);
            newEditor.SetRemoveNode(true);
            
            newEditor.OnLoadAction = stack =>
            {
                DataEditor.LoadClassControls(stack, ZoneManager.Instance.CurrentMap.AssetName, null, elementName, typeof(SingleCharEvent), new object[0], element, true, new Type[0], advancedEdit);
            };

            newEditor.OnOKAction = async stack =>
            {
                element = DataEditor.SaveClassControls(stack, elementName, typeof(SingleCharEvent), new object[0], true, new Type[0], advancedEdit);
                op(index, element);
                return true;
            };

            _context.TabEvents.AddChildPage(_parent, newEditor);
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
