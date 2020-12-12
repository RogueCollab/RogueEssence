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


        private List<string> anims;
        public SearchListBoxViewModel Anims { get; set; }

        private string cachedPath;
        public string CachedPath
        {
            get => cachedPath;
            set => this.SetIfChanged(ref cachedPath, value);
        }


        public AnimEditViewModel()
        {
            anims = new List<string>();

            Anims = new SearchListBoxViewModel();
            Anims.DataName = "Graphics:";
            Anims.SelectedIndexChanged += Anims_SelectedIndexChanged;

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
                anims.Clear();
                Anims.Clear();
                string[] dirs = PathMod.GetModFiles(Path.GetDirectoryName(assetPattern), String.Format(Path.GetFileName(assetPattern), "*"));
                for (int ii = 0; ii < dirs.Length; ii++)
                {
                    string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                    anims.Add(filename);
                    Anims.AddItem(filename);
                }
            }
        }

        public async void btnImport_Click()
        {
            //remember addresses in registry
            string folderName = DevForm.GetConfig(Name + "Dir", Directory.GetCurrentDirectory());

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

                if (anims.Contains(animName))
                {
                    MessageBox.MessageBoxResult result = await MessageBox.Show(parent, "Are you sure you want to overwrite the existing sheet:\n" + animName, "Sprite Sheet already exists.",
                        MessageBox.MessageBoxButtons.YesNo);
                    if (result == MessageBox.MessageBoxResult.No)
                        return;
                }

                DevForm.SetConfig(Name + "Dir", Path.GetDirectoryName(results[0]));
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
            string animData = anims[Anims.InternalIndex];

            //remember addresses in registry
            string folderName = DevForm.GetConfig(Name + "Dir", Directory.GetCurrentDirectory());

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Directory = folderName;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "PNG Files";
            filter.Extensions.Add("png");
            saveFileDialog.Filters.Add(filter);

            string folder = await saveFileDialog.ShowAsync(parent);

            if (folder != null)
            {
                DevForm.SetConfig(Name + "Dir", Path.GetDirectoryName(folder));
                //CachedPath = folder;
                lock (GameBase.lockObj)
                    Export(folder, animData);
            }
        }

        public async void btnDelete_Click()
        {
            //get current sprite
            int animIdx = Anims.InternalIndex;

            MessageBox.MessageBoxResult result = await MessageBox.Show(parent, "Are you sure you want to delete the following sheet:\n" + anims[animIdx], "Delete Sprite Sheet.",
                MessageBox.MessageBoxButtons.YesNo);
            if (result == MessageBox.MessageBoxResult.No)
                return;


            lock (GameBase.lockObj)
                Delete(animIdx);
        }


        private void Import(string currentPath)
        {
            string animName = Path.GetFileNameWithoutExtension(currentPath);
            string[] components = animName.Split('.');
            //write sprite data
            using (DirSheet sheet = DirSheet.Import(currentPath))
            {
                using (FileStream stream = File.OpenWrite(PathMod.HardMod(String.Format(assetPattern, components[0]))))
                {
                    //save data
                    using (BinaryWriter writer = new BinaryWriter(stream))
                        sheet.Save(writer);
                }
            }
            GraphicsManager.RebuildIndices(assetType);
            GraphicsManager.ClearCaches(assetType);

            DiagManager.Instance.LogInfo("Frames from:\n" +
                currentPath + "\nhave been imported.");

            //recompute
            recomputeAnimList();
        }



        private void Export(string currentPath, string anim)
        {
            string animPath = PathMod.ModPath(String.Format(assetPattern, anim));
            if (File.Exists(animPath))
            {
                //read file and read binary data
                using (FileStream fileStream = File.OpenRead(animPath))
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


        private void Delete(int animIdx)
        {
            string anim = anims[animIdx];

            string animPath = PathMod.ModPath(String.Format(assetPattern, anim));
            if (File.Exists(animPath))
                File.Delete(animPath);

            anims.RemoveAt(animIdx);
            Anims.RemoveInternalAt(animIdx);

            GraphicsManager.RebuildIndices(assetType);
            GraphicsManager.ClearCaches(assetType);

            DiagManager.Instance.LogInfo("Deleted frames for:" + anim);

        }


        private void Anims_SelectedIndexChanged()
        {
            CachedPath = null;
            if (Anims.InternalIndex == -1)
                return;

            lock (GameBase.lockObj)
            {
                if (DungeonScene.Instance != null)
                {
                    DungeonScene.Instance.DebugAsset = assetType;
                    DungeonScene.Instance.DebugAnim = anims[Anims.InternalIndex];
                }
            }
        }

    }
}
