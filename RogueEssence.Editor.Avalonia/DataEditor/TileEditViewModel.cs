using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using System;

namespace RogueEssence.Dev.ViewModels
{
    public class TileEditViewModel : ViewModelBase
    {
        public event Action SelectedOKEvent;
        public event Action SelectedCancelEvent;

        public TileEditViewModel()
        {
            TileBrowser = new TileBrowserViewModel();
            TileBrowser.CanMultiSelect = false;
            AutotileBrowser = new AutotileBrowserViewModel();
        }

        public TileBrowserViewModel TileBrowser { get; set; }
        public AutotileBrowserViewModel AutotileBrowser { get; set; }

        private string name;
        public string Name
        {
            get => name;
            set => this.SetIfChanged(ref name, value);
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



        public AutoTile GetTile()
        {
            TileBrush brush;
            if (tabIndex == 0)
                brush = TileBrowser.GetBrush();
            else
                brush = AutotileBrowser.GetBrush();
            return brush.GetSanitizedTile();
        }

        public void LoadTile(AutoTile autoTile)
        {
            if (autoTile.AutoTileset > -1)
            {
                //switch to autotile tab
                AutotileBrowser.SetBrush(autoTile);
            }
            else
            {
                TileLayer layer = (autoTile.Layers.Count > 0) ? autoTile.Layers[0] : new TileLayer();
                TileBrowser.SetBrush(layer);
            }
        }


        private void fillTile(Loc loc, TileBrush brush)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                return;

            AutoTile tile = ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].FloorTile.Copy();
            Rect bounds = new Rect(loc, Loc.One);
            Grid.FloodFill(new Rect(0, 0, ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height),
                    (Loc testLoc) =>
                    {
                        return !tile.Equals(ZoneManager.Instance.CurrentMap.Tiles[testLoc.X][testLoc.Y].FloorTile);
                    },
                    (Loc testLoc) =>
                    {
                        return true;
                    },
                    (Loc testLoc) =>
                    {
                        bounds = Rect.FromPoints(new Loc(Math.Min(bounds.X, testLoc.X), Math.Min(bounds.Y, testLoc.Y)),
                            new Loc(Math.Max(bounds.End.X, testLoc.X+1), Math.Max(bounds.End.Y, testLoc.Y + 1)));
                        ZoneManager.Instance.CurrentMap.Tiles[testLoc.X][testLoc.Y].FloorTile = brush.GetSanitizedTile();
                    },
                loc);

            //now recompute all autotiles within the rectangle
            bounds.Inflate(1, 1);
            ZoneManager.Instance.CurrentMap.CalculateAutotiles(bounds.Start, bounds.Size);
        }

        public void btnOK_Click()
        {
            SelectedOKEvent?.Invoke();
        }

        public void btnCancel_Click()
        {
            SelectedCancelEvent?.Invoke();
        }

    }
}
