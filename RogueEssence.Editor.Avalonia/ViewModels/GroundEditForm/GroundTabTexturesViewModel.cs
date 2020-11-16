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

        public TileBrowserViewModel TileBrowser { get; set; }
    }
}
