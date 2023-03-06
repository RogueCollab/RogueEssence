using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using RogueElements;
using RogueEssence.Dungeon;
using ReactiveUI;
using RogueEssence.Content;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{

    public class TeamMemberNodeViewModel : ViewModelBase
    {

        private string name;
        public string Name
        {
            get => name;
            set => this.SetIfChanged(ref name, value);
        }

        public TeamMemberNodeViewModel(string name)
        {
            this.name = name;
        }

    }

    public class TeamNodeViewModel : ViewModelBase
    {

        private string name;
        public string Name
        {
            get => name;
            set => this.SetIfChanged(ref name, value);
        }

        public ObservableCollection<TeamMemberNodeViewModel> Nodes { get; }

        public TeamNodeViewModel(string name)
        {
            this.name = name;
            Nodes = new ObservableCollection<TeamMemberNodeViewModel>();
        }

    }

    public class TeamBoxViewModel : ViewModelBase
    {
        protected UndoStack edits { get; }

        public TeamBoxViewModel(UndoStack stack)
        {
            this.edits = stack;
            Teams = new WrappedObservableCollection<Team>();
        }

        public WrappedObservableCollection<Team> Teams { get; }

        public event Action SelectedTeamChanged;


        private Team chosenTeam;
        public Team ChosenTeam
        {
            get => chosenTeam;
            set
            {
                this.SetIfChanged(ref chosenTeam, value);
                SelectedTeamChanged?.Invoke();
            }
        }

        public async Task EditTeam()
        {
            TeamWindow window = new TeamWindow();
            TeamViewModel vm = new TeamViewModel(chosenTeam);
            window.DataContext = vm;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            bool result = await window.ShowDialog<bool>(form.MapEditForm);

            //lock (GameBase.lockObj)
            //{
            //    if (result)
            //    {
            //        AnimLayer newLayer = new AnimLayer(vm.Name);
            //        AnimLayer oldLayer = Layers[ChosenLayer];
            //        newLayer.Layer = vm.Front ? DrawLayer.Top : DrawLayer.Bottom;
            //        newLayer.Visible = oldLayer.Visible;
            //        newLayer.Anims = oldLayer.Anims;

            //        edits.Apply(new GroundDecorationStateUndo(ChosenLayer));

            //        Layers[ChosenLayer] = newLayer;
            //    }
            //}
        }

        //public void AddTeam()
        //{
        //    lock (GameBase.lockObj)
        //    {
        //        int curLayer = chosenLayer;
        //        T layer = GetNewLayer();

        //        edits.Apply(new AddLayerUndo<T>(Layers, chosenLayer + 1, layer, false));
        //        ChosenLayer = curLayer + 1;
        //    }
        //}

        //public void DeleteTeam()
        //{
        //    lock (GameBase.lockObj)
        //    {
        //        if (Layers.Count > 1)
        //        {
        //            int curLayer = chosenLayer;

        //            edits.Apply(new AddLayerUndo<T>(Layers, chosenLayer, Layers[chosenLayer], true));

        //            ChosenLayer = Math.Min(curLayer, Layers.Count - 1);
        //        }
        //    }
        //}

        //public void MoveTeamUp()
        //{
        //    lock (GameBase.lockObj)
        //    {
        //        if (chosenLayer > 0)
        //        {
        //            int insertLayer = chosenLayer - 1;
        //            edits.Apply(new MoveLayerUndo<T>(Layers, insertLayer));
        //            ChosenLayer = insertLayer;
        //        }
        //    }
        //}

        //public void MoveTeamDown()
        //{
        //    lock (GameBase.lockObj)
        //    {
        //        if (chosenLayer < Layers.Count - 1)
        //        {
        //            int insertLayer = chosenLayer + 1;
        //            edits.Apply(new MoveLayerUndo<T>(Layers, chosenLayer));
        //            ChosenLayer = insertLayer;
        //        }
        //    }
        //}

        public void LoadTeams()
        {
            if (Design.IsDesignMode)
                return;

            Teams.LoadModels(ZoneManager.Instance.CurrentMap.MapTeams);
        }
    }

}
