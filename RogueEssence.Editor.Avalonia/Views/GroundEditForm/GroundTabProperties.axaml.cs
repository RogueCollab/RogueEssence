using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RogueEssence.Dev.Views
{
    public class GroundTabProperties : UserControl
    {
        public GroundTabProperties()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
