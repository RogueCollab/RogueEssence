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
    public class EntityBrowser : UserControl
    {
        public EntityBrowser()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
