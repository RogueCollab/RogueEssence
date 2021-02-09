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
            string[] tile_names = DataManager.Instance.DataIndices[DataManager.DataType.Tile].GetLocalStringArray();
            for (int ii = 0; ii < tile_names.Length; ii++)
                TileTypes.Add(ii.ToString("D3") + ": " + tile_names[ii]);

            Owners = new ObservableCollection<string>();
            for (int ii = 0; ii < 3; ii++)
                Owners.Add(((EffectTile.TileOwner)ii).ToString());


            TileStates = new CollectionBoxViewModel();
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
            DataEditForm frmData = new DataEditForm();
            if (element == null)
                frmData.Title = "New State";
            else
                frmData.Title = element.ToString();

            //TODO: make this a member and reference it that way
            DataEditor.LoadClassControls(frmData.ControlPanel, "(TileStates) [" + index + "]", typeof(TileState), new object[0] { }, element, true);

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            frmData.SelectedOKEvent += async () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, "TileStates", typeof(TileState), new object[0] { }, true);

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
                    await MessageBox.Show(form.MapEditForm, "Cannot add duplicate states.", "Entry already exists.", MessageBox.MessageBoxButtons.Ok);
                }
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

        public void ProcessInput(InputManager input)
        {
            Loc tileCoords = DungeonEditScene.Instance.ScreenCoordsToMapCoords(input.MouseLoc);
            switch (TileMode)
            {
                case TileEditMode.Draw:
                    {
                        if (input[FrameInput.InputType.LeftMouse])
                            paintTile(tileCoords, getBrush());
                        else if (input[FrameInput.InputType.RightMouse])
                            paintTile(tileCoords, new EffectTile());
                    }
                    break;
                case TileEditMode.Rectangle:
                    {
                        if (input.JustPressed(FrameInput.InputType.LeftMouse))
                        {
                            DungeonEditScene.Instance.TileInProgress = getBrushAnim();
                            DungeonEditScene.Instance.RectInProgress = new Rect(tileCoords, Loc.Zero);
                        }
                        else if (input[FrameInput.InputType.LeftMouse])
                            DungeonEditScene.Instance.RectInProgress.Size = (tileCoords - DungeonEditScene.Instance.RectInProgress.Start);
                        else if (input.JustReleased(FrameInput.InputType.LeftMouse))
                        {
                            rectTile(DungeonEditScene.Instance.RectPreview(), getBrush());
                            DungeonEditScene.Instance.TileInProgress = null;
                        }
                        else if (input.JustPressed(FrameInput.InputType.RightMouse))
                        {
                            DungeonEditScene.Instance.TileInProgress = new ObjAnimData();
                            DungeonEditScene.Instance.RectInProgress = new Rect(tileCoords, Loc.Zero);
                        }
                        else if (input[FrameInput.InputType.RightMouse])
                            DungeonEditScene.Instance.RectInProgress.Size = (tileCoords - DungeonEditScene.Instance.RectInProgress.Start);
                        else if (input.JustReleased(FrameInput.InputType.RightMouse))
                        {
                            rectTile(DungeonEditScene.Instance.RectPreview(), new EffectTile());
                            DungeonEditScene.Instance.TileInProgress = null;
                        }
                    }
                    break;
                case TileEditMode.Fill:
                    {
                        if (input.JustReleased(FrameInput.InputType.LeftMouse))
                            fillTile(tileCoords, getBrush());
                        else if (input.JustReleased(FrameInput.InputType.RightMouse))
                            fillTile(tileCoords, new EffectTile());
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

        private ObjAnimData getBrushAnim()
        {
            TileData entry = DataManager.Instance.GetTile(ChosenTile);
            return entry.Anim;
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

        private void paintTile(Loc loc, EffectTile brush)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                return;

            ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].Effect = new EffectTile(brush, loc);
        }

        private void rectTile(Rect rect, EffectTile brush)
        {
            for (int xx = rect.X; xx < rect.End.X; xx++)
            {
                for (int yy = rect.Y; yy < rect.End.Y; yy++)
                {
                    Loc loc = new Loc(xx, yy);
                    if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                        continue;

                    ZoneManager.Instance.CurrentMap.Tiles[xx][yy].Effect = new EffectTile(brush, loc);
                }
            }
        }

        private void eyedropTile(Loc loc)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                return;

            EffectTile effectTile = ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].Effect;
            if (effectTile.ID > -1)
                setBrush(effectTile);
        }


        private void fillTile(Loc loc, EffectTile brush)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                return;

            EffectTile tile = ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].Effect;
            Rect bounds = new Rect(loc, Loc.One);
            RogueElements.Grid.FloodFill(new Rect(0, 0, ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height),
                    (Loc testLoc) =>
                    {
                        return tile.ID != ZoneManager.Instance.CurrentMap.Tiles[testLoc.X][testLoc.Y].Effect.ID;
                    },
                    (Loc testLoc) =>
                    {
                        return true;
                    },
                    (Loc testLoc) =>
                    {
                        bounds = Rect.FromPoints(new Loc(Math.Min(bounds.X, testLoc.X), Math.Min(bounds.Y, testLoc.Y)),
                            new Loc(Math.Max(bounds.End.X, testLoc.X + 1), Math.Max(bounds.End.Y, testLoc.Y + 1)));
                        ZoneManager.Instance.CurrentMap.Tiles[testLoc.X][testLoc.Y].Effect = new EffectTile(brush, testLoc);
                    },
                loc);

        }
    }
}
