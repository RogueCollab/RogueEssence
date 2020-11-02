using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RogueEssence.Dev.Views
{
    public class DevTabPlayer : UserControl
    {
        public DevTabPlayer()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }



        public void nudLevel_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {

        }

        public void cbDexNum_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void cbForm_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void cbSkin_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void cbGender_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void cbAnim_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
