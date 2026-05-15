using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public partial class MapRetileWindow : ChromelessWindow
    {
        public MapRetileWindow()
        {
            // var pref = ViewModels.PreferencesWindowViewModel.Instance;
            // DataContext = pref;
            // CloseOnESC = true;
            // InitializeComponent();
        }
    
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            // base.OnClosing(e);
            //
            // if (Design.IsDesignMode)
            //     return;
            //
            // ViewModels.PreferencesWindowViewModel.Instance.Save();
        }
        
        public void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close(true);
        }


        public void btnCancel_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
