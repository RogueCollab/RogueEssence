using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace RogueEssence.Dev.Views
{
    public class EntityLayerWindow : Window
    {
        public EntityLayerWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
