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
    public class BeamEditViewModel : ViewModelBase
    {
        private GraphicsManager.AssetType assetType;
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


        public BeamEditViewModel()
        {
            anims = new List<string>();

            Anims = new SearchListBoxViewModel();
            Anims.DataName = "Graphics:";
            //TODO: properly support beam/column anims
            //Anims.SelectedIndexChanged += Anims_SelectedIndexChanged;

        }

        public void LoadDataEntries(GraphicsManager.AssetType assetType, Window parent)
        {
            this.assetType = assetType;
            this.parent = parent;

            //Anims.DataName = assetType.ToString() + ":";
            recomputeAnimList();
        }

        private void recomputeAnimList()
        {
            lock (GameBase.lockObj)
            {
                anims.Clear();
                Anims.Clear();
                string assetPattern = GraphicsManager.GetPattern(assetType);
                string[] dirs = PathMod.GetModFiles(Path.GetDirectoryName(assetPattern), String.Format(Path.GetFileName(assetPattern), "*"));
                for (int ii = 0; ii < dirs.Length; ii++)
                {
                    string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                    anims.Add(filename);
                    Anims.AddItem(filename);
                }
            }
        }

        public async void mnuMassImport_Click()
        {
            await MessageBox.Show(parent, "Note: Importing a sprite to a slot that is already filled will automatically overwrite the old one.", "Mass Import", MessageBox.MessageBoxButtons.Ok);

            //remember addresses in registry
            string folderName = DevForm.GetConfig(Name + "Dir", Directory.GetCurrentDirectory());

            //open window to choose directory
            OpenFolderDialog openFileDialog = new OpenFolderDialog();
            openFileDialog.Directory = folderName;

            string folder = await openFileDialog.ShowAsync(parent);

            if (!String.IsNullOrEmpty(folder))
            {
                DevForm.SetConfig(Name + "Dir", folder);
                CachedPath = folder + "/";

                try
                {
                    lock (GameBase.lockObj)
                        MassImport(CachedPath);
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex, false);
                    await MessageBox.Show(parent, "Error importing from\n" + CachedPath + "\n\n" + ex.Message, "Import Failed", MessageBox.MessageBoxButtons.Ok);
                    return;
                }
            }
        }

        public async void mnuMassExport_Click()
        {
            //remember addresses in registry
            string folderName = DevForm.GetConfig(Name + "Dir", Directory.GetCurrentDirectory());

            //open window to choose directory
            OpenFolderDialog openFileDialog = new OpenFolderDialog();
            openFileDialog.Directory = folderName;

            string folder = await openFileDialog.ShowAsync(parent);

            if (!String.IsNullOrEmpty(folder))
            {
                DevForm.SetConfig(Name + "Dir", folder);
                CachedPath = folder + "/";

                bool success = MassExport(CachedPath);
                if (!success)
                    await MessageBox.Show(parent, "Errors found exporting to\n" + CachedPath + "\n\nCheck logs for more info.", "Mass Export Failed", MessageBox.MessageBoxButtons.Ok);
            }
        }

        public async void btnImport_Click()
        {
            //remember addresses in registry
            string folderName = DevForm.GetConfig(Name + "Dir", Directory.GetCurrentDirectory());

            //open window to choose directory
            OpenFolderDialog openFileDialog = new OpenFolderDialog();
            openFileDialog.Directory = folderName;

            string folder = await openFileDialog.ShowAsync(parent);

            if (!String.IsNullOrEmpty(folder))
            {
                string animName = Path.GetFileNameWithoutExtension(folder);

                if (anims.Contains(animName))
                {
                    MessageBox.MessageBoxResult result = await MessageBox.Show(parent, "Are you sure you want to overwrite the existing sheet:\n" + animName, "Sprite Sheet already exists.",
                        MessageBox.MessageBoxButtons.YesNo);
                    if (result == MessageBox.MessageBoxResult.No)
                        return;
                }

                DevForm.SetConfig(Name + "Dir", Path.GetDirectoryName(folder));
                CachedPath = folder;

                try
                {
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

            //open window to choose directory
            OpenFolderDialog openFileDialog = new OpenFolderDialog();
            openFileDialog.Directory = folderName;

            string folder = await openFileDialog.ShowAsync(parent);

            if (!String.IsNullOrEmpty(folder))
            {
                DevForm.SetConfig(Name + "Dir", Path.GetDirectoryName(folder));
                //CachedPath = folder;

                try
                {
                    DevForm.ExecuteOrPend(() => { Export(folder, animData); });
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex, false);
                    await MessageBox.Show(parent, "Error exporting to\n" + CachedPath + "\n\n" + ex.Message, "Export Failed", MessageBox.MessageBoxButtons.Ok);
                    return;
                }
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

            Delete(animIdx);
        }



        private void MassImport(string currentPath)
        {
            string assetPattern = GraphicsManager.GetPattern(assetType);
            if (!Directory.Exists(Path.GetDirectoryName(PathMod.HardMod(assetPattern))))
                Directory.CreateDirectory(Path.GetDirectoryName(PathMod.HardMod(assetPattern)));
            ImportHelper.ImportAllBeams(currentPath, PathMod.HardMod(assetPattern));

            GraphicsManager.RebuildIndices(assetType);
            GraphicsManager.ClearCaches(assetType);

            DiagManager.Instance.LogInfo("Mass import complete.");

            //recompute
            recomputeAnimList();
        }


        private void Import(string currentPath)
        {
            DevForm.ExecuteOrPend(() => { tryImport(currentPath); });

            //recompute
            recomputeAnimList();
        }

        private void tryImport(string currentPath)
        {
            lock (GameBase.lockObj)
            {
                string animName = Path.GetFileNameWithoutExtension(currentPath);
                string destFile = PathMod.HardMod(String.Format(GraphicsManager.GetPattern(assetType), animName));
                if (!Directory.Exists(Path.GetDirectoryName(destFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(destFile));

                //write sprite data
                using (BeamSheet sheet = BeamSheet.Import(currentPath + "/"))
                {
                    using (FileStream stream = File.OpenWrite(destFile))
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                            sheet.Save(writer);
                    }
                }

                GraphicsManager.RebuildIndices(assetType);
                GraphicsManager.ClearCaches(assetType);

                DiagManager.Instance.LogInfo("Frames from:\n" +
                    currentPath + "\nhave been imported.");
            }
        }


        private bool MassExport(string currentPath)
        {
            bool success = true;
            string assetPattern = GraphicsManager.GetPattern(assetType);
            string[] dirs = PathMod.GetModFiles(Path.GetDirectoryName(assetPattern), String.Format(Path.GetFileName(assetPattern), "*"));
            for (int ii = 0; ii < dirs.Length; ii++)
            {
                try
                {
                    string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                    DevForm.ExecuteOrPend(() => { Export(currentPath + filename, filename); });
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex, false);
                    success = false;
                }
            }
            return success;
        }

        private void Export(string currentPath, string anim)
        {
            lock (GameBase.lockObj)
            {
                if (!Directory.Exists(currentPath))
                    Directory.CreateDirectory(currentPath);

                string animPath = PathMod.ModPath(String.Format(GraphicsManager.GetPattern(assetType), anim));
                if (File.Exists(animPath))
                {
                    //read file and read binary data
                    using (FileStream fileStream = File.OpenRead(animPath))
                    {
                        using (BinaryReader reader = new BinaryReader(fileStream))
                        {
                            BeamSheet sheet = BeamSheet.Load(reader);
                            BeamSheet.Export(sheet, currentPath + "/");
                        }
                    }
                }

                DiagManager.Instance.LogInfo("Frames from:\n" +
                    anim +
                    "\nhave been exported to:" + currentPath);
            }
        }


        private void Delete(int animIdx)
        {
            lock (GameBase.lockObj)
            {
                string anim = anims[animIdx];

                string animPath = PathMod.ModPath(String.Format(GraphicsManager.GetPattern(assetType), anim));
                if (File.Exists(animPath))
                    File.Delete(animPath);

                GraphicsManager.RebuildIndices(assetType);
                GraphicsManager.ClearCaches(assetType);

                DiagManager.Instance.LogInfo("Deleted frames for:" + anim);

                anims.RemoveAt(animIdx);
                Anims.RemoveInternalAt(animIdx);
            }
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
