using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Data;
using ReactiveUI;

namespace RogueEssence.Dev.ViewModels
{
    public class MapRetileViewModel : ViewModelBase
    {
        public MapRetileViewModel(int origSize)
        {
            TileSize = origSize;
        }

        private int tileSize;
        public int TileSize
        {
            get { return tileSize; }
            set
            {
                this.SetIfChanged(ref tileSize, value);
                AllowConfirm = (tileSize % 8 == 0);
            }
        }

        private bool allowConfirm;
        public bool AllowConfirm
        {
            get { return allowConfirm; }
            set
            {
                this.SetIfChanged(ref allowConfirm, value);
            }
        }

    }
}
