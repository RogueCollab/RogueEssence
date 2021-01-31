using ReactiveUI;
using RogueElements;
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
            SelectedEntity = new MapItem();

            ItemTypes = new ObservableCollection<string>();
            ItemTypes.Add("---: Money");
            string[] monster_names = DataManager.Instance.DataIndices[DataManager.DataType.Item].GetLocalStringArray();
            for (int ii = 0; ii < monster_names.Length; ii++)
                ItemTypes.Add(ii.ToString("D3") + ": " + monster_names[ii]);

        }

        private EntEditMode entMode;
        public EntEditMode EntMode
        {
            get { return entMode; }
            set
            {
                this.SetIfChanged(ref entMode, value);
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

        public ObservableCollection<string> ItemTypes { get; }

        public int ChosenItem
        {
            get
            {
                if (SelectedEntity.IsMoney)
                    return 0;
                else
                    return SelectedEntity.Value + 1;
            }
            set
            {
                if (value == 0)
                {
                    SelectedEntity.IsMoney = true;
                    Amount = 1;
                    TabIndex = 0;
                }
                else
                {
                    SelectedEntity.IsMoney = false;
                    SelectedEntity.Value = value - 1;
                    TabIndex = 1;
                }
                this.RaisePropertyChanged();
            }
        }

        public int Amount
        {
            get { return SelectedEntity.Value; }
            set
            {
                this.SetIfChanged(ref SelectedEntity.Value, value);
            }
        }

        public int HiddenValue
        {
            get { return SelectedEntity.HiddenValue; }
            set
            {
                this.SetIfChanged(ref SelectedEntity.HiddenValue, value);
            }
        }

        public bool Cursed
        {
            get { return SelectedEntity.Cursed; }
            set
            {
                this.SetIfChanged(ref SelectedEntity.Cursed, value);
            }
        }

        public MapItem SelectedEntity;



        public void ProcessInput(InputManager input)
        {
            Loc mapCoords = DungeonEditScene.Instance.ScreenCoordsToMapCoords(input.MouseLoc);

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

            ZoneManager.Instance.CurrentMap.Items.Remove(ent);
        }

        public void PlaceEntity(Loc position)
        {
            RemoveEntityAt(position);

            MapItem placeableEntity = new MapItem(SelectedEntity);

            placeableEntity.TileLoc = position;
            ZoneManager.Instance.CurrentMap.Items.Add(placeableEntity);
        }



        public void SelectEntity(MapItem ent)
        {
            if (ent != null)
                setEntity(ent);
            else
                setEntity(new MapItem());
        }

        private void setEntity(MapItem ent)
        {
            SelectedEntity = ent;
            ChosenItem = ChosenItem;
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
}
