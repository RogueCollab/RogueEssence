using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueEssence.Dev.ViewModels;
using System;

namespace RogueEssence.Dev.Views
{
    public class ModConfigWindow : Window
    {
        public ModConfigWindow()
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




        public async void btnOK_Click(object sender, RoutedEventArgs e)
        {
            ModConfigViewModel vm = (ModConfigViewModel)DataContext;
            try
            {
                if (String.IsNullOrWhiteSpace(Text.Sanitize(vm.Name)))
                    throw new InvalidOperationException("Invalid Name");
                if (String.IsNullOrWhiteSpace(Text.Sanitize(vm.Namespace).ToLower()))
                    throw new InvalidOperationException("Invalid Namespace");

                Guid uuid = Guid.Parse(vm.UUID);
                if (uuid == Guid.Empty)
                    throw new InvalidOperationException("Invalid UUID");

                Version.Parse(vm.Version);

                if (vm.ChosenModType < 0 || vm.ChosenModType >= (int)PathMod.ModType.Count)
                    throw new InvalidOperationException("Invalid ModType");
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBox.Show(this, ex.Message, "Invalid Input", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            this.Close(true);
        }


        public void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close(false);
        }
    }
}
