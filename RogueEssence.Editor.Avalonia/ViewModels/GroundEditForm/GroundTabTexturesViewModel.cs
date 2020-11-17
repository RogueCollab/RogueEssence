using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class GroundTabTexturesViewModel : ViewModelBase
    {
        public GroundTabTexturesViewModel()
        {
            TileBrowser = new TileBrowserViewModel();
        }

        private int currentLayer;
        public int CurrentLayer
        {
            get { return currentLayer; }
            set { this.SetIfChanged(ref currentLayer, value); }
        }

        public TileBrowserViewModel TileBrowser { get; set; }

        public void LoadLayers()
        {

        }
    }
}
