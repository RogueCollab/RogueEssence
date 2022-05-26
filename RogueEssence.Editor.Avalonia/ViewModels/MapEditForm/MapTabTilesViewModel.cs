using Avalonia.Controls;
using ReactiveUI;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.Views;
using RogueEssence.Dungeon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RogueEssence.Dev.ViewModels
{
    public class MapTabTilesViewModel : ViewModelBase
    {

        public MapTabTilesViewModel()
        {
            TileTypes = new ObservableCollection<string>();
            string[] tile_names = DataManager.Instance.DataIndices[DataManager.DataType.Tile].GetLocalStringArray(true);
            for (int ii = 0; ii < tile_names.Length; ii++)
                TileTypes.Add(ii.ToString("D3") + ": " + tile_names[ii]);

            Owners = new ObservableCollection<string>();
            for (int ii = 0; ii < 3; ii++)
                Owners.Add(((EffectTile.TileOwner)ii).ToString());


            TileStates = new CollectionBoxViewModel(new StringConv(typeof(TileState), new object[0]));
            TileStates.OnEditItem += TileStates_EditItem;
        }

        private TileEditMode tileMode;
        public TileEditMode TileMode
        {
            get { return tileMode; }
            set
            {
                this.SetIfChanged(ref tileMode, value);
            }
        }


        public ObservableCollection<string> TileTypes { get; }

        private int chosenTile;
        public int ChosenTile
        {
            get { return chosenTile; }
            set
            {
                this.RaiseAndSet(ref chosenTile, value);
            }
        }

        private bool isRevealed;
        public bool IsRevealed
        {
            get { return isRevealed; }
            set
            {
                this.RaiseAndSet(ref isRevealed, value);
            }
        }

        private bool isDanger;
        public bool IsDanger
        {
            get { return isDanger; }
            set
            {
                this.RaiseAndSet(ref isDanger, value);
            }
        }

        public ObservableCollection<string> Owners { get; }

        private int chosenOwner;
        public int ChosenOwner
        {
            get { return chosenOwner; }
            set
            {
                this.RaiseAndSet(ref chosenOwner, value);
            }
        }

        public CollectionBoxViewModel TileStates { get; set; }

        public void TileStates_EditItem(int index, object element, CollectionBoxViewModel.EditElementOp op)
        {
            string elementName = "TileStates[" + index + "]";
            DataEditForm frmData = new DataEditRootForm();
            frmData.Title = DataEditor.GetWindowTitle("Tile", elementName, element, typeof(TileState), new object[0]);

            //TODO: make this a member and reference it that way
            DataEditor.LoadClassControls(frmData.ControlPanel, "Tile", elementName, typeof(TileState), new object[0], element, true, new Type[0]);

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            frmData.SelectedOKEvent += async () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, typeof(TileState), new object[0], true, new Type[0]);

                bool itemExists = false;

                List<object> states = (List<object>)TileStates.GetList(typeof(List<object>));
                for (int ii = 0; ii < states.Count; ii++)
                {
                    if (ii != index)
                    {
                        if (states[ii].GetType() == element.GetType())
                            itemExists = true;
                    }
                }

                if (itemExists)
                {
                    await MessageBox.Show(frmData, "Cannot add duplicate states.", "Entry already exists.", MessageBox.MessageBoxButtons.Ok);
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

        public void ProcessInput(InputManager input)
        {
            bool inWindow = Collision.InBounds(GraphicsManager.WindowWidth, GraphicsManager.WindowHeight, input.MouseLoc);

            Loc tileCoords = DungeonEditScene.Instance.ScreenCoordsToMapCoords(input.MouseLoc);
            switch (TileMode)
            {
                case TileEditMode.Draw:
                    {
                        EffectTile brush = getBrush();
                        CanvasStroke<EffectTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                            () => new DrawStroke<EffectTile>(tileCoords, brush),
                            () => new DrawStroke<EffectTile>(tileCoords, new EffectTile()),
                            paintStroke, ref DungeonEditScene.Instance.TileInProgress);
                    }
                    break;
                case TileEditMode.Rectangle:
                    {
                        EffectTile brush = getBrush();
                        CanvasStroke<EffectTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                            () => new RectStroke<EffectTile>(tileCoords, brush),
                            () => new RectStroke<EffectTile>(tileCoords, new EffectTile()),
                            paintStroke, ref DungeonEditScene.Instance.TileInProgress);
                    }
                    break;
                case TileEditMode.Fill:
                    {
                        EffectTile brush = getBrush();
                        CanvasStroke<EffectTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                            () => new FillStroke<EffectTile>(tileCoords, brush),
                            () => new FillStroke<EffectTile>(tileCoords, new EffectTile()),
                            fillStroke, ref DungeonEditScene.Instance.TileInProgress);
                    }
                    break;
                case TileEditMode.Eyedrop:
                    {
                        if (input[FrameInput.InputType.LeftMouse])
                            eyedropTile(tileCoords);
                    }
                    break;
            }

        }

        private EffectTile getBrush()
        {
            EffectTile brush = new EffectTile(ChosenTile, IsRevealed);
            brush.Danger = IsDanger;
            brush.Owner = (EffectTile.TileOwner)ChosenOwner;

            List<TileState> states = TileStates.GetList<List<TileState>>();
            for (int ii = 0; ii < states.Count; ii++)
                brush.TileStates.Set(states[ii]);
            return brush;
        }

        private void setBrush(EffectTile brush)
        {
            ChosenTile = brush.ID;
            ChosenOwner = (int)brush.Owner;
            IsDanger = brush.Danger;
            IsRevealed = brush.Revealed;

            List<TileState> states = new List<TileState>();
            foreach (TileState state in brush.TileStates)
                states.Add(state);
            TileStates.LoadFromList(states);
        }


        private void paintStroke(CanvasStroke<EffectTile> stroke)
        {
            Dictionary<Loc, EffectTile> brush = new Dictionary<Loc, EffectTile>();
            foreach (Loc loc in stroke.GetLocs())
            {
                if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                    continue;

                brush[loc] = new EffectTile(stroke.GetBrush(loc), loc);
            }

            DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new DrawTileUndo(brush));
        }

        private void eyedropTile(Loc loc)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                return;

            EffectTile effectTile = ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].Effect;
            if (effectTile.ID > -1)
                setBrush(effectTile);
        }



        private void fillStroke(CanvasStroke<EffectTile> stroke)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, stroke.CoveredRect.Start))
                return;

            EffectTile tile = ZoneManager.Instance.CurrentMap.Tiles[stroke.CoveredRect.Start.X][stroke.CoveredRect.Start.Y].Effect;

            Dictionary<Loc, EffectTile> brush = new Dictionary<Loc, EffectTile>();
            EffectTile brushTile = stroke.GetBrush(stroke.CoveredRect.Start);
            RogueElements.Grid.FloodFill(new Rect(0, 0, ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height),
                    (Loc testLoc) =>
                    {
                        if (brush.ContainsKey(testLoc))
                            return true;
                        return tile.ID != ZoneManager.Instance.CurrentMap.Tiles[testLoc.X][testLoc.Y].Effect.ID;
                    },
                    (Loc testLoc) =>
                    {
                        return true;
                    },
                    (Loc testLoc) =>
                    {
                        brush[testLoc] = new EffectTile(brushTile, testLoc);
                    },
                stroke.CoveredRect.Start);

            DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new DrawTileUndo(brush));
        }
    }

    public class DrawTileUndo : DrawUndo<EffectTile>
    {
        public DrawTileUndo(Dictionary<Loc, EffectTile> brush) : base(brush)
        {
        }

        protected override EffectTile GetValue(Loc loc)
        {
            return ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].Effect;
        }
        protected override void SetValue(Loc loc, EffectTile val)
        {
            ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].Effect = val;
        }
    }
}
