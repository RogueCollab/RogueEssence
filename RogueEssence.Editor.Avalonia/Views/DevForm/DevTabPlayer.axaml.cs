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
            if (GameManager.Instance.IsInGame())
                Dungeon.DungeonScene.Instance.FocusedCharacter.Level = (int)e.NewValue;
        }

    }
}
