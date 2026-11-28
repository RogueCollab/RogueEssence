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
using RogueEssence.Dev.Services;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{

    public class TextureLayerBoxViewModel : LayerBoxViewModel<MapLayer>
    {
        private bool groundMode;
        private IDialogService _dialogService;
        public TextureLayerBoxViewModel(bool groundMode, IDialogService dialogService) : base(groundMode ? DiagManager.Instance.DevEditor.GroundEditor.Edits : DiagManager.Instance.DevEditor.MapEditor.Edits)
        {
            _dialogService = dialogService;
            this.groundMode = groundMode;
        }

        public override async Task EditLayer()
        {
            MapLayerWindowViewModel vm = new MapLayerWindowViewModel(Layers[ChosenLayer]);
            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            bool result;
            if (groundMode)
            {
                GroundLayerWindow window = new GroundLayerWindow();
                window.DataContext = vm;

                // result = await window.ShowDialog<bool>(form.GroundEditorPage);
            }
            else
            {
                
                Console.WriteLine(_dialogService);
                bool animResult = await _dialogService.ShowDialogAsync<MapLayerWindowViewModel, bool>(vm, "Map Layer");
                if (!animResult)
                    return;
                // MapLayerWindow window = new MapLayerWindow();
                // window.DataContext = vm;
                // result = true;
                result = true;
                // result = await window.ShowDialog<bool>(form.MapEditPage);
            }

            
            result = false;
            lock (GameBase.lockObj)
            {
                if (result)
                {
                    MapLayer newLayer = new MapLayer(vm.Name);
                    MapLayer oldLayer = Layers[ChosenLayer];
                    newLayer.Layer = vm.Front ? DrawLayer.Top : (vm.Back ? DrawLayer.Under : DrawLayer.Bottom);
                    newLayer.Visible = oldLayer.Visible;
                    newLayer.Tiles = oldLayer.Tiles;

                    if (groundMode)
                        edits.Apply(new GroundTextureStateUndo(ChosenLayer));
                    else
                        edits.Apply(new MapTextureStateUndo(ChosenLayer));

                    SetLayer(ChosenLayer, newLayer);
                }
            }
        }

        protected override MapLayer GetNewLayer()
        {
            MapLayer layer = new MapLayer(String.Format("Layer {0}", Layers.Count));
            if (groundMode)
                layer.CreateNew(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height);
            else
                layer.CreateNew(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height);
            return layer;
        }

        protected override void LoadLayersFromSource()
        {
            if (groundMode)
                Layers.LoadModels(ZoneManager.Instance.CurrentGround.Layers);
            else
                Layers.LoadModels(ZoneManager.Instance.CurrentMap.Layers);
        }
    }

}
