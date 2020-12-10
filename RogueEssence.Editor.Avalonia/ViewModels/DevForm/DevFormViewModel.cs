using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Data;

namespace RogueEssence.Dev.ViewModels
{
    public class DevFormViewModel : ViewModelBase
    {
        public DevFormViewModel()
        {
            Game = new DevTabGameViewModel();
            Player = new DevTabPlayerViewModel();
            Data = new DevTabDataViewModel();
            Travel = new DevTabTravelViewModel();
            Sprites = new DevTabSpritesViewModel();
            Script = new DevTabScriptViewModel();
            Mods = new DevTabModsViewModel();
        }

        public DevTabGameViewModel Game { get; set; }
        public DevTabPlayerViewModel Player { get; set; }
        public DevTabDataViewModel Data { get; set; }
        public DevTabTravelViewModel Travel { get; set; }
        public DevTabSpritesViewModel Sprites { get; set; }
        public DevTabScriptViewModel Script { get; set; }
        public DevTabModsViewModel Mods { get; set; }





    }
}
