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
        protected UndoStack edits { get; }

        public LayerBoxViewModel(UndoStack stack)
        {
            this.edits = stack;
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

                edits.Apply(new AddLayerUndo<T>(Layers, chosenLayer + 1, layer, false));
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

                    edits.Apply(new AddLayerUndo<T>(Layers, chosenLayer, Layers[chosenLayer], false));

                    ChosenLayer = Math.Min(curLayer, Layers.Count - 1);
                }
            }
        }

        public void DupeLayer()
        {
            lock (GameBase.lockObj)
            {
                T oldLayer = Layers[chosenLayer];

                T layer = (T)oldLayer.Clone();
                edits.Apply(new AddLayerUndo<T>(Layers, chosenLayer + 1, layer, false));
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
                    T newLayer = (T)bottomLayer.Clone();
                    newLayer.Merge(topLayer);

                    edits.Apply(new MergeLayerUndo<T>(Layers, chosenLayer, newLayer));
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
                    edits.Apply(new MoveLayerUndo<T>(Layers, insertLayer));
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
                    edits.Apply(new MoveLayerUndo<T>(Layers, chosenLayer));
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


    public class AddLayerUndo<T> : ReversibleUndo
    {
        private Collection<T> layers;
        private int index;
        private T layer;
        public AddLayerUndo(Collection<T> layers, int index, T layer, bool reversed) : base(reversed)
        {
            this.layers = layers;
            this.index = index;
            this.layer = layer;
        }

        public override void Forward()
        {
            layers.Insert(index + 1, layer);
        }
        public override void Backward()
        {
            layers.RemoveAt(index + 1);
        }
    }

    public class MoveLayerUndo<T> : SymmetricUndo
    {
        private Collection<T> layers;
        private int index;
        public MoveLayerUndo(Collection<T> layers, int index)
        {
            this.layers = layers;
            this.index = index;
        }

        public override void Redo()
        {
            int insertLayer = index + 1;
            T oldLayer = layers[index];
            layers.RemoveAt(index);
            layers.Insert(insertLayer, oldLayer);
        }
    }


    public class MergeLayerUndo<T> : Undoable
        where T : IMapLayer
    {
        private Collection<T> layers;
        private int index;
        private T topLayer;
        private T bottomLayer;
        private T newLayer;

        public MergeLayerUndo(Collection<T> layers, int index, T mergeLayer)
        {
            this.layers = layers;
            this.index = index;
            this.newLayer = mergeLayer;
        }


        public override void Apply()
        {
            int insertLayer = index - 1;
            topLayer = layers[index];
            bottomLayer = layers[insertLayer];
            Redo();
        }

        public override void Redo()
        {
            //merge down
            int insertLayer = index - 1;
            layers.RemoveAt(index);
            layers.RemoveAt(insertLayer);
            layers.Insert(insertLayer, newLayer);
        }

        public override void Undo()
        {
            int insertLayer = index - 1;
            layers.RemoveAt(insertLayer);
            layers.Insert(insertLayer, bottomLayer);
            layers.Insert(index, topLayer);
        }
    }
}
