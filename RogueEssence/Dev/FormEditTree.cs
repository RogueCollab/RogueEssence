using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RogueEssence.Data;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using System.IO;
using Microsoft.Win32;

namespace RogueEssence.Dev
{
    public partial class FormEditTree : Form
    {
        private bool checkSprites;
        private bool notifiedImport;

        private string cachedPath;
        private MonsterID cachedForm;

        public FormEditTree()
        {
            InitializeComponent();
        }

        public void LoadFormDataEntries(bool checkSprites)
        {
            this.checkSprites = checkSprites;
            this.Text = checkSprites ? "Char Sheets" : "Portraits";

            CharaIndexNode charaNode = GetIndexNode();
            for (int ii = 0; ii < DataManager.Instance.DataIndices[DataManager.DataType.Monster].Count; ii++)
            {
                FormEntrySummary dex = (FormEntrySummary)DataManager.Instance.DataIndices[DataManager.DataType.Monster].Entries[ii];

                TreeNode node = new TreeNode("#"+ii.ToString() + ": " + dex.Name.ToLocal(), 0, 0);
                node.Tag = new MonsterID(ii, -1, -1, Gender.Unknown);
                for (int jj = 0; jj < dex.FormTexts.Count; jj++)
                {
                    TreeNode formNode = new TreeNode("Form" + jj.ToString() + ": " + dex.FormTexts[jj].ToLocal(), 0, 0);
                    formNode.Tag = new MonsterID(ii, jj, -1, Gender.Unknown);
                    
                    for (int kk = 0; kk < DataManager.Instance.DataIndices[DataManager.DataType.Skin].Count; kk++)
                    {
                        SkinData skinData = DataManager.Instance.GetSkin(kk);
                        if (!skinData.Challenge)
                        {
                            TreeNode skinNode = new TreeNode(skinData.Name.ToLocal(), 0, 0);
                            skinNode.Tag = new MonsterID(ii, jj, kk, Gender.Unknown);
                            for (int mm = 0; mm < 3; mm++)
                            {
                                TreeNode genderNode = new TreeNode(((Gender)mm).ToString(), 0, 0);
                                genderNode.Tag = new MonsterID(ii, jj, kk, (Gender)mm);
                                skinNode.Nodes.Add(genderNode);
                            }

                            formNode.Nodes.Add(skinNode);
                        }
                    }

                    node.Nodes.Add(formNode);
                }

                try
                {
                    if (charaNode.Nodes.ContainsKey(ii))
                    {
                        if (charaNode.Nodes[ii].Position > 0)
                        {
                            node.ImageIndex = 1;
                            node.SelectedImageIndex = 1;
                        }

                        foreach (int form in charaNode.Nodes[ii].Nodes.Keys)
                        {
                            if (charaNode.Nodes[ii].Nodes[form].Position > 0)
                            {
                                node.Nodes[form].ImageIndex = 1;
                                node.Nodes[form].SelectedImageIndex = 1;
                            }

                            foreach (int skin in charaNode.Nodes[ii].Nodes[form].Nodes.Keys)
                            {
                                if (charaNode.Nodes[ii].Nodes[form].Nodes[skin].Position > 0)
                                {
                                    node.Nodes[form].Nodes[skin].ImageIndex = 1;
                                    node.Nodes[form].Nodes[skin].SelectedImageIndex = 1;
                                }


                                foreach (int gender in charaNode.Nodes[ii].Nodes[form].Nodes[skin].Nodes.Keys)
                                {
                                    if (charaNode.Nodes[ii].Nodes[form].Nodes[skin].Nodes[gender].Position > 0)
                                    {
                                        node.Nodes[form].Nodes[skin].Nodes[gender].ImageIndex = 1;
                                        node.Nodes[form].Nodes[skin].Nodes[gender].SelectedImageIndex = 1;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }

                tvForms.Nodes.Add(node);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            TreeNode node = tvForms.SelectedNode;
            if (node == null)
            {
                MessageBox.Show("No item selected to export from.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //get current sprite
            MonsterID formdata = (MonsterID)node.Tag;

            if (node.ImageIndex == 0)
            {
                MessageBox.Show("No graphics exist on this item.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //remember addresses in registry
            string folderName = (string)Registry.GetValue(DiagManager.REG_PATH, checkSprites ? "SpriteDir" : "PortraitDir", "");
            if (String.IsNullOrEmpty(folderName))
                folderName = Directory.GetCurrentDirectory();


            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = folderName;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Registry.SetValue(DiagManager.REG_PATH, checkSprites ? "SpriteDir" : "PortraitDir", dialog.SelectedPath);
                cachedPath = dialog.SelectedPath + "/";
                cachedForm = formdata;
                Export(cachedPath, cachedForm);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            TreeNode node = tvForms.SelectedNode;
            if (node == null)
            {
                MessageBox.Show("No item selected to import to.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //get current sprite
            MonsterID formdata = (MonsterID)node.Tag;

            if (node.ImageIndex != 0)
            {
                if (MessageBox.Show("Are you sure you want to overwrite the existing sheet:\n" + GetFormString(formdata), "Sprite Sheet already exists.",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
                    return;
            }

            //notify (once) that sprites need to follow a rigid guideline
            if (!notifiedImport)
            {
                MessageBox.Show("When importing sprites, " +
                    "make sure that all files in each folder adhere to the naming convention.", "Sprite Importing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                notifiedImport = true;
            }

            //remember addresses in registry
            string folderName = (string)Registry.GetValue(DiagManager.REG_PATH, checkSprites ? "SpriteDir" : "PortraitDir", "");
            if (String.IsNullOrEmpty(folderName))
                folderName = Directory.GetCurrentDirectory();

            //open window to choose directory
            
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = folderName;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Registry.SetValue(DiagManager.REG_PATH, checkSprites ? "SpriteDir" : "PortraitDir", dialog.SelectedPath);
                cachedPath = dialog.SelectedPath + "/";
                cachedForm = formdata;
                Import(cachedPath, cachedForm);
            }
        }

        private void btnReimport_Click(object sender, EventArgs e)
        {
            Import(cachedPath, cachedForm);
        }



        private void Export(string currentPath, MonsterID currentForm)
        {
            if (checkSprites)
            {
                CharSheet sheet = GraphicsManager.GetChara(currentForm);
                CharSheet.Export(sheet, currentPath);
            }
            else
            {
                PortraitSheet sheet = GraphicsManager.GetPortrait(currentForm);
                PortraitSheet.Export(sheet, currentPath);
            }


            DiagManager.Instance.LogInfo("Frames from:\n" +
                GetFormString(currentForm) +
                "\nhave been exported to:" + currentPath);


            btnReimport.Enabled = true;

        }

        private void Import(string currentPath, MonsterID currentForm)
        {
            string fileName = GetFilename(currentForm.Species);

            //load data
            Dictionary<MonsterID, byte[]> data = LoadSpeciesData(currentForm.Species);

            try
            {
                //write sprite data
                if (checkSprites)
                    ImportHelper.BakeCharSheet(currentPath, data, currentForm);
                else
                    ImportHelper.BakePortrait(currentPath, data, currentForm);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                MessageBox.Show("Error importing from\n"+currentPath+"\n\n" + ex.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //save data
            ImportHelper.SaveSpecies(fileName, data);

            DiagManager.Instance.LogInfo("Frames from:\n" +
                currentPath +
                "\nhave been imported to:" + GetFormString(currentForm));


            TreeNode node = tvForms.Nodes[currentForm.Species];
            if (currentForm.Form > -1)
                node = node.Nodes[currentForm.Form];
            if (currentForm.Skin > -1)
                node = node.Nodes[currentForm.Skin];
            if (currentForm.Gender != Gender.Unknown)
                node = node.Nodes[(int)currentForm.Gender];
            
            node.ImageIndex = 1;
            node.SelectedImageIndex = 1;

            btnReimport.Enabled = true;


            //signal for reload
            if (checkSprites)
                GraphicsManager.NeedReload = GraphicsManager.AssetType.Chara;
            else
                GraphicsManager.NeedReload = GraphicsManager.AssetType.Portrait;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            TreeNode node = tvForms.SelectedNode;
            if (node == null)
            {
                MessageBox.Show("No item selected to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (node.ImageIndex == 0)
            {
                MessageBox.Show("This spritesheet does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //get current sprite
            MonsterID formdata = (MonsterID)node.Tag;

            if (MessageBox.Show("Are you sure you want to delete the following sheet:\n"+GetFormString(formdata), "Delete Sprite Sheet.",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            string fileName = GetFilename(formdata.Species);
            Dictionary<MonsterID, byte[]> data = LoadSpeciesData(formdata.Species);

            //delete sprite data
            data.Remove(formdata);

            //save data
            ImportHelper.SaveSpecies(fileName, data);

            DiagManager.Instance.LogInfo("Deleted frames for:" + GetFormString(formdata));

            node.ImageIndex = 0;
            node.SelectedImageIndex = 0;

            //signal for reload
            GraphicsManager.NeedReload = GraphicsManager.AssetType.Chara;
        }

        private void tvForms_AfterSelect(object sender, TreeViewEventArgs e)
        {
            btnReimport.Enabled = false;
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
                return String.Format(GraphicsManager.CHARA_PATTERN, num);
            else
                return String.Format(GraphicsManager.PORTRAIT_PATTERN, num);
        }
    }
}
