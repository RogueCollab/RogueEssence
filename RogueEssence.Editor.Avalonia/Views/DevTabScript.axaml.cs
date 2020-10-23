using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;

namespace RogueEssence.Dev.Views
{
    public class DevTabScript : UserControl
    {
        public DevTabScript()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        public void txtScriptInput_KeyDown(object sender, KeyEventArgs args)
        {

        }

    }
}
