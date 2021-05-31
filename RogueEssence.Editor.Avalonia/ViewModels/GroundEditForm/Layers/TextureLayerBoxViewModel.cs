using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using RogueElements;
using RogueEssence.Dungeon;
using ReactiveUI;
using RogueEssence.Content;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{

    public class TextureLayerBoxViewModel : LayerBoxViewModel<MapLayer>
    {
        public TextureLayerBoxViewModel(UndoStack edits) : base(edits)
        { }

        public override async Task EditLayer()
        {
            MapLayerWindow window = new MapLayerWindow();
            MapLayerViewModel vm = new MapLayerViewModel(Layers[ChosenLayer]);
            window.DataContext = vm;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            bool result = await window.ShowDialog<bool>(form.GroundEditForm);

            lock (GameBase.lockObj)
            {
                if (result)
                {
                    MapLayer newLayer = new MapLayer(vm.Name);
                    MapLayer oldLayer = Layers[ChosenLayer];
                    newLayer.Layer = vm.Front ? DrawLayer.Top : DrawLayer.Bottom;
                    newLayer.Visible = oldLayer.Visible;
                    newLayer.Tiles = oldLayer.Tiles;

                    edits.Apply(new GroundTextureStateUndo(ChosenLayer));

                    Layers[ChosenLayer] = newLayer;
                }
            }
        }

        protected override MapLayer GetNewLayer()
        {
            MapLayer layer = new MapLayer(String.Format("Layer {0}", Layers.Count));
            layer.CreateNew(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height);
            return layer;
        }

        protected override void LoadLayersFromSource()
        {
            Layers.LoadModels(ZoneManager.Instance.CurrentGround.Layers);
        }
    }

}
