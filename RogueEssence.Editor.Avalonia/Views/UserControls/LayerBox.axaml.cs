using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.Views
{
    public class LayerBox : UserControl
    {
        public LayerBox()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async void lbxItems_DoubleClick(object sender, RoutedEventArgs e)
        {
            ILayerBoxViewModel viewModel = (ILayerBoxViewModel)DataContext;
            if (viewModel == null)
                return;

            await viewModel.EditLayer();
        }
    }
}
