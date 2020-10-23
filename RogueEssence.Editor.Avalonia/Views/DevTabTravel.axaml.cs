using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RogueEssence.Dev.Views
{
    public class DevTabTravel : UserControl
    {
        public DevTabTravel()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        public void cbZones_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void cbStructure_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
