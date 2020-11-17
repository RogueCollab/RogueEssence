using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using RogueEssence.Dev;

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

        public void SetupLayerVisibility()
        {

            //ShowDataLayer = false;
        }
    }
}
