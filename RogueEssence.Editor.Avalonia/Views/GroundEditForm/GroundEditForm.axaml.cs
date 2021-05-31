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
    public class GroundEditForm : ParentForm, IGroundEditor
    {

        public bool Active { get; private set; }

        // TODO: make undo/redo enabled/disabled based on if there's anything left on the stack
        public UndoStack Edits { get; }
        public GroundEditForm()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Edits = new UndoStack();

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

        private bool silentClose;
        public void SilentClose()
        {
            silentClose = true;
            Close();
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            Active = false;
            CloseChildren();
            if (!silentClose)
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
