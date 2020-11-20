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
    public class LayerBoxViewModel : ViewModelBase
    {
        public LayerBoxViewModel()
        {
            Layers = new WrappedObservableCollection<MapLayer>();
        }

        public WrappedObservableCollection<MapLayer> Layers { get; }

        private int chosenLayer;
        public int ChosenLayer
        {
            get => chosenLayer;
            set => this.SetIfChanged(ref chosenLayer, value);
        }


        public async Task EditLayer()
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
                    newLayer.Front = vm.Front;
                    newLayer.Visible = Layers[ChosenLayer].Visible;
                    newLayer.Tiles = Layers[ChosenLayer].Tiles;
                    Layers[ChosenLayer] = newLayer;
                }
            }
        }

        public void AddLayer()
        {
            lock (GameBase.lockObj)
            {
                MapLayer layer = new MapLayer(String.Format("Layer {0}", Layers.Count));
                layer.CreateNew(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height);
                Layers.Add(layer);
            }
        }

        public void DeleteLayer()
        {
            lock (GameBase.lockObj)
            {
                if (Layers.Count > 1)
                    Layers.RemoveAt(chosenLayer);
            }
        }

        public void DupeLayer()
        {
            lock (GameBase.lockObj)
            {
                MapLayer oldLayer = Layers[chosenLayer];
                Layers.Insert(chosenLayer + 1, oldLayer.Clone());
            }
        }

        //public void MergeLayer()
        //{

        //}

        public void MoveLayerUp()
        {
            lock (GameBase.lockObj)
            {
                if (chosenLayer > 0)
                {
                    int insertLayer = chosenLayer - 1;
                    MapLayer oldLayer = Layers[chosenLayer];
                    Layers.RemoveAt(chosenLayer);
                    Layers.Insert(insertLayer, oldLayer);
                    ChosenLayer = insertLayer;
                }
            }
        }

        public void MoveLayerDown()
        {
            lock (GameBase.lockObj)
            {
                if (chosenLayer < Layers.Count - 1)
                {
                    int insertLayer = chosenLayer + 1;
                    MapLayer oldLayer = Layers[chosenLayer];
                    Layers.RemoveAt(chosenLayer);
                    Layers.Insert(insertLayer, oldLayer);
                    ChosenLayer = insertLayer;
                }
            }
        }

        public void LoadLayers()
        {
            if (Design.IsDesignMode)
                return;
            Layers.LoadModels(ZoneManager.Instance.CurrentGround.Layers);
        }
    }
}
