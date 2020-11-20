using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.IO;
using RogueEssence;
using RogueEssence.Dev;
using Microsoft.Xna.Framework;
using Avalonia.Threading;
using System.Threading;
using RogueEssence.Data;
using RogueEssence.Content;
using System.Collections.Generic;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public class GroundEditForm : Window, IGroundEditor
    {

        public enum EntEditMode
        {
            SelectEntity = 0,
            PlaceEntity = 1,
            MoveEntity = 2,
        }

        public bool Active { get; private set; }

        private List<string> objectAnimIndex;

        public bool ShowDataLayer;

        private EntEditMode EntMode;


        public GroundEditForm()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif


            //SetupLayerVisibility();

            //InitializeComponent();

            //lbxLayers.LoadFromList(ZoneManager.Instance.CurrentGround.Layers, IsLayerChecked);
            //tileBrowser.TileSize = tileBrowser.TileSize;

            //UpdateHasScriptFolder();

            //saveMapFileDialog.Filter = "map files (*" + DataManager.GROUND_EXT + ")|*" + DataManager.GROUND_EXT;

            //for (int ii = 0; ii < (int)GroundEntity.EEntTypes.Count; ii++)
            //    cmbEntityType.Items.Add(((GroundEntity.EEntTypes)ii).ToLocal());

            //for (int ii = 0; ii <= (int)Gender.Female; ii++)
            //    cmbEntCharGender.Items.Add(((Gender)ii).ToLocal());

            //string[] names = DataManager.Instance.DataIndices[DataManager.DataType.Monster].GetLocalStringArray();
            //for (int ii = 0; ii < names.Length; ii++)
            //    cmbEntKind.Items.Add(ii + " - " + names[ii]);

            //foreach (string s in TemplateManager.TemplateTypeNames)
            //    cmbTemplateType.Items.Add(s);

            //foreach (LuaEngine.EMapCallbacks v in LuaEngine.EnumerateCallbackTypes())
            //    chklstScriptMapCallbacks.Items.Add(v.ToString());


            //for (int ii = 0; ii <= (int)Map.ScrollEdge.Clamp; ii++)
            //    cbScrollEdge.Items.Add(((Map.ScrollEdge)ii).ToLocal());


            //tabctrlEntData.TabPages.Clear();
            //cmbEntityType.SelectedIndex = 0;
            //cmbEntCharGender.SelectedIndex = 0;
            //cmbEntKind.SelectedIndex = 0;
            //cmbTemplateType.SelectedIndex = 0;
            //cmbSpawnerType.SelectedIndex = 0;

            //objectAnimIndex = new List<string>();

            //objectAnimIndex.Add("");
            //cbEntObjSpriteID.Items.Add("---");

            //string[] dirs = Directory.GetFiles(DiagManager.CONTENT_PATH + "Object/");

            //for (int ii = 0; ii < dirs.Length; ii++)
            //{
            //    string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
            //    cbEntObjSpriteID.Items.Add(filename);
            //    objectAnimIndex.Add(filename);
            //}
            //cbEntObjSpriteID.SelectedIndex = 0;


            //ReloadDirections();
            //cmbEntityDir.SelectedIndex = 0;

            //selectedEntity = null;

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        public void ProcessInput(InputManager input)
        {
            GroundEditViewModel vm = (GroundEditViewModel)DataContext;
            DevForm.ExecuteOrInvoke(() => vm.ProcessInput(input));
        }




        public void Window_Loaded(object sender, EventArgs e)
        {
            Active = true;
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            Active = false;
            GameManager.Instance.SceneOutcome = exitGroundEdit();
        }


        private IEnumerator<YieldInstruction> exitGroundEdit()
        {
            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            form.GroundEditForm = null;

            //move to the previous scene or the title, if there was none
            if (DataManager.Instance.Save != null && DataManager.Instance.Save.NextDest.IsValid())
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.Save.NextDest));
            else
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.RestartToTitle());
        }
    }
}
