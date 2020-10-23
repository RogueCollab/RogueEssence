using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RogueEssence.Dev.Views
{
    public class DevTabData : UserControl
    {
        public DevTabData()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
