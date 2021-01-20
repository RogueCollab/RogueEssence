using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueEssence.Content;
using System.IO;
using Avalonia.Media.Imaging;
using RogueElements;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{
    public class OpContainer
    {
        public Action CommandAction;
        public CharSheetOp Op;
        public string Name { get { return Op.Name; } }

        public OpContainer(CharSheetOp op, Action command)
        {
            Op = op;
            CommandAction = command;
        }

        public void Command()
        {
            CommandAction();
        }
    }

    public class MonsterNodeViewModel : ViewModelBase
    {

        private bool filled;
        public bool Filled
        {
            get => filled;
            set => this.SetIfChanged(ref filled, value);
        }

        private string name;
        public string Name
        {
            get => name;
            set => this.SetIfChanged(ref name, value);
        }

        public MonsterID ID;

        public ObservableCollection<MonsterNodeViewModel> Nodes { get; }

        public MonsterNodeViewModel(string name, MonsterID id, bool filled)
        {
            this.name = name;
            this.ID = id;
            this.filled = filled;
            Nodes = new ObservableCollection<MonsterNodeViewModel>();
        }

    }

    public class SpeciesEditViewModel : ViewModelBase
    {

        private bool checkSprites;
        private bool notifiedImport;

        private Window parent;


        public string Name
        {
            get { return checkSprites ? GraphicsManager.AssetType.Chara.ToString() : GraphicsManager.AssetType.Portrait.ToString(); }
            set { }
        }
        public ObservableCollection<MonsterNodeViewModel> Monsters { get; }
        public ObservableCollection<OpContainer> OpList { get; }


        private MonsterNodeViewModel chosenMonster;
        public MonsterNodeViewModel ChosenMonster
        {
            get => chosenMonster;
            set
            {
                this.SetIfChanged(ref chosenMonster, value);
                CachedPath = null;
            }
        }

        //ReImport enabled if CachedPath is not null

        private string cachedPath;
        public string CachedPath
        {
            get => cachedPath;
            set => this.SetIfChanged(ref cachedPath, value);
        }


        public SpeciesEditViewModel()
        {
            Monsters = new ObservableCollection<MonsterNodeViewModel>();
            OpList = new ObservableCollection<OpContainer>();
        }


        private bool hasSprite(CharaIndexNode parent, MonsterID id)
        {
            return GraphicsManager.GetFallbackForm(parent, id) == id;
        }

        private async void applyOpToCharSheet(CharSheetOp op)
        {
            //get current sprite
            MonsterID currentForm = chosenMonster.ID;
            string fileName = GetFilename(currentForm.Species);

            if (!chosenMonster.Filled)
            {
                await MessageBox.Show(parent, String.Format("No graphics exist for {0}.", chosenMonster.Name), "Error", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            CharSheet sheet = GraphicsManager.GetChara(currentForm);

            op.Apply(sheet);

            //load data
            Dictionary<MonsterID, byte[]> data = LoadSpeciesData(currentForm.Species);

            //write sprite data
            WriteSpeciesData(data, currentForm, sheet);

            //save data
            ImportHelper.SaveSpecies(fileName, data);

            GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Chara);
            GraphicsManager.ClearCaches(GraphicsManager.AssetType.Chara);


            DiagManager.Instance.LogInfo(String.Format("{0} applied to: {1}", op.Name, GetFormString(currentForm)));
        }

        public void LoadFormDataEntries(bool sprites, Window parent)
        {
            checkSprites = sprites;
            this.parent = parent;
            if (sprites)
            {
                OpList.Add(new OpContainer(new CharSheetDummyOp("Export as Single Sheet"), ExportSingleSheet));
                foreach (CharSheetOp op in DevGraphicsManager.CharSheetOps)
                    OpList.Add(new OpContainer(op, () => applyOpToCharSheet(op)));
            }

            lock (GameBase.lockObj)
            {
                CharaIndexNode charaNode = GetIndexNode();
                for (int ii = 0; ii < DataManager.Instance.DataIndices[DataManager.DataType.Monster].Count; ii++)
                {
                    FormEntrySummary dex = (FormEntrySummary)DataManager.Instance.DataIndices[DataManager.DataType.Monster].Entries[ii];

                    MonsterID dexID = new MonsterID(ii, -1, -1, Gender.Unknown);
                    MonsterNodeViewModel node = new MonsterNodeViewModel("#" + ii.ToString() + ": " + dex.Name.ToLocal(), dexID, hasSprite(charaNode, dexID));
                    for (int jj = 0; jj < dex.FormTexts.Count; jj++)
                    {
                        MonsterID formID = new MonsterID(ii, jj, -1, Gender.Unknown);
                        MonsterNodeViewModel formNode = new MonsterNodeViewModel("Form" + jj.ToString() + ": " + dex.FormTexts[jj].ToLocal(), formID, hasSprite(charaNode, formID));
                        for (int kk = 0; kk < DataManager.Instance.DataIndices[DataManager.DataType.Skin].Count; kk++)
                        {
                            SkinData skinData = DataManager.Instance.GetSkin(kk);
                            if (!skinData.Challenge)
                            {
                                MonsterID skinID = new MonsterID(ii, jj, kk, Gender.Unknown);
                                MonsterNodeViewModel skinNode = new MonsterNodeViewModel(skinData.Name.ToLocal(), skinID, hasSprite(charaNode, skinID));
                                for (int mm = 0; mm < 3; mm++)
                                {
                                    MonsterID genderID = new MonsterID(ii, jj, kk, (Gender)mm);
                                    MonsterNodeViewModel genderNode = new MonsterNodeViewModel(((Gender)mm).ToString(), genderID, hasSprite(charaNode, genderID));
                                    skinNode.Nodes.Add(genderNode);
                                }

                                formNode.Nodes.Add(skinNode);
                            }
                        }

                        node.Nodes.Add(formNode);
                    }

                    Monsters.Add(node);
                }
            }
        }


        public async void btnImport_Click()
        {
            //get current sprite
            MonsterID formdata = chosenMonster.ID;

            if (chosenMonster.Filled)
            {
                MessageBox.MessageBoxResult result = await MessageBox.Show(parent, "Are you sure you want to overwrite the existing sheet:\n" + GetFormString(formdata), "Sprite Sheet already exists.",
                    MessageBox.MessageBoxButtons.YesNo);
                if (result == MessageBox.MessageBoxResult.No)
                    return;
            }

            //notify (once) that sprites need to follow a rigid guideline
            if (!notifiedImport)
            {
                await MessageBox.Show(parent, "When importing sprites, " +
                    "make sure that all files in each folder adhere to the naming convention.", "Sprite Importing", MessageBox.MessageBoxButtons.Ok);
                notifiedImport = true;
            }

            //remember addresses in registry
            string folderName = DevForm.GetConfig(Name + "Dir", Directory.GetCurrentDirectory());

            //open window to choose directory
            OpenFolderDialog openFileDialog = new OpenFolderDialog();
            openFileDialog.Directory = folderName;

            string folder = await openFileDialog.ShowAsync(parent);

            if (folder != "")
            {
                DevForm.SetConfig(Name + "Dir", folder);
                CachedPath = folder + "/";

                try
                {
                    lock (GameBase.lockObj)
                        Import(CachedPath, formdata);
                    chosenMonster.Filled = true;
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex, false);
                    await MessageBox.Show(parent, "Error importing from\n" + CachedPath + "\n\n" + ex.Message, "Import Failed", MessageBox.MessageBoxButtons.Ok);
                    return;
                }
            }
        }

        public async void btnReImport_Click()
        {
            try
            {
                lock (GameBase.lockObj)
                    Import(CachedPath, chosenMonster.ID);
                chosenMonster.Filled = true;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBox.Show(parent, "Error importing from\n" + CachedPath + "\n\n" + ex.Message, "Import Failed", MessageBox.MessageBoxButtons.Ok);
                return;
            }
        }

        public void ExportSingleSheet()
        {
            ExportFlow(true);
        }

        public void btnExport_Click()
        {
            ExportFlow(false);
        }

        public async void ExportFlow(bool singleSheet)
        {
            //get current sprite
            MonsterID formdata = chosenMonster.ID;

            if (!chosenMonster.Filled)
            {
                await MessageBox.Show(parent, String.Format("No graphics exist for {0}.", chosenMonster.Name), "Error", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            //remember addresses in registry
            string folderName = DevForm.GetConfig(Name + "Dir", Directory.GetCurrentDirectory());

            //open window to choose directory
            OpenFolderDialog openFileDialog = new OpenFolderDialog();
            openFileDialog.Directory = folderName;

            string folder = await openFileDialog.ShowAsync(parent);

            if (folder != "")
            {
                DevForm.SetConfig(Name + "Dir", folder);
                CachedPath = folder + "/";
                lock (GameBase.lockObj)
                    Export(CachedPath, formdata, singleSheet);
            }
        }

        public async void btnDelete_Click()
        {
            if (!chosenMonster.Filled)
            {
                await MessageBox.Show(parent, String.Format("No graphics exist for {0}.", chosenMonster.Name), "Error", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            //get current sprite
            MonsterID formdata = chosenMonster.ID;

            MessageBox.MessageBoxResult result = await MessageBox.Show(parent, "Are you sure you want to delete the following sheet:\n" + GetFormString(formdata), "Delete Sprite Sheet.",
                MessageBox.MessageBoxButtons.YesNo);
            if (result == MessageBox.MessageBoxResult.No)
                return;


            lock (GameBase.lockObj)
                Delete(formdata);

            chosenMonster.Filled = false;
        }


        private void Import(string currentPath, MonsterID currentForm)
        {
            string fileName = GetFilename(currentForm.Species);

            //load data
            Dictionary<MonsterID, byte[]> data = LoadSpeciesData(currentForm.Species);

            //write sprite data
            if (checkSprites)
                ImportHelper.BakeCharSheet(currentPath, data, currentForm);
            else
                ImportHelper.BakePortrait(currentPath, data, currentForm);

            //save data
            ImportHelper.SaveSpecies(fileName, data);

            if (checkSprites)
            {
                GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Chara);
                GraphicsManager.ClearCaches(GraphicsManager.AssetType.Chara);
            }
            else
            {
                GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Portrait);
                GraphicsManager.ClearCaches(GraphicsManager.AssetType.Portrait);
            }


            DiagManager.Instance.LogInfo("Frames from:\n" +
                currentPath +
                "\nhave been imported to:" + GetFormString(currentForm));

        }



        private void Export(string currentPath, MonsterID currentForm, bool singleSheet)
        {
            if (checkSprites)
            {
                CharSheet sheet = GraphicsManager.GetChara(currentForm);
                CharSheet.Export(sheet, currentPath, singleSheet);
            }
            else
            {
                PortraitSheet sheet = GraphicsManager.GetPortrait(currentForm);
                PortraitSheet.Export(sheet, currentPath);
            }


            DiagManager.Instance.LogInfo("Frames from:\n" +
                GetFormString(currentForm) +
                "\nhave been exported to:" + currentPath);
        }


        private void Delete(MonsterID formdata)
        {
            string fileName = GetFilename(formdata.Species);
            Dictionary<MonsterID, byte[]> data = LoadSpeciesData(formdata.Species);

            //delete sprite data
            data.Remove(formdata);

            //save data
            ImportHelper.SaveSpecies(fileName, data);

            if (checkSprites)
            {
                GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Chara);
                GraphicsManager.ClearCaches(GraphicsManager.AssetType.Chara);
            }
            else
            {
                GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Portrait);
                GraphicsManager.ClearCaches(GraphicsManager.AssetType.Portrait);
            }

            DiagManager.Instance.LogInfo("Deleted frames for:" + GetFormString(formdata));

        }

        private void WriteSpeciesData(Dictionary<MonsterID, byte[]> spriteData, MonsterID formData, CharSheet sprite)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                    sprite.Save(writer);

                byte[] writingBytes = stream.ToArray();
                spriteData[formData] = writingBytes;
            }
        }

        private Dictionary<MonsterID, byte[]> LoadSpeciesData(int num)
        {
            Dictionary<MonsterID, byte[]> dict = new Dictionary<MonsterID, byte[]>();

            MonsterData dex = DataManager.Instance.GetMonster(num);
            CharaIndexNode charaNode = GetIndexNode();

            if (charaNode.Nodes.ContainsKey(num))
            {
                if (charaNode.Nodes[num].Position > 0)
                    LoadSpeciesFormData(dict, new MonsterID(num, -1, -1, Gender.Unknown));

                foreach (int form in charaNode.Nodes[num].Nodes.Keys)
                {
                    if (charaNode.Nodes[num].Nodes[form].Position > 0)
                        LoadSpeciesFormData(dict, new MonsterID(num, form, -1, Gender.Unknown));

                    foreach (int skin in charaNode.Nodes[num].Nodes[form].Nodes.Keys)
                    {
                        if (charaNode.Nodes[num].Nodes[form].Nodes[skin].Position > 0)
                            LoadSpeciesFormData(dict, new MonsterID(num, form, skin, Gender.Unknown));

                        foreach (int gender in charaNode.Nodes[num].Nodes[form].Nodes[skin].Nodes.Keys)
                        {
                            if (charaNode.Nodes[num].Nodes[form].Nodes[skin].Nodes[gender].Position > 0)
                                LoadSpeciesFormData(dict, new MonsterID(num, form, skin, (Gender)gender));
                        }
                    }
                }
            }

            return dict;
        }

        private void LoadSpeciesFormData(Dictionary<MonsterID, byte[]> data, MonsterID formData)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    if (checkSprites)
                        GraphicsManager.GetChara(formData).Save(writer);
                    else
                        GraphicsManager.GetPortrait(formData).Save(writer);
                }
                byte[] writingBytes = stream.ToArray();
                data[formData] = writingBytes;
            }
        }

        private string GetFormString(MonsterID formdata)
        {
            string name = DataManager.Instance.GetMonster(formdata.Species).Name.ToLocal();
            if (formdata.Form > -1)
                name += ", " + DataManager.Instance.GetMonster(formdata.Species).Forms[formdata.Form].FormName.ToLocal() + " form";
            if (formdata.Skin > -1)
                name += ", " + formdata.Skin + " skin";
            if (formdata.Gender != Gender.Unknown)
                name += ", " + formdata.Gender + " gender";
            return name;
        }

        private CharaIndexNode GetIndexNode()
        {
            if (checkSprites)
                return GraphicsManager.CharaIndex;
            else
                return GraphicsManager.PortraitIndex;
        }

        private string GetFilename(int num)
        {
            if (checkSprites)
                return PathMod.HardMod(String.Format(GraphicsManager.CHARA_PATTERN, num));
            else
                return PathMod.HardMod(String.Format(GraphicsManager.PORTRAIT_PATTERN, num));
        }
    }
}
