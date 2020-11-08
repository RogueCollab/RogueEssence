using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Dungeon;
using RogueEssence.Menu;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabDataViewModel : ViewModelBase
    {

        public void btnEditMonster_Click()
        {

        }
        public void btnEditSkill_Click()
        {

        }
        public void btnEditIntrinsics_Click()
        {

        }
        public void btnEditItem_Click()
        {

        }
        public void btnEditZone_Click()
        {
            Views.DataListForm dataListForm = new Views.DataListForm
            {
                DataContext = new DataListFormViewModel(),
            };
            dataListForm.Show();
        }
        public void btnEditStatuses_Click()
        {

        }
        public void btnEditMapStatuses_Click()
        {

        }
        public void btnEditTerrain_Click()
        {

        }
        public void btnEditTiles_Click()
        {

        }
        public void btnEditAutoTile_Click()
        {

        }

        public void btnEditEmote_Click()
        {

        }

        public void btnEditElement_Click()
        {

        }

        public void btnEditGrowthGroup_Click()
        {

        }

        public void btnEditSkillGroup_Click()
        {

        }

        public void btnEditRank_Click()
        {

        }

        public void btnEditSkin_Click()
        {

        }
        public void btnMapEditor_Click()
        {

        }

        public void btnGroundEditor_Click()
        {
            Views.DevForm form = (Views.DevForm)DiagManager.Instance.DevEditor;
            if (form.GroundEditForm == null)
            {
                MenuManager.Instance.ClearMenus();
                if (ZoneManager.Instance.CurrentGround != null)
                    GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToEditor(true, ZoneManager.Instance.CurrentGround.AssetName);
                else
                    GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToEditor(true, "");
            }
        }

    }
}
