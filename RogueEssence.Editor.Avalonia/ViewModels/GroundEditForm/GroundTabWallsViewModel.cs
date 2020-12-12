using ReactiveUI;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.ViewModels
{
    public class GroundTabWallsViewModel : ViewModelBase
    {
        public GroundTabWallsViewModel()
        {

        }

        private TileEditMode blockMode;
        public TileEditMode BlockMode
        {
            get { return blockMode; }
            set
            {
                this.SetIfChanged(ref blockMode, value);
            }
        }

        public bool ShowWalls
        {
            get
            {
                return GroundEditScene.Instance.ShowWalls;
            }
            set
            {
                GroundEditScene.Instance.ShowWalls = value;
                this.RaisePropertyChanged();
            }
        }

        public void SetupLayerVisibility()
        {

            //ShowDataLayer = false;
        }

        public void ProcessInput(InputManager input)
        {
            Loc tileCoords = GroundEditScene.Instance.ScreenCoordsToBlockCoords(input.MouseLoc);
            switch (BlockMode)
            {
                case TileEditMode.Draw:
                    {
                        if (input[FrameInput.InputType.LeftMouse])
                            paintBlockTile(tileCoords, true);
                        else if (input[FrameInput.InputType.RightMouse])
                            paintBlockTile(tileCoords, false);
                    }
                    break;
                case TileEditMode.Rectangle:
                    {
                        Loc groundCoords = GroundEditScene.Instance.ScreenCoordsToGroundCoords(input.MouseLoc);
                        if (input.JustPressed(FrameInput.InputType.LeftMouse))
                        {
                            GroundEditScene.Instance.BlockInProgress = true;
                            GroundEditScene.Instance.RectInProgress = new Rect(groundCoords, Loc.Zero);
                        }
                        else if (input[FrameInput.InputType.LeftMouse])
                            GroundEditScene.Instance.RectInProgress.Size = (groundCoords - GroundEditScene.Instance.RectInProgress.Start);
                        else if (input.JustReleased(FrameInput.InputType.LeftMouse))
                        {
                            rectBlockTile(GroundEditScene.Instance.BlockRectPreview(), true);
                            GroundEditScene.Instance.BlockInProgress = null;
                        }
                        else if (input.JustPressed(FrameInput.InputType.RightMouse))
                        {
                            GroundEditScene.Instance.BlockInProgress = false;
                            GroundEditScene.Instance.RectInProgress = new Rect(groundCoords, Loc.Zero);
                        }
                        else if (input[FrameInput.InputType.RightMouse])
                            GroundEditScene.Instance.RectInProgress.Size = (groundCoords - GroundEditScene.Instance.RectInProgress.Start);
                        else if (input.JustReleased(FrameInput.InputType.RightMouse))
                        {
                            rectBlockTile(GroundEditScene.Instance.BlockRectPreview(), false);
                            GroundEditScene.Instance.BlockInProgress = null;
                        }
                    }
                    break;
                case TileEditMode.Fill:
                    {
                        if (input.JustReleased(FrameInput.InputType.LeftMouse))
                            fillBlockTile(tileCoords, true);
                        else if (input.JustReleased(FrameInput.InputType.RightMouse))
                            fillBlockTile(tileCoords, false);
                    }
                    break;
            }
        }


        private void paintBlockTile(Loc loc, bool block)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.TexWidth, ZoneManager.Instance.CurrentGround.TexHeight, loc))
                return;

            ZoneManager.Instance.CurrentGround.SetObstacle(loc.X, loc.Y, block ? 1u : 0u);
        }

        private void rectBlockTile(Rect rect, bool block)
        {
            for (int xx = rect.X; xx < rect.End.X; xx++)
            {
                for (int yy = rect.Y; yy < rect.End.Y; yy++)
                {
                    if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.TexWidth, ZoneManager.Instance.CurrentGround.TexHeight, new Loc(xx, yy)))
                        continue;

                    ZoneManager.Instance.CurrentGround.SetObstacle(xx, yy, block ? 1u : 0u);
                }
            }
        }

        private void fillBlockTile(Loc loc, bool block)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.TexWidth, ZoneManager.Instance.CurrentGround.TexHeight, loc))
                return;

            uint tile = ZoneManager.Instance.CurrentGround.GetObstacle(loc.X, loc.Y);

            Grid.FloodFill(new Rect(0, 0, ZoneManager.Instance.CurrentGround.TexWidth, ZoneManager.Instance.CurrentGround.TexHeight),
                    (Loc testLoc) =>
                    {
                        return tile != ZoneManager.Instance.CurrentGround.GetObstacle(testLoc.X, testLoc.Y);
                    },
                    (Loc testLoc) =>
                    {
                        return true;
                    },
                    (Loc testLoc) =>
                    {
                        ZoneManager.Instance.CurrentGround.SetObstacle(testLoc.X, testLoc.Y, block ? 1u : 0u);
                    },
                loc);
        }
    }
}
