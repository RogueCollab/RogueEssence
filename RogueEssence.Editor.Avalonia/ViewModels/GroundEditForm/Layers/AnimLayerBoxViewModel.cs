using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using ReactiveUI;
using RogueEssence.Content;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{

    public class AnimLayerBoxViewModel : LayerBoxViewModel<AnimLayer>
    {
        private bool groundMode;
        public AnimLayerBoxViewModel(bool groundMode) : base(groundMode ? DiagManager.Instance.DevEditor.GroundEditor.Edits : DiagManager.Instance.DevEditor.MapEditor.Edits)
        {
            this.groundMode = groundMode;
        }
        public override async Task EditLayer()
        {
            AnimLayerViewModel vm = new AnimLayerViewModel(Layers[ChosenLayer]);
            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            bool result;
            if (groundMode)
            {
                AnimLayerWindow window = new AnimLayerWindow();
                window.DataContext = vm;

                result = await window.ShowDialog<bool>(form.GroundEditForm);
            }
            else
            {
                AnimLayerWindow window = new AnimLayerWindow();
                window.DataContext = vm;

                result = await window.ShowDialog<bool>(form.MapEditForm);
            }

            lock (GameBase.lockObj)
            {
                if (result)
                {
                    AnimLayer newLayer = new AnimLayer(vm.Name);
                    AnimLayer oldLayer = Layers[ChosenLayer];
                    newLayer.Layer = vm.Front ? DrawLayer.Top : DrawLayer.Bottom;
                    newLayer.Visible = oldLayer.Visible;
                    newLayer.Anims = oldLayer.Anims;

                    if (groundMode)
                        edits.Apply(new GroundDecorationStateUndo(ChosenLayer));
                    else
                        edits.Apply(new MapDecorationStateUndo(ChosenLayer));

                    SetLayer(ChosenLayer, newLayer);
                }
            }
        }

        protected override AnimLayer GetNewLayer()
        {
            AnimLayer layer = new AnimLayer(String.Format("Deco {0}", Layers.Count));
            return layer;
        }

        protected override void LoadLayersFromSource()
        {
            if (groundMode)
                Layers.LoadModels(ZoneManager.Instance.CurrentGround.Decorations);
            else
                Layers.LoadModels(ZoneManager.Instance.CurrentMap.Decorations);
        }
    }
}
