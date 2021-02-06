using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RogueEssence.Dev.Views
{
    public class MapTabSpawns : UserControl
    {
        public MapTabSpawns()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
