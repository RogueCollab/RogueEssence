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
    public partial class TileBrowser : UserControl
    {
        public TileBrowser()
        {
            this.InitializeComponent();
        }
    
        public void picTileset_Click(object sender, PointerPressedEventArgs e)
        {
            PointerPoint pt = e.GetCurrentPoint((Visual)sender);
            TileBrowserViewModel vm = (TileBrowserViewModel)DataContext;
            
            Loc clickedLoc = new Loc((int)pt.Position.X / vm.TileSize, (int)pt.Position.Y / vm.TileSize);
            vm.SelectTile(clickedLoc, pt.Properties.IsRightButtonPressed, (e.KeyModifiers & KeyModifiers.Shift) != KeyModifiers.None);
        }
    }
}
