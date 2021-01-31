using ReactiveUI;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System;
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
                this.SetIfChanged(ref chosenTile, value);
            }
        }

        private bool isRevealed;
        public bool IsRevealed
        {
            get { return isRevealed; }
            set
            {
                this.SetIfChanged(ref isRevealed, value);
            }
        }

        private bool isDanger;
        public bool IsDanger
        {
            get { return isDanger; }
            set
            {
                this.SetIfChanged(ref isDanger, value);
            }
        }

        public ObservableCollection<string> Owners { get; }

        private int chosenOwner;
        public int ChosenOwner
        {
            get { return chosenOwner; }
            set
            {
                this.SetIfChanged(ref chosenOwner, value);
            }
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
            return brush;
        }

        private void setBrush(EffectTile brush)
        {
            ChosenTile = brush.ID;
            ChosenOwner = (int)brush.Owner;
            IsDanger = brush.Danger;
            IsRevealed = brush.Revealed;
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
            Grid.FloodFill(new Rect(0, 0, ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height),
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
