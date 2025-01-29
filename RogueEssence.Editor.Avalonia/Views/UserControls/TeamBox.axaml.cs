using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dungeon;
using Avalonia.Input;

namespace RogueEssence.Dev.Views
{
    public partial class TeamBox : UserControl
    {
        public TeamBox()
        {
            this.InitializeComponent();
        }
        
        bool doubleclick;
        public void doubleClickStart(object sender, RoutedEventArgs e)
        {
            doubleclick = true;
        }

        public async void lbxItems_DoubleClick(object sender, PointerReleasedEventArgs e)
        {
            if (!doubleclick)
                return;
            doubleclick = false;

            TeamBoxViewModel viewModel = (TeamBoxViewModel)DataContext;
            if (viewModel == null)
                return;

            await viewModel.EditTeam();
        }
    }
}
