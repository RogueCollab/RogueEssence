using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace RogueEssence.Dev.Views
{
    public partial class AnimLayerWindow : Window
    {
        public AnimLayerWindow()
        {
            this.InitializeComponent();
        }
        
        public void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close(true);
        }

        public void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close(false);
        }
    }
}
