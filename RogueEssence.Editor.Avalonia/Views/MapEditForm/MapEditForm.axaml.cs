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
    public class MapEditForm : ParentForm, IMapEditor
    {

        public bool Active { get; private set; }

        public MapEditForm()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif


        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        public void ProcessInput(InputManager input)
        {
            MapEditViewModel vm = (MapEditViewModel)DataContext;
            DevForm.ExecuteOrInvoke(() => vm.ProcessInput(input));
        }




        public void Window_Loaded(object sender, EventArgs e)
        {
            Active = true;
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            Active = false;
            CloseChildren();
            GameManager.Instance.SceneOutcome = exitMapEdit();
        }


        private IEnumerator<YieldInstruction> exitMapEdit()
        {
            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            form.MapEditForm = null;

            //move to the previous scene or the title, if there was none
            if (DataManager.Instance.Save != null && DataManager.Instance.Save.NextDest.IsValid())
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.Save.NextDest));
            else
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.RestartToTitle());
        }
    }
}
