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
            if (!String.IsNullOrEmpty(autoTile.AutoTileset))
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
