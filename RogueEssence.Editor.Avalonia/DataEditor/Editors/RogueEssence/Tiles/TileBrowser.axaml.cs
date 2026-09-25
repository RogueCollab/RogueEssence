using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using RogueElements;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public class TileBrowser : UserControl
    {
        public TileBrowser()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        public void picTileset_Click(object sender, PointerPressedEventArgs e)
        {
            PointerPoint pt = e.GetCurrentPoint((IVisual)sender);
            TileBrowserViewModel vm = (TileBrowserViewModel)DataContext;
            
            double scaledTileSize = vm.TileSize * vm.ZoomPercent / 100.0;
            Loc clickedLoc = new Loc((int)(pt.Position.X / scaledTileSize), (int)(pt.Position.Y / scaledTileSize));
            vm.SelectTile(clickedLoc, pt.Properties.IsRightButtonPressed, (e.KeyModifiers & KeyModifiers.Shift) != KeyModifiers.None);
        }
    }
}
