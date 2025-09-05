using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using Avalonia.Input;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public partial class ClassBox : UserControl
    {
        public ClassBox()
        {
            this.InitializeComponent();
            Button button = this.FindControl<Button>("ClassBoxEditButton");
            button.AddHandler(PointerReleasedEvent, ClassBoxEditButton_OnPointerReleased, RoutingStrategies.Tunnel);
        }
        
        private void ClassBoxEditButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            ClassBoxViewModel vm = (ClassBoxViewModel) DataContext;
            vm.btnEdit_Click(advancedEdit);
        }
    }
}
