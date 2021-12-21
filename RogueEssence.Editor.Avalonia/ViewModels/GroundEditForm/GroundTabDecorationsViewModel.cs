using ReactiveUI;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class GroundTabDecorationsViewModel : ViewModelBase
    {
        public delegate void EntityOp(GroundAnim ent);

        public GroundTabDecorationsViewModel()
        {
            Layers = new AnimLayerBoxViewModel(DiagManager.Instance.DevEditor.GroundEditor.Edits);
            Layers.SelectedLayerChanged += Layers_SelectedLayerChanged;
            SelectedEntity = new GroundAnim();

            Directions = new ObservableCollection<string>();
            foreach (Dir8 dir in DirExt.VALID_DIR8)
                Directions.Add(dir.ToLocal());

            AnimTypes = new ObservableCollection<string>();
            assignables = typeof(IPlaceableAnimData).GetAssignableTypes();
            foreach (Type type in assignables)
            {
                IPlaceableAnimData newData = (IPlaceableAnimData)ReflectionExt.CreateMinimalInstance(type);
                AnimTypes.Add(newData.AssetType.ToString());
            }
            ObjectAnims = new ObservableCollection<string>();
            ChosenAnimType = ChosenAnimType;

            FrameLength = 1;
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

        public ILayerBoxViewModel Layers { get; set; }

        public bool ShowBoxes
        {
            get
            {
                return GroundEditScene.Instance.ShowObjectBoxes;
            }
            set
            {
                GroundEditScene.Instance.ShowObjectBoxes = value;
                this.RaisePropertyChanged();
            }
        }


        private Type[] assignables;
        public ObservableCollection<string> AnimTypes { get; }

        public int ChosenAnimType
        {
            get => Array.IndexOf(assignables, SelectedEntity.ObjectAnim.GetType());
            set
            {
                if (value < 0)
                    return;
                Type type = assignables[value];
                IPlaceableAnimData newData = (IPlaceableAnimData)ReflectionExt.CreateMinimalInstance(type);
                newData.LoadFrom(SelectedEntity.ObjectAnim);
                SelectedEntity.ObjectAnim = newData;
                this.RaisePropertyChanged();
                animTypeChanged();
            }
        }


        public ObservableCollection<string> ObjectAnims { get; }

        public int ChosenObjectAnim
        {
            get => ObjectAnims.IndexOf(SelectedEntity.ObjectAnim.AnimIndex);
            set
            {
                SelectedEntity.ObjectAnim.AnimIndex = value > -1 ? ObjectAnims[value] : "";
                this.RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Directions { get; }

        public int ChosenDirection
        {
            get => (int)SelectedEntity.ObjectAnim.AnimDir;
            set
            {
                SelectedEntity.ObjectAnim.AnimDir = (Dir8)value;
                this.RaisePropertyChanged();
            }
        }


        public int StartFrame
        {
            get => SelectedEntity.ObjectAnim.StartFrame;
            set
            {
                SelectedEntity.ObjectAnim.StartFrame = value;
                this.RaisePropertyChanged();
            }
        }

        public int EndFrame
        {
            get => SelectedEntity.ObjectAnim.EndFrame;
            set
            {
                SelectedEntity.ObjectAnim.EndFrame = value;
                this.RaisePropertyChanged();
            }
        }

        public int FrameLength
        {
            get => SelectedEntity.ObjectAnim.FrameTime;
            set
            {
                SelectedEntity.ObjectAnim.FrameTime = value;
                this.RaisePropertyChanged();
            }
        }


        public bool FlipHoriz
        {
            get => (SelectedEntity.ObjectAnim.AnimFlip & SpriteFlip.Horiz) != SpriteFlip.None;
            set
            {
                if (value)
                    SelectedEntity.ObjectAnim.AnimFlip |= SpriteFlip.Horiz;
                else
                    SelectedEntity.ObjectAnim.AnimFlip &= ~SpriteFlip.Horiz;
                this.RaisePropertyChanged();
            }
        }


        public bool FlipVert
        {
            get => (SelectedEntity.ObjectAnim.AnimFlip & SpriteFlip.Vert) != SpriteFlip.None;
            set
            {
                if (value)
                    SelectedEntity.ObjectAnim.AnimFlip |= SpriteFlip.Vert;
                else
                    SelectedEntity.ObjectAnim.AnimFlip &= ~SpriteFlip.Vert;
                this.RaisePropertyChanged();
            }
        }




        public GroundAnim SelectedEntity;
        private Loc dragDiff;


        private void EntModeChanged()
        {
            if (entMode == EntEditMode.SelectEntity)
            {
                //do nothing
            }
            else
            {
                //copy the selection
                if (GroundEditScene.Instance.SelectedDecoration != null)
                {
                    setEntity(new GroundAnim(GroundEditScene.Instance.SelectedDecoration));
                    GroundEditScene.Instance.SelectedDecoration = null;
                }
            }
        }

        public void ProcessInput(InputManager input)
        {
            if (!Collision.InBounds(GraphicsManager.WindowWidth, GraphicsManager.WindowHeight, input.MouseLoc))
                return;

            Loc groundCoords = GroundEditScene.Instance.ScreenCoordsToGroundCoords(input.MouseLoc);

            bool snapGrid = input.BaseKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || input.BaseKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl);
            if (snapGrid)
                groundCoords = new Loc((int)Math.Round((float)groundCoords.X / GraphicsManager.TEX_SIZE) * GraphicsManager.TEX_SIZE,
                    (int)Math.Round((float)groundCoords.Y / GraphicsManager.TEX_SIZE) * GraphicsManager.TEX_SIZE);

            switch (EntMode)
            {
                case EntEditMode.PlaceEntity:
                    {
                        if (GroundEditScene.Instance.DecorationInProgress == null)
                        {
                            if (input.JustPressed(FrameInput.InputType.LeftMouse))
                                PendEntity(groundCoords);
                            else if (!input[FrameInput.InputType.LeftMouse] && input.JustReleased(FrameInput.InputType.RightMouse))
                                RemoveEntityAt(groundCoords);
                        }
                        else
                        {
                            GroundEditScene.Instance.DecorationInProgress.MapLoc = groundCoords;
                            if (input.JustReleased(FrameInput.InputType.LeftMouse))
                                PlaceEntity();
                            else if (input.JustPressed(FrameInput.InputType.RightMouse))
                                GroundEditScene.Instance.DecorationInProgress = null;
                        }
                        break;
                    }
                case EntEditMode.SelectEntity:
                    {
                        if (input.JustPressed(FrameInput.InputType.LeftMouse))
                        {
                            SelectEntityAt(groundCoords);
                            if (SelectedEntity != null)
                                dragDiff = groundCoords - SelectedEntity.MapLoc;
                        }
                        else if (input[FrameInput.InputType.LeftMouse])
                            MoveEntity(groundCoords - dragDiff);
                        else if (input.Direction != input.PrevDirection)
                        {
                            Loc diff = input.Direction.GetLoc();
                            if (snapGrid)
                                diff *= 8;
                            MoveEntity(SelectedEntity.MapLoc + diff);
                        }
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

        public void RemoveEntity(GroundAnim ent)
        {
            if (ent == null)
                return;

            DiagManager.Instance.DevEditor.GroundEditor.Edits.Apply(new GroundDecorationStateUndo(Layers.ChosenLayer));

            ZoneManager.Instance.CurrentGround.Decorations[Layers.ChosenLayer].Anims.Remove(ent);
        }

        public void PendEntity(Loc position)
        {
            GroundAnim placeableEntity = new GroundAnim((IPlaceableAnimData)SelectedEntity.ObjectAnim.Clone(), position);
            GroundEditScene.Instance.DecorationInProgress = placeableEntity;
        }
        public void PlaceEntity()
        {
            GroundAnim placeableEntity = GroundEditScene.Instance.DecorationInProgress;
            GroundEditScene.Instance.DecorationInProgress = null;
            DiagManager.Instance.DevEditor.GroundEditor.Edits.Apply(new GroundDecorationStateUndo(Layers.ChosenLayer));

            ZoneManager.Instance.CurrentGround.Decorations[Layers.ChosenLayer].Anims.Add(placeableEntity);
        }


        public void SelectEntity(GroundAnim ent)
        {
            if (ent != null)
            {
                DiagManager.Instance.DevEditor.GroundEditor.Edits.Apply(new GroundSelectUndo());
                GroundEditScene.Instance.SelectedDecoration = ent;
                setEntity(ent);
            }
            else
            {
                GroundEditScene.Instance.SelectedDecoration = ent;
                setEntity(new GroundAnim(new ObjAnimData(ObjectAnims[0], 1, Dir8.Down), Loc.Zero));
            }
        }

        private void setEntity(GroundAnim ent)
        {
            SelectedEntity = ent;
            ChosenAnimType = ChosenAnimType;
            ChosenObjectAnim = ChosenObjectAnim;
            ChosenDirection = ChosenDirection;
            StartFrame = StartFrame;
            EndFrame = EndFrame;
            FrameLength = FrameLength;
            FlipHoriz = FlipHoriz;
            FlipVert = FlipVert;
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
            List<GroundAnim> found = ZoneManager.Instance.CurrentGround.Decorations[Layers.ChosenLayer].FindAnimsAtPosition(position);
            if (found.Count > 0)
                op(found.First());
            else
                op(null);
        }

        private void MoveEntity(Loc loc)
        {
            if (SelectedEntity != null)
                SelectedEntity.MapLoc = loc;
        }

        public void Layers_SelectedLayerChanged()
        {
            if (EntMode == EntEditMode.SelectEntity)
                SelectEntity(null);
        }

        private void animTypeChanged()
        {
            string oldIndex = SelectedEntity.ObjectAnim.AnimIndex;
            ObjectAnims.Clear();
            string[] dirs = PathMod.GetModFiles(GraphicsManager.CONTENT_PATH + SelectedEntity.ObjectAnim.AssetType.ToString() + "/");
            int newAnim = 0;
            for (int ii = 0; ii < dirs.Length; ii++)
            {
                string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                ObjectAnims.Add(filename);

                if (filename == oldIndex)
                    newAnim = ii;
            }
            ChosenObjectAnim = newAnim;
        }
    }

    public class GroundDecorationStateUndo : StateUndo<AnimLayer>
    {
        private int layer;
        public GroundDecorationStateUndo(int layer)
        {
            this.layer = layer;
        }

        public override AnimLayer GetState()
        {
            return ZoneManager.Instance.CurrentGround.Decorations[layer];
        }

        public override void SetState(AnimLayer state)
        {
            ZoneManager.Instance.CurrentGround.Decorations[layer] = state;
        }
    }
}
