using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RogueEssence.Dev
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DevForm : Window, IRootEditor
    {
        public bool LoadComplete { get; private set; }

        //private MapEditor mapEditor;
        //private GroundEditor groundEditor;

        public IMapEditor MapEditor => null;
        public IGroundEditor GroundEditor => null;


        public DevForm()
        {
            InitializeComponent();
        }


        void IRootEditor.Load()
        {

            this.Show();
        }

        public void OpenGround()
        {

        }


        public void CloseGround()
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComplete = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DiagManager.Instance.LoadMsg = "Closing...";
            EnterLoadPhase(GameBase.LoadPhase.Unload);
        }

        public static void EnterLoadPhase(GameBase.LoadPhase loadState)
        {
            GameBase.CurrentPhase = loadState;
        }

        private void btnEditSprites_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditPortraits_Click(object sender, RoutedEventArgs e)
        {

        }


        private void btnMapEditor_Click(object sender, RoutedEventArgs e)
        {

        }


        private void btnGroundEditor_Click(object sender, RoutedEventArgs e)
        {


        }

        private void btnEditMonster_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnEditSkill_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnEditIntrinsics_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnEditItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnEditZone_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnEditStatuses_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnEditMapStatuses_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnEditTerrain_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnEditTiles_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnEditAutoTile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditEmote_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditElement_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditGrowthGroup_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditSkillGroup_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditRank_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditSkin_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chkObject_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }


        private void btnSpawn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDespawn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSpawnItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRollSkill_Click(object sender, RoutedEventArgs e)
        {

        }

        private void nudLevel_ValueChanged(object sender, RoutedEventArgs e)
        {

        }

        private void btnToggleStatus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLearnSkill_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGiveSkill_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSetIntrinsic_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGiveFoes_Click(object sender, RoutedEventArgs e)
        {

        }



        private void btnReloadScripts_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtScriptInput_KeyPress()
        {

        }

        private void btnEnterMap_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEnterDungeon_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
