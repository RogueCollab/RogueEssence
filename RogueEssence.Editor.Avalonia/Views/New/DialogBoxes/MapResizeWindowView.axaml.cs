using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public partial class MapResizeWindowView : ChromelessWindow
    {
        public MapResizeWindowView()
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
