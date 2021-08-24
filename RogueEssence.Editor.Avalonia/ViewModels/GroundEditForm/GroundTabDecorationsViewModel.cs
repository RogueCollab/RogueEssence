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

            ObjectAnims = new ObservableCollection<string>();
            string[] dirs = PathMod.GetModFiles(GraphicsManager.CONTENT_PATH + "Object/");
            for (int ii = 0; ii < dirs.Length; ii++)
            {
                string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                ObjectAnims.Add(filename);
            }
            ChosenObjectAnim = 0;

            FrameLength = 1;
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

        public ILayerBoxViewModel Layers { get; set; }


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
            set => this.RaiseAndSet(ref SelectedEntity.ObjectAnim.StartFrame, value);
        }

        public int EndFrame
        {
            get => SelectedEntity.ObjectAnim.EndFrame;
            set => this.RaiseAndSet(ref SelectedEntity.ObjectAnim.EndFrame, value);
        }

        public int FrameLength
        {
            get => SelectedEntity.ObjectAnim.FrameTime;
            set => this.RaiseAndSet(ref SelectedEntity.ObjectAnim.FrameTime, value);
        }





        public GroundAnim SelectedEntity;
        private Loc dragDiff;

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
                        if (input.JustPressed(FrameInput.InputType.LeftMouse))
                            PlaceEntity(groundCoords);
                        else if (input.JustPressed(FrameInput.InputType.RightMouse))
                            RemoveEntityAt(groundCoords);
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

        public void PlaceEntity(Loc position)
        {
            GroundAnim placeableEntity = new GroundAnim(new ObjAnimData(SelectedEntity.ObjectAnim), position);

            DiagManager.Instance.DevEditor.GroundEditor.Edits.Apply(new GroundDecorationStateUndo(Layers.ChosenLayer));

            ZoneManager.Instance.CurrentGround.Decorations[Layers.ChosenLayer].Anims.Add(placeableEntity);
        }


        public void SelectEntity(GroundAnim ent)
        {
            if (ent != null)
            {
                DiagManager.Instance.DevEditor.GroundEditor.Edits.Apply(new GroundDecorationStateUndo(Layers.ChosenLayer));
                setEntity(ent);
            }
            else
                setEntity(new GroundAnim(new ObjAnimData(ObjectAnims[0], 1), Loc.Zero));
        }

        private void setEntity(GroundAnim ent)
        {
            SelectedEntity = ent;
            ChosenObjectAnim = ChosenObjectAnim;
            ChosenDirection = ChosenDirection;
            StartFrame = StartFrame;
            EndFrame = EndFrame;
            FrameLength = FrameLength;
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
