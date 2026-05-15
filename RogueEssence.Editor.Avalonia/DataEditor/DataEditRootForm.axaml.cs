using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using RogueEssence;
using RogueEssence.Dev;
using Microsoft.Xna.Framework;
using Avalonia.Threading;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RogueEssence.Dev.Views
{
    public partial class DataEditRootForm : DataEditForm
    {
        public async void btnApply_Click(object sender, RoutedEventArgs e)
        {
            await SaveChildren();
            if (SelectedOKEvent != null)
                await SelectedOKEvent.Invoke();
        }

        public override async void Window_Closing(object sender, WindowClosingEventArgs e)
        {
            base.Window_Closing(sender, e);

            DevDataManager.SaveEditorSettings();
        }
    }
}
