using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class GroundTabEntitiesViewModel : ViewModelBase
    {
        public delegate void EntityOp(GroundEntity ent);

        public GroundTabEntitiesViewModel()
        {
            Layers = new EntityLayerBoxViewModel(DiagManager.Instance.DevEditor.GroundEditor.Edits);
            Layers.SelectedLayerChanged += Layers_SelectedLayerChanged;
            EntBrowser = new EntityBrowserViewModel();
        }

        private EntEditMode entMode;
        public EntEditMode EntMode
        {
            get { return entMode; }
            set
            {
                this.SetIfChanged(ref entMode, value);
                EntBrowser.AllowEntTypes = (entMode == EntEditMode.PlaceEntity);
            }
        }

        public ILayerBoxViewModel Layers { get; set; }

        public EntityBrowserViewModel EntBrowser { get; set; }


        private Loc dragDiff;

        public void ProcessUndo()
        {
            if (EntMode == EntEditMode.SelectEntity)
                SelectEntity(null);
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
                            dragDiff = groundCoords - EntBrowser.SelectedEntity.MapLoc;
                        }
                        else if (input[FrameInput.InputType.LeftMouse])
                            MoveEntity(groundCoords - dragDiff);
                        else if (input.Direction != input.PrevDirection)
                        {
                            Loc diff = input.Direction.GetLoc();
                            if (snapGrid)
                                diff *= 8;
                            MoveEntity(EntBrowser.SelectedEntity.MapLoc + diff);
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

        public void RemoveEntity(GroundEntity ent)
        {
            if (ent == null)
                return;

            DiagManager.Instance.DevEditor.GroundEditor.Edits.Apply(new GroundEntityStateUndo(0));

            if (ent.GetEntityType() == GroundEntity.EEntTypes.Character)
                ZoneManager.Instance.CurrentGround.RemoveMapChar((GroundChar)ent);
            else if (ent.GetEntityType() == GroundEntity.EEntTypes.Object)
                ZoneManager.Instance.CurrentGround.RemoveObject((GroundObject)ent);
            else if (ent.GetEntityType() == GroundEntity.EEntTypes.Marker)
                ZoneManager.Instance.CurrentGround.RemoveMarker((GroundMarker)ent);
            else if (ent.GetEntityType() == GroundEntity.EEntTypes.Spawner)
                ZoneManager.Instance.CurrentGround.RemoveSpawner((GroundSpawner)ent);
        }

        public void PlaceEntity(Loc position)
        {
            GroundEntity placeableEntity = EntBrowser.CreateEntity();

            if (placeableEntity == null)
                return;

            placeableEntity.Position = position;


            placeableEntity.EntName = ZoneManager.Instance.CurrentGround.FindNonConflictingName(placeableEntity.EntName);
            placeableEntity.ReloadEvents();

            DiagManager.Instance.DevEditor.GroundEditor.Edits.Apply(new GroundEntityStateUndo(0));

            if (placeableEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                ZoneManager.Instance.CurrentGround.AddMapChar((GroundChar)placeableEntity);
            else if (placeableEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                ZoneManager.Instance.CurrentGround.AddObject((GroundObject)placeableEntity);
            else if (placeableEntity.GetEntityType() == GroundEntity.EEntTypes.Marker)
                ZoneManager.Instance.CurrentGround.AddMarker((GroundMarker)placeableEntity);
            else if (placeableEntity.GetEntityType() == GroundEntity.EEntTypes.Spawner)
                ZoneManager.Instance.CurrentGround.AddSpawner((GroundSpawner)placeableEntity);

        }

        /// <summary>
        /// Select the entity at that position and displays its data for editing
        /// </summary>
        /// <param name="position"></param>
        public void SelectEntityAt(Loc position)
        {
            OperateOnEntityAt(position, SelectEntity);
        }


        public void SelectEntity(GroundEntity ent)
        {
            if (ent != null)
                DiagManager.Instance.DevEditor.GroundEditor.Edits.Apply(new GroundEntityStateUndo(0));

            EntBrowser.SelectEntity(ent);
        }

        public void OperateOnEntityAt(Loc position, EntityOp op)
        {
            List<GroundEntity> found = ZoneManager.Instance.CurrentGround.FindEntitiesAtPosition(position);
            if (found.Count > 0)
                op(found.First());
            else
                op(null);
        }

        private void MoveEntity(Loc loc)
        {
            if (EntBrowser.SelectedEntity != null)
                EntBrowser.SelectedEntity.SetMapLoc(loc);
        }

        public void Layers_SelectedLayerChanged()
        {
            if (EntMode == EntEditMode.SelectEntity)
                EntBrowser.SelectEntity(null);
        }

    }

    public class GroundEntityStateUndo : StateUndo<EntityLayer>
    {
        private int layer;
        public GroundEntityStateUndo(int layer)
        {
            this.layer = layer;
        }

        public override EntityLayer GetState()
        {
            ZoneManager.Instance.CurrentGround.PreSaveEntLayer(layer);
            return ZoneManager.Instance.CurrentGround.Entities[layer];
        }

        public override void SetState(EntityLayer state)
        {
            ZoneManager.Instance.CurrentGround.Entities[layer] = state;
            ZoneManager.Instance.CurrentGround.ReloadEntLayer(layer);
        }
    }
}
