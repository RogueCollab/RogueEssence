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
    public class MapLayerViewModel : ViewModelBase
    {
        public MapLayerViewModel(MapLayer layer)
        {
            name = layer.Name;
            front = layer.Layer == Content.DrawLayer.Top;
            back = layer.Layer == Content.DrawLayer.Under;
            mid = !front && !back;
        }

        private string name;
        public string Name
        {
            get => name;
            set => this.SetIfChanged(ref name, value);
        }

        private bool front;
        public bool Front
        {
            get { return front; }
            set
            {
                this.SetIfChanged(ref front, value);
            }
        }

        private bool mid;
        public bool Mid
        {
            get { return mid; }
            set
            {
                this.SetIfChanged(ref mid, value);
            }
        }

        private bool back;
        public bool Back
        {
            get { return back; }
            set
            {
                this.SetIfChanged(ref back, value);
            }
        }

    }
}
