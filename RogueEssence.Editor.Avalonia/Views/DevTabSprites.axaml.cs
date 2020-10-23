using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RogueEssence.Dev.Views
{
    public class DevTabSprites : UserControl
    {
        public DevTabSprites()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
