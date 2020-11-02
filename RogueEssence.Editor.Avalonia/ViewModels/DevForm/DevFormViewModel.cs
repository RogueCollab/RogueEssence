using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class DevFormViewModel : ViewModelBase
    {
        public DevFormViewModel()
        {
            Game = new DevTabGameViewModel();
            Player = new DevTabPlayerViewModel();
            Data = new DevTabDataViewModel();
            Sprites = new DevTabSpritesViewModel();
            Script = new DevTabScriptViewModel();
            Travel = new DevTabTravelViewModel();

        }

        public DevTabGameViewModel Game { get; set; }
        public DevTabPlayerViewModel Player { get; set; }
        public DevTabDataViewModel Data { get; set; }
        public DevTabSpritesViewModel Sprites { get; set; }
        public DevTabScriptViewModel Script { get; set; }
        public DevTabTravelViewModel Travel { get; set; }
    }
}
