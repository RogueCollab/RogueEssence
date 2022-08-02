using ReactiveUI;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RogueEssence.Dev.ViewModels
{
    public class MapTabItemsViewModel : ViewModelBase
    {
        public delegate void EntityOp(MapItem ent);

        public MapTabItemsViewModel()
        {
            SelectedEntity = MapItem.CreateMoney(1);

            ItemTypes = new ObservableCollection<string>();
            itemKeys = new List<string>();
            ItemTypes.Add("[Money]");
            itemKeys.Add("");
            Dictionary<string, string> monster_names = DataManager.Instance.DataIndices[DataManager.DataType.Item].GetLocalStringArray(true);
            foreach (string key in monster_names.Keys)
            {
                ItemTypes.Add(key + ": " + monster_names[key]);
                itemKeys.Add(key);
            }
        }

        private EntEditMode entMode;
        public EntEditMode EntMode
        {
            get { return entMode; }
            set
            {
                this.SetIfChanged(ref entMode, value);
                EntModeChanged();
            }
        }

        private int tabIndex;
        public int TabIndex
        {
            get => tabIndex;
            set
            {
                this.SetIfChanged(ref tabIndex, value);
            }
        }

        List<string> itemKeys;

        public ObservableCollection<string> ItemTypes { get; }

        public int ChosenItem
        {
            get
            {
                if (SelectedEntity.IsMoney)
                    return 0;
                else
                    return itemKeys.IndexOf(SelectedEntity.Value);
            }
            set
            {
                if (value == 0)
                {
                    SelectedEntity.IsMoney = true;
                    TabIndex = 0;
                }
                else
                {
                    SelectedEntity.IsMoney = false;
                    SelectedEntity.Value = itemKeys[value];
                    TabIndex = 1;
                }
                this.RaisePropertyChanged();
            }
        }

        public int Amount
        {
            get { return SelectedEntity.HiddenValue; }
            set
            {
                this.RaiseAndSet(ref SelectedEntity.HiddenValue, value);
            }
        }

        public int HiddenValue
        {
            get { return SelectedEntity.HiddenValue; }
            set
            {
                this.RaiseAndSet(ref SelectedEntity.HiddenValue, value);
            }
        }

        public bool Cursed
        {
            get { return SelectedEntity.Cursed; }
            set
            {
                this.RaiseAndSet(ref SelectedEntity.Cursed, value);
            }
        }

        public MapItem SelectedEntity;


        private void EntModeChanged()
        {
            if (entMode == EntEditMode.SelectEntity)
            {
                //do nothing
            }
            else
            {
                //copy the selection
                setEntity(new MapItem(SelectedEntity));
            }
        }

        public void ProcessUndo()
        {
            if (EntMode == EntEditMode.SelectEntity)
                SelectEntity(null);
        }

        public void ProcessInput(InputManager input)
        {
            if (!Collision.InBounds(GraphicsManager.WindowWidth, GraphicsManager.WindowHeight, input.MouseLoc))
                return;

            Loc mapCoords = DungeonEditScene.Instance.ScreenCoordsToMapCoords(input.MouseLoc);

            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, mapCoords))
                return;

            switch (EntMode)
            {
                case EntEditMode.PlaceEntity:
                    {
                        if (input.JustPressed(FrameInput.InputType.LeftMouse))
                            PlaceEntity(mapCoords);
                        else if (input.JustPressed(FrameInput.InputType.RightMouse))
                            RemoveEntityAt(mapCoords);
                        break;
                    }
                case EntEditMode.SelectEntity:
                    {
                        if (input.JustPressed(FrameInput.InputType.LeftMouse))
                            SelectEntityAt(mapCoords);
                        else if (input[FrameInput.InputType.LeftMouse])
                            MoveEntity(mapCoords);
                        else if (input.Direction != input.PrevDirection)
                            MoveEntity(SelectedEntity.TileLoc + input.Direction.GetLoc());
                        break;
                    }
            }
        }



        /// <summary>
        /// Select the entity at that position and displays its data for editing
        /// </summary>
        /// <param name="position"></param>
        public void RemoveEntityAt(Loc position)
        {
            OperateOnEntityAt(position, RemoveEntity);
        }

        public void RemoveEntity(MapItem ent)
        {
            if (ent == null)
                return;

            DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new MapItemStateUndo());
            ZoneManager.Instance.CurrentMap.Items.Remove(ent);
        }

        public void PlaceEntity(Loc position)
        {
            RemoveEntityAt(position);

            MapItem placeableEntity = new MapItem(SelectedEntity);

            placeableEntity.TileLoc = position;

            DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new MapItemStateUndo());
            ZoneManager.Instance.CurrentMap.Items.Add(placeableEntity);
        }



        public void SelectEntity(MapItem ent)
        {
            if (ent != null)
            {
                DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new MapItemStateUndo());
                setEntity(ent);
            }
            else
                setEntity(new MapItem());
        }

        private void setEntity(MapItem ent)
        {
            SelectedEntity = ent;
            ChosenItem = ChosenItem;
            Amount = Amount;
            HiddenValue = HiddenValue;
            Cursed = Cursed;
        }

        /// <summary>
        /// Select the entity at that position and displays its data for editing
        /// </summary>
        /// <param name="position"></param>
        public void SelectEntityAt(Loc position)
        {
            OperateOnEntityAt(position, SelectEntity);
        }

        public void OperateOnEntityAt(Loc position, EntityOp op)
        {
            int idx = ZoneManager.Instance.CurrentMap.GetItem(position);
            if (idx > -1)
                op(ZoneManager.Instance.CurrentMap.Items[idx]);
            else
                op(null);
        }

        private void MoveEntity(Loc loc)
        {
            if (SelectedEntity != null)
                SelectedEntity.TileLoc = loc;
        }
    }

    public class MapItemStateUndo : StateUndo<List<MapItem>>
    {
        public MapItemStateUndo()
        {
        }

        public override List<MapItem> GetState()
        {
            return ZoneManager.Instance.CurrentMap.Items;
        }

        public override void SetState(List<MapItem> state)
        {
            ZoneManager.Instance.CurrentMap.Items = state;
        }
    }
}
