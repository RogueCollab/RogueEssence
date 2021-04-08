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
    public abstract class LayerBoxViewModel<T> : ViewModelBase, ILayerBoxViewModel
        where T : IMapLayer
    {
        public LayerBoxViewModel()
        {
            Layers = new WrappedObservableCollection<T>();
        }

        public WrappedObservableCollection<T> Layers { get; }
        public event Action SelectedLayerChanged;

        private int chosenLayer;
        public int ChosenLayer
        {
            get => chosenLayer;
            set
            {
                this.SetIfChanged(ref chosenLayer, value);
                SelectedLayerChanged?.Invoke();
            }
        }


        protected abstract T GetNewLayer();
        protected abstract void LoadLayersFromSource();

        public abstract Task EditLayer();

        public void AddLayer()
        {
            lock (GameBase.lockObj)
            {
                int curLayer = chosenLayer;
                T layer = GetNewLayer();
                Layers.Insert(chosenLayer + 1, layer);
                ChosenLayer = curLayer + 1;
            }
        }

        public void DeleteLayer()
        {
            lock (GameBase.lockObj)
            {
                if (Layers.Count > 1)
                {
                    int curLayer = chosenLayer;
                    Layers.RemoveAt(chosenLayer);
                    ChosenLayer = Math.Min(curLayer, Layers.Count - 1);
                }
            }
        }

        public void DupeLayer()
        {
            lock (GameBase.lockObj)
            {
                T oldLayer = Layers[chosenLayer];
                Layers.Insert(chosenLayer + 1, (T)oldLayer.Clone());
            }
        }

        public void MergeLayer()
        {
            lock (GameBase.lockObj)
            {
                if (chosenLayer > 0)
                {
                    int insertLayer = chosenLayer - 1;
                    T topLayer = Layers[chosenLayer];
                    T bottomLayer = Layers[insertLayer];
                    Layers.RemoveAt(chosenLayer);
                    Layers.RemoveAt(insertLayer);
                    bottomLayer.Merge(topLayer);
                    Layers.Insert(insertLayer, bottomLayer);
                    ChosenLayer = insertLayer;
                }
            }
        }

        public void MoveLayerUp()
        {
            lock (GameBase.lockObj)
            {
                if (chosenLayer > 0)
                {
                    int insertLayer = chosenLayer - 1;
                    T oldLayer = Layers[chosenLayer];
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
                    T oldLayer = Layers[chosenLayer];
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

            LoadLayersFromSource();
        }
    }

    public interface ILayerBoxViewModel
    {
        event Action SelectedLayerChanged;
        int ChosenLayer { get; set; }
        Task EditLayer();
        void LoadLayers();
    }
}
