﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace RogueEssence.Dev.Views
{
    public partial class AnimChoiceWindow : Window
    {
        public AnimChoiceWindow()
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
