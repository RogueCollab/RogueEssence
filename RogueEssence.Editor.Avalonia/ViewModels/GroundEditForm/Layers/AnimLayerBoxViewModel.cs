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
        public override async Task EditLayer()
        {
            AnimLayerWindow window = new AnimLayerWindow();
            AnimLayerViewModel vm = new AnimLayerViewModel(Layers[ChosenLayer]);
            window.DataContext = vm;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            bool result = await window.ShowDialog<bool>(form.GroundEditForm);

            lock (GameBase.lockObj)
            {
                if (result)
                {
                    AnimLayer newLayer = new AnimLayer(vm.Name);
                    AnimLayer oldLayer = Layers[ChosenLayer];
                    newLayer.Layer = vm.Front ? DrawLayer.Top : DrawLayer.Bottom;
                    newLayer.Visible = oldLayer.Visible;
                    newLayer.Anims = oldLayer.Anims;
                    Layers[ChosenLayer] = newLayer;
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
            Layers.LoadModels(ZoneManager.Instance.CurrentGround.Decorations);
        }
    }

}
