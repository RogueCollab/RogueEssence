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

    public class EntityLayerBoxViewModel : LayerBoxViewModel<EntityLayer>
    {
        public EntityLayerBoxViewModel(UndoStack edits) : base(edits)
        { }
        public override async Task EditLayer()
        {
            EntityLayerWindow window = new EntityLayerWindow();
            EntityLayerViewModel vm = new EntityLayerViewModel(Layers[ChosenLayer]);
            window.DataContext = vm;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            bool result = await window.ShowDialog<bool>(form.GroundEditForm);

            lock (GameBase.lockObj)
            {
                if (result)
                {
                    EntityLayer newLayer = new EntityLayer(vm.Name);
                    EntityLayer oldLayer = Layers[ChosenLayer];
                    newLayer.Visible = oldLayer.Visible;
                    newLayer.GroundObjects = oldLayer.GroundObjects;
                    newLayer.Markers = oldLayer.Markers;
                    newLayer.MapChars = oldLayer.MapChars;
                    newLayer.Spawners = oldLayer.Spawners;
                    newLayer.TemporaryChars = oldLayer.TemporaryChars;

                    edits.Apply(new GroundEntityStateUndo(ChosenLayer));

                    Layers[ChosenLayer] = newLayer;
                }
            }
        }

        protected override EntityLayer GetNewLayer()
        {
            EntityLayer layer = new EntityLayer(String.Format("EntLayer {0}", Layers.Count));
            return layer;
        }

        protected override void LoadLayersFromSource()
        {
            Layers.LoadModels(ZoneManager.Instance.CurrentGround.Entities);
        }
    }

    public class GroundEntityStateUndo : StateUndo<EntityLayer>
    {
        private int layer;
        public GroundEntityStateUndo(int layer)
        {
            this.layer = layer;
        }

        public override EntityLayer GetState()
        {
            ZoneManager.Instance.CurrentGround.PreSaveEntLayer(layer);
            return ZoneManager.Instance.CurrentGround.Entities[layer];
        }

        public override void SetState(EntityLayer state)
        {
            ZoneManager.Instance.CurrentGround.Entities[layer] = state;
            ZoneManager.Instance.CurrentGround.ReloadEntLayer(layer);
        }
    }
}
