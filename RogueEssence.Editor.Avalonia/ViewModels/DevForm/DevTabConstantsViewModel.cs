using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Menu;
using RogueEssence.Script;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabConstantsViewModel : ViewModelBase
    {
        public void btnEditStartParams_Click()
        {
            OpenItem<StartParams>("Start Params", DataManager.Instance.Start, (obj) => {
                DataManager.Instance.Start = obj;
                DataManager.Instance.SaveStartParams();
            });
        }

        public void btnEditUniversal_Click()
        {
            OpenItem<UniversalBaseEffect>("Universal Event", (UniversalBaseEffect)DataManager.Instance.UniversalEvent, (obj) => {
                DataManager.Instance.UniversalEvent = obj;
                DataManager.SaveData(obj, DataManager.DATA_PATH, "Universal", DataManager.DATA_EXT);
            });
        }
        public void mnuUniversalFile_Click()
        {
            DataManager.SaveData(DataManager.Instance.UniversalEvent, DataManager.DATA_PATH, "Universal", DataManager.DATA_EXT, DataManager.SavePolicy.File);
        }
        public void mnuUniversalDiff_Click()
        {
            //TODO: yell at the user or prevent them from doing this, if they do not have a mod currently selected.
            //you can't make a diff for the base game!
            DataManager.SaveData(DataManager.Instance.UniversalEvent, DataManager.DATA_PATH, "Universal", DataManager.DATA_EXT, DataManager.SavePolicy.Diff);
        }

        public void btnEditStrings_Click()
        {
            StringsEditViewModel mv = new StringsEditViewModel();
            Views.StringsEditForm editForm = new Views.StringsEditForm();
            mv.LoadStringEntries(false, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }

        public void btnEditStringsEx_Click()
        {
            StringsEditViewModel mv = new StringsEditViewModel();
            Views.StringsEditForm editForm = new Views.StringsEditForm();
            mv.LoadStringEntries(true, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }



        public void btnEditHeal_Click()
        {
            OpenItem<BattleFX>("Heal FX", DataManager.Instance.HealFX, (fx) => {
                DataManager.Instance.HealFX = fx;
                DataManager.SaveData(fx, DataManager.FX_PATH, "Heal", DataManager.DATA_EXT);
            });
        }
        public void btnEditRestoreCharge_Click()
        {
            OpenItem<BattleFX>("Restore Charge FX", DataManager.Instance.RestoreChargeFX, (fx) => {
                DataManager.Instance.RestoreChargeFX = fx;
                DataManager.SaveData(fx, DataManager.FX_PATH, "RestoreCharge", DataManager.DATA_EXT);
            });
        }
        public void btnEditLoseCharge_Click()
        {
            OpenItem<BattleFX>("Lose Charge FX", DataManager.Instance.LoseChargeFX, (fx) => {
                DataManager.Instance.LoseChargeFX = fx;
                DataManager.SaveData(fx, DataManager.FX_PATH, "LoseCharge", DataManager.DATA_EXT);
            });
        }
        public void btnEditNoCharge_Click()
        {
            OpenItem<EmoteFX>("No Charge FX", DataManager.Instance.NoChargeFX, (fx) => {
                DataManager.Instance.NoChargeFX = fx;
                DataManager.SaveData(fx, DataManager.FX_PATH, "NoCharge", DataManager.DATA_EXT);
            });
        }
        public void btnEditElement_Click()
        {
            OpenItem<BattleFX>("Element FX", DataManager.Instance.ElementFX, (fx) => {
                DataManager.Instance.ElementFX = fx;
                DataManager.SaveData(fx, DataManager.FX_PATH, "Element", DataManager.DATA_EXT);
            });
        }
        public void btnEditIntrinsic_Click()
        {
            OpenItem<BattleFX>("Intrinsic FX", DataManager.Instance.IntrinsicFX, (fx) => {
                DataManager.Instance.IntrinsicFX = fx;
                DataManager.SaveData(fx, DataManager.FX_PATH, "Intrinsic", DataManager.DATA_EXT);
            });
        }
        public void btnEditSendHome_Click()
        {
            OpenItem<BattleFX>("Send Home FX", DataManager.Instance.SendHomeFX, (fx) => {
                DataManager.Instance.SendHomeFX = fx;
                DataManager.SaveData(fx, DataManager.FX_PATH, "SendHome", DataManager.DATA_EXT);
            });
        }
        public void btnEditItemLost_Click()
        {
            OpenItem<BattleFX>("Item Lost FX", DataManager.Instance.ItemLostFX, (fx) => {
                DataManager.Instance.ItemLostFX = fx;
                DataManager.SaveData(fx, DataManager.FX_PATH, "ItemLost", DataManager.DATA_EXT);
            });
        }
        public void btnEditWarp_Click()
        {
            OpenItem<BattleFX>("Warp FX", DataManager.Instance.WarpFX, (fx) => {
                DataManager.Instance.WarpFX = fx;
                DataManager.SaveData(fx, DataManager.FX_PATH, "Warp", DataManager.DATA_EXT);
            });
        }

        public void btnEditKnockback_Click()
        {
            OpenItem<BattleFX>("Knockback FX", DataManager.Instance.KnockbackFX, (fx) => {
                DataManager.Instance.KnockbackFX = fx;
                DataManager.SaveData(fx, DataManager.FX_PATH, "Knockback", DataManager.DATA_EXT);
            });
        }

        public void btnEditJump_Click()
        {
            OpenItem<BattleFX>("Jump FX", DataManager.Instance.JumpFX, (fx) => {
                DataManager.Instance.JumpFX = fx;
                DataManager.SaveData(fx, DataManager.FX_PATH, "Jump", DataManager.DATA_EXT);
            });
        }

        public void btnEditThrow_Click()
        {
            OpenItem<BattleFX>("Throw FX", DataManager.Instance.ThrowFX, (fx) => { DataManager.Instance.ThrowFX = fx;
                DataManager.SaveData(fx, DataManager.FX_PATH, "Throw", DataManager.DATA_EXT);
            });
        }

        private delegate void SaveFX<T>(T obj);
        private void OpenItem<T>(string name, T data, SaveFX<T> saveOp)
        {
            lock (GameBase.lockObj)
            {
                Views.DataEditForm editor = new Views.DataEditRootForm();
                editor.Title = DataEditor.GetWindowTitle("", name, data, data.GetType());
                DataEditor.LoadDataControls("", data, editor);
                editor.SelectedOKEvent += async () =>
                {
                    lock (GameBase.lockObj)
                    {
                        object obj = data;
                        DataEditor.SaveDataControls(ref obj, editor.ControlPanel, new Type[0]);
                        saveOp((T)obj);
                        return true;
                    }
                };

                editor.Show();

            }
        }

    }
}
