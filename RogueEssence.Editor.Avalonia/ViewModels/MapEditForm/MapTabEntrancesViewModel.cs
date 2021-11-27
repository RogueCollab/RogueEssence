using ReactiveUI;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RogueEssence.Dev.ViewModels
{
    public class MapTabEntrancesViewModel : ViewModelBase
    {
        public delegate void EntityOp(LocRay8? ent);

        public MapTabEntrancesViewModel()
        {
            SelectedEntity = new LocRay8();

            Directions = new ObservableCollection<string>();
            foreach (Dir8 dir in DirExt.VALID_DIR8)
                Directions.Add(dir.ToLocal());
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

        public bool ShowEntrances
        {
            get { return DungeonEditScene.Instance.ShowEntrances; }
            set { this.RaiseAndSet(ref DungeonEditScene.Instance.ShowEntrances, value); }
        }

        public ObservableCollection<string> Directions { get; }

        public int ChosenDir
        {
            get => (int)SelectedEntity.Dir;
            set
            {
                SelectedEntity.Dir = (Dir8)value;
                this.RaisePropertyChanged();
            }
        }


        public LocRay8 SelectedEntity;



        public void SetupLayerVisibility()
        {
            ShowEntrances = ShowEntrances;
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
                            MoveEntity(SelectedEntity.Loc + input.Direction.GetLoc());
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

        public void RemoveEntity(LocRay8? ent)
        {
            if (ent == null)
                return;

            DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new MapItemStateUndo());
            ZoneManager.Instance.CurrentMap.EntryPoints.Remove(ent.Value);
        }

        public void PlaceEntity(Loc position)
        {
            RemoveEntityAt(position);

            LocRay8 placeableEntity = new LocRay8(SelectedEntity.Loc, SelectedEntity.Dir);

            placeableEntity.Loc = position;

            DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new MapItemStateUndo());
            ZoneManager.Instance.CurrentMap.EntryPoints.Add(placeableEntity);
        }



        public void SelectEntity(LocRay8? ent)
        {
            if (ent != null)
            {
                DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new MapItemStateUndo());
                setEntity(ent.Value);
            }
            else
                setEntity(new LocRay8());
        }

        private void setEntity(LocRay8 ent)
        {
            SelectedEntity = ent;
            ChosenDir = ChosenDir;
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
            int idx = ZoneManager.Instance.CurrentMap.EntryPoints.FindIndex((a) => a.Loc == position);
            if (idx > -1)
                op(ZoneManager.Instance.CurrentMap.EntryPoints[idx]);
            else
                op(null);
        }

        private void MoveEntity(Loc loc)
        {
            SelectedEntity.Loc = loc;
        }
    }

    public class MapEntryStateUndo : StateUndo<List<LocRay8>>
    {
        public MapEntryStateUndo()
        {
        }

        public override List<LocRay8> GetState()
        {
            return ZoneManager.Instance.CurrentMap.EntryPoints;
        }

        public override void SetState(List<LocRay8> state)
        {
            ZoneManager.Instance.CurrentMap.EntryPoints = state;
        }
    }
}
