using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using RogueEssence.Dev.ViewModels;

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
            if ((args.KeyModifiers & KeyModifiers.Shift) != KeyModifiers.None || (args.KeyModifiers & KeyModifiers.Control) != KeyModifiers.None)
            {
                DevTabScriptViewModel viewModel = (DevTabScriptViewModel)DataContext;

                switch (args.Key)
                {
                    case Key.Enter:
                        {
                            viewModel.ScriptLine = viewModel.ScriptLine + "\n";
                            break;
                        }
                    case Key.Up:
                        {
                            viewModel.ShiftHistory(-1);
                            break;
                        }
                    case Key.Down:
                        {
                            viewModel.ShiftHistory(1);
                            break;
                        }
                }
            }
            else if (args.Key == Key.Enter)
            {
                DevTabScriptViewModel viewModel = (DevTabScriptViewModel)DataContext;
                viewModel.SendScript();

            }
        }

    }
}
