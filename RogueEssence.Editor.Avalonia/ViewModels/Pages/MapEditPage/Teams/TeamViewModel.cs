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
    public class TeamViewModel : ViewModelBase
    {
        public TeamViewModel(Team team)
        {
            //name = layer.Name;
            //front = layer.Layer == Content.DrawLayer.Top;
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

    }
}
