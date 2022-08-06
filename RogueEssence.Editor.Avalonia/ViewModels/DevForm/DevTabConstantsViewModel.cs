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

        public void btnEditUniversal_Click()
        {
            OpenList<ActiveEffect>("Universal Event", DataManager.Instance.UniversalEvent, (obj) => {
                DataManager.Instance.UniversalEvent = obj;
                DataManager.SaveData(PathMod.HardMod(DataManager.DATA_PATH + "Universal" + DataManager.DATA_EXT), obj);
            });
        }
        public void btnEditHeal_Click()
        {
            OpenList<BattleFX>("Heal FX", DataManager.Instance.HealFX, (fx) => {
                DataManager.Instance.HealFX = fx;
                DataManager.SaveData(PathMod.HardMod(DataManager.FX_PATH + "Heal" + DataManager.DATA_EXT), fx);
            });
        }
        public void btnEditRestoreCharge_Click()
        {
            OpenList<BattleFX>("Restore Charge FX", DataManager.Instance.RestoreChargeFX, (fx) => {
                DataManager.Instance.RestoreChargeFX = fx;
                DataManager.SaveData(PathMod.HardMod(DataManager.FX_PATH + "RestoreCharge" + DataManager.DATA_EXT), fx);
            });
        }
        public void btnEditLoseCharge_Click()
        {
            OpenList<BattleFX>("Lose Charge FX", DataManager.Instance.LoseChargeFX, (fx) => {
                DataManager.Instance.LoseChargeFX = fx;
                DataManager.SaveData(PathMod.HardMod(DataManager.FX_PATH + "LoseCharge" + DataManager.DATA_EXT), fx);
            });
        }
        public void btnEditNoCharge_Click()
        {
            OpenList<EmoteFX>("No Charge FX", DataManager.Instance.NoChargeFX, (fx) => {
                DataManager.Instance.NoChargeFX = fx;
                DataManager.SaveData(PathMod.HardMod(DataManager.FX_PATH + "NoCharge" + DataManager.DATA_EXT), fx);
            });
        }
        public void btnEditElement_Click()
        {
            OpenList<BattleFX>("Element FX", DataManager.Instance.ElementFX, (fx) => {
                DataManager.Instance.ElementFX = fx;
                DataManager.SaveData(PathMod.HardMod(DataManager.FX_PATH + "Element" + DataManager.DATA_EXT), fx);
            });
        }
        public void btnEditIntrinsic_Click()
        {
            OpenList<BattleFX>("Intrinsic FX", DataManager.Instance.IntrinsicFX, (fx) => {
                DataManager.Instance.IntrinsicFX = fx;
                DataManager.SaveData(PathMod.HardMod(DataManager.FX_PATH + "Intrinsic" + DataManager.DATA_EXT), fx);
            });
        }
        public void btnEditSendHome_Click()
        {
            OpenList<BattleFX>("Send Home FX", DataManager.Instance.SendHomeFX, (fx) => {
                DataManager.Instance.SendHomeFX = fx;
                DataManager.SaveData(PathMod.HardMod(DataManager.FX_PATH + "SendHome" + DataManager.DATA_EXT), fx);
            });
        }
        public void btnEditItemLost_Click()
        {
            OpenList<BattleFX>("Item Lost FX", DataManager.Instance.ItemLostFX, (fx) => {
                DataManager.Instance.ItemLostFX = fx;
                DataManager.SaveData(PathMod.HardMod(DataManager.FX_PATH + "ItemLost" + DataManager.DATA_EXT), fx);
            });
        }
        public void btnEditWarp_Click()
        {
            OpenList<BattleFX>("Warp FX", DataManager.Instance.WarpFX, (fx) => {
                DataManager.Instance.WarpFX = fx;
                DataManager.SaveData(PathMod.HardMod(DataManager.FX_PATH + "Warp" + DataManager.DATA_EXT), fx);
            });
        }

        public void btnEditKnockback_Click()
        {
            OpenList<BattleFX>("Knockback FX", DataManager.Instance.KnockbackFX, (fx) => {
                DataManager.Instance.KnockbackFX = fx;
                DataManager.SaveData(PathMod.HardMod(DataManager.FX_PATH + "Knockback" + DataManager.DATA_EXT), fx);
            });
        }

        public void btnEditJump_Click()
        {
            OpenList<BattleFX>("Jump FX", DataManager.Instance.JumpFX, (fx) => {
                DataManager.Instance.JumpFX = fx;
                DataManager.SaveData(PathMod.HardMod(DataManager.FX_PATH + "Jump" + DataManager.DATA_EXT), fx);
            });
        }

        public void btnEditThrow_Click()
        {
            OpenList<BattleFX>("Throw FX", DataManager.Instance.ThrowFX, (fx) => { DataManager.Instance.ThrowFX = fx;
                DataManager.SaveData(PathMod.HardMod(DataManager.FX_PATH + "Throw" + DataManager.DATA_EXT), fx);
            });
        }

        private delegate void SaveFX<T>(T obj);
        private void OpenList<T>(string name, T data, SaveFX<T> saveOp)
        {
            lock (GameBase.lockObj)
            {
                Views.DataEditForm editor = new Views.DataEditRootForm();
                editor.Title = DataEditor.GetWindowTitle("", name, data, data.GetType());
                DataEditor.LoadDataControls(data, editor);
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
