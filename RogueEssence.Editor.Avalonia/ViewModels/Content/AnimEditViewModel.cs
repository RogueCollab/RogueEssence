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
    public class AnimEditViewModel : ViewModelBase
    {
        private GraphicsManager.AssetType assetType;
        private string assetPattern;
        private Window parent;


        public string Name { get { return assetType.ToString(); } }
        public ObservableCollection<string> Anims { get; }


        private string chosenAnim;
        public string ChosenAnim
        {
            get => chosenAnim;
            set
            {
                this.SetIfChanged(ref chosenAnim, value);
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


        public AnimEditViewModel()
        {
            Anims = new ObservableCollection<string>();
        }

        public void LoadDataEntries(GraphicsManager.AssetType assetType, string assetPattern, Window parent)
        {
            this.assetType = assetType;
            this.assetPattern = assetPattern;
            this.parent = parent;

            recomputeAnimList();
        }

        private void recomputeAnimList()
        {
            lock (GameBase.lockObj)
            {
                Anims.Clear();
                string[] dirs = Directory.GetFiles(Path.GetDirectoryName(assetPattern), String.Format(Path.GetFileName(assetPattern), "*"));
                for (int ii = 0; ii < dirs.Length; ii++)
                {
                    string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                    Anims.Add(filename);
                }
            }
        }

        public async void btnImport_Click()
        {
            //remember addresses in registry
            string folderName = null;// (string)Registry.GetValue(DiagManager.REG_PATH, checkSprites ? "SpriteDir" : "PortraitDir", "");
            if (String.IsNullOrEmpty(folderName))
                folderName = Directory.GetCurrentDirectory();

            //open window to choose directory
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Directory = folderName;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "PNG Files";
            filter.Extensions.Add("png");
            openFileDialog.Filters.Add(filter);

            string[] results = await openFileDialog.ShowAsync(parent);

            if (results.Length > 0)
            {
                string animName = Path.GetFileNameWithoutExtension(results[0]);

                if (Anims.Contains(animName))
                {
                    MessageBox.MessageBoxResult result = await MessageBox.Show(parent, "Are you sure you want to overwrite the existing sheet:\n" + animName, "Sprite Sheet already exists.",
                        MessageBox.MessageBoxButtons.YesNo);
                    if (result == MessageBox.MessageBoxResult.No)
                        return;
                }

                //Registry.SetValue(DiagManager.REG_PATH, checkSprites ? "SpriteDir" : "PortraitDir", dialog.SelectedPath);
                CachedPath = results[0];

                try
                {
                    lock (GameBase.lockObj)
                        Import(CachedPath);
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
                    Import(CachedPath);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBox.Show(parent, "Error importing from\n" + CachedPath + "\n\n" + ex.Message, "Import Failed", MessageBox.MessageBoxButtons.Ok);
                return;
            }
        }

        public async void btnExport_Click()
        {
            //get current sprite
            string animData = chosenAnim;

            //remember addresses in registry
            string folderName = null;// (string)Registry.GetValue(DiagManager.REG_PATH, checkSprites ? "SpriteDir" : "PortraitDir", "");
            if (String.IsNullOrEmpty(folderName))
                folderName = Directory.GetCurrentDirectory();


            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Directory = folderName;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "PNG Files";
            filter.Extensions.Add("png");
            saveFileDialog.Filters.Add(filter);

            string folder = await saveFileDialog.ShowAsync(parent);

            if (folder != null)
            {
                //Registry.SetValue(DiagManager.REG_PATH, checkSprites ? "SpriteDir" : "PortraitDir", dialog.SelectedPath);
                //CachedPath = folder;
                lock (GameBase.lockObj)
                    Export(folder, animData);
            }
        }

        public async void btnDelete_Click()
        {
            //get current sprite
            string animData = chosenAnim;

            MessageBox.MessageBoxResult result = await MessageBox.Show(parent, "Are you sure you want to delete the following sheet:\n" + animData, "Delete Sprite Sheet.",
                MessageBox.MessageBoxButtons.YesNo);
            if (result == MessageBox.MessageBoxResult.No)
                return;


            lock (GameBase.lockObj)
                Delete(chosenAnim);
        }


        private void Import(string currentPath)
        {
            string animName = Path.GetFileNameWithoutExtension(currentPath);
            string[] components = animName.Split('.');
            //write sprite data
            using (DirSheet sheet = DirSheet.Import(currentPath))
            {
                using (FileStream stream = File.OpenWrite(String.Format(assetPattern, components[0])))
                {
                    //save data
                    using (BinaryWriter writer = new BinaryWriter(stream))
                        sheet.Save(writer);
                }
            }

            DiagManager.Instance.LogInfo("Frames from:\n" +
                currentPath + "\nhave been imported.");

            //recompute
            recomputeAnimList();

            //signal for reload
            GraphicsManager.NeedReload = assetType;
        }



        private void Export(string currentPath, string anim)
        {
            if (File.Exists(String.Format(assetPattern, anim)))
            {
                //read file and read binary data
                using (FileStream fileStream = File.OpenRead(String.Format(assetPattern, anim)))
                {
                    using (BinaryReader reader = new BinaryReader(fileStream))
                    {
                        DirSheet sheet = DirSheet.Load(reader);

                        string filename = DirSheet.GetExportString(sheet, Path.GetFileNameWithoutExtension(currentPath));
                        string dirname = Path.GetDirectoryName(currentPath);
                        DirSheet.Export(sheet, Path.Combine(dirname, filename + ".png"));
                    }
                }
            }

            DiagManager.Instance.LogInfo("Frames from:\n" +
                anim +
                "\nhave been exported to:" + currentPath);
        }


        private void Delete(string anim)
        {
            if (File.Exists(String.Format(assetPattern, anim)))
                File.Delete(String.Format(assetPattern, anim));

            Anims.Remove(anim);

            DiagManager.Instance.LogInfo("Deleted frames for:" + anim);

            //signal for reload
            GraphicsManager.NeedReload = GraphicsManager.AssetType.Chara;

        }

    }
}
