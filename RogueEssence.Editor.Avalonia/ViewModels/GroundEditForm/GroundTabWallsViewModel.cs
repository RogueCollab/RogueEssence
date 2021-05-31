using ReactiveUI;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using System.Collections.Generic;

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
            ShowWalls = ShowWalls;
        }

        public void ProcessInput(InputManager input)
        {
            bool inWindow = Collision.InBounds(GraphicsManager.WindowWidth, GraphicsManager.WindowHeight, input.MouseLoc);

            Loc tileCoords = GroundEditScene.Instance.ScreenCoordsToBlockCoords(input.MouseLoc);
            switch (BlockMode)
            {
                case TileEditMode.Draw:
                    {
                        CanvasStroke<bool>.ProcessCanvasInput(input, tileCoords, inWindow,
                            () => new DrawStroke<bool>(tileCoords, true),
                            () => new DrawStroke<bool>(tileCoords, false),
                            paintStroke, ref GroundEditScene.Instance.BlockInProgress);
                    }
                    break;
                case TileEditMode.Rectangle:
                    {
                        CanvasStroke<bool>.ProcessCanvasInput(input, tileCoords, inWindow,
                            () => new RectStroke<bool>(tileCoords, true),
                            () => new RectStroke<bool>(tileCoords, false),
                            paintStroke, ref GroundEditScene.Instance.BlockInProgress);
                    }
                    break;
                case TileEditMode.Fill:
                    {
                        CanvasStroke<bool>.ProcessCanvasInput(input, tileCoords, inWindow,
                            () => new FillStroke<bool>(tileCoords, true),
                            () => new FillStroke<bool>(tileCoords, false),
                            fillStroke, ref GroundEditScene.Instance.BlockInProgress);
                    }
                    break;
            }
        }


        private void paintStroke(CanvasStroke<bool> stroke)
        {
            foreach (Loc loc in stroke.GetLocs())
            {
                if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.TexWidth, ZoneManager.Instance.CurrentGround.TexHeight, loc))
                    continue;

                ZoneManager.Instance.CurrentGround.SetObstacle(loc.X, loc.Y, stroke.GetBrush(loc) ? 1u : 0u);
            }
        }

        private void fillStroke(CanvasStroke<bool> stroke)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.TexWidth, ZoneManager.Instance.CurrentGround.TexHeight, stroke.CoveredRect.Start))
                return;

            uint tile = ZoneManager.Instance.CurrentGround.GetObstacle(stroke.CoveredRect.Start.X, stroke.CoveredRect.Start.Y);

            bool blocked = stroke.GetBrush(stroke.CoveredRect.Start);
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
                        ZoneManager.Instance.CurrentGround.SetObstacle(testLoc.X, testLoc.Y, blocked ? 1u : 0u);
                    },
                stroke.CoveredRect.Start);
        }
    }
}
