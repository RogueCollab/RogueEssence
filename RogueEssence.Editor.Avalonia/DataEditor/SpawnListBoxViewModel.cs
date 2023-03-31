using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Controls;
using RogueElements;

namespace RogueEssence.Dev.ViewModels
{
    public class SpawnListElement : ViewModelBase
    {
        private int weight;
        public int Weight
        {
            get { return weight; }
            set { this.SetIfChanged(ref weight, value); }
        }
        private double chance;
        public double Chance
        {
            get { return chance; }
            set { this.SetIfChanged(ref chance, value); }
        }
        private object val;
        public object Value
        {
            get { return val; }
        }
        public string DisplayValue
        {
            get { return conv.GetString(val); }
        }

        private StringConv conv;

        public SpawnListElement(StringConv conv, int weight, double chance, object val)
        {
            this.conv = conv;
            this.weight = weight;
            this.chance = chance;
            this.val = val;
        }
    }

    public class SpawnListBoxViewModel : ViewModelBase
    {
        public delegate void EditElementOp(int index, object element);
        public delegate void ElementOp(int index, object element, EditElementOp op);

        public event ElementOp OnEditItem;

        public StringConv StringConv;

        public SpawnListBoxViewModel(StringConv conv)
        {
            StringConv = conv;
            Collection = new ObservableCollection<SpawnListElement>();
        }

        public ObservableCollection<SpawnListElement> Collection { get; }

        private int currentElement;
        public int CurrentElement
        {
            get { return currentElement; }
            set
            {
                this.SetIfChanged(ref currentElement, value);
                if (currentElement > -1)
                    CurrentWeight = Collection[currentElement].Weight;
                else
                    CurrentWeight = 1;
            }
        }

        private int currentWeight;
        public int CurrentWeight
        {
            get { return currentWeight; }
            set
            {
                this.SetIfChanged(ref currentWeight, value);
                if (currentElement > -1)
                {
                    Collection[currentElement].Weight = currentWeight;

                    int spawnTotal = 0;
                    foreach (SpawnListElement curSpawn in Collection)
                        spawnTotal += curSpawn.Weight;
                    foreach (SpawnListElement curSpawn in Collection)
                        curSpawn.Chance = (double)curSpawn.Weight / spawnTotal;
                }
            }
        }

        public ISpawnList GetList(Type type)
        {
            ISpawnList result = (ISpawnList)Activator.CreateInstance(type);
            foreach (SpawnListElement item in Collection)
                result.Add(item.Value, item.Weight);
            return result;
        }

        public void LoadFromList(ISpawnList source)
        {
            Collection.Clear();
            for (int ii = 0; ii < source.Count; ii++)
            {
                object obj = source.GetSpawn(ii);
                int rate = source.GetSpawnRate(ii);
                Collection.Add(new SpawnListElement(StringConv, rate, (double)rate / source.SpawnTotal, obj));
            }
        }

        public void LoadFromTupleList(List<(object, double)> source)
        {
            Collection.Clear();

            for (int ii = 0; ii < source.Count; ii++)
            {
                object obj = source[ii].Item1;
                double rate = source[ii].Item2;
                Collection.Add(new SpawnListElement(StringConv, (int)(rate * 1000000), rate, obj));
            }
        }


        private void editItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count);
            Collection[index] = new SpawnListElement(StringConv, Collection[index].Weight, Collection[index].Chance, element);
            CurrentElement = index;
        }

        private void insertItem(int index, object element)
        {
            int newWeight = 10;

            int spawnTotal = 0;
            foreach (SpawnListElement curSpawn in Collection)
                spawnTotal += curSpawn.Weight;

            spawnTotal += newWeight;
            foreach (SpawnListElement curSpawn in Collection)
                curSpawn.Chance = (double)curSpawn.Weight / spawnTotal;

            index = Math.Min(Math.Max(0, index), Collection.Count + 1);
            Collection.Insert(index, new SpawnListElement(StringConv, newWeight, (double)newWeight / spawnTotal, element));
            CurrentElement = index;
        }

        public void InsertOnKey(int index, object element)
        {
            int newWeight = 10;
            if (0 <= index && index < Collection.Count)
            {
                newWeight = Collection[index].Weight;
            }

            int spawnTotal = 0;
            foreach (SpawnListElement curSpawn in Collection)
                spawnTotal += curSpawn.Weight;

            spawnTotal += newWeight;
            foreach (SpawnListElement curSpawn in Collection)
                curSpawn.Chance = (double)curSpawn.Weight / spawnTotal;

            index = Math.Min(Math.Max(0, index), Collection.Count + 1);
            Collection.Insert(index, new SpawnListElement(StringConv, newWeight, (double)newWeight / spawnTotal, element));
            CurrentElement = index;
        }

        public void gridCollection_DoubleClick(object sender, RoutedEventArgs e)
        {
            //int index = lbxCollection.IndexFromPoint(e.X, e.Y);
            int index = CurrentElement;
            if (index > -1)
            {
                SpawnListElement element = Collection[index];
                OnEditItem?.Invoke(index, element.Value, editItem);
            }
        }


        private void btnAdd_Click()
        {
            int index = CurrentElement;
            if (index < 0)
                index = Collection.Count;
            object element = null;
            OnEditItem?.Invoke(index, element, insertItem);
        }

        private void btnDelete_Click()
        {
            if (CurrentElement > -1 && CurrentElement < Collection.Count)
                Collection.RemoveAt(CurrentElement);
        }

        private void Switch(int a, int b)
        {
            SpawnListElement obj = Collection[a];
            Collection[a] = Collection[b];
            Collection[b] = obj;
        }

        private void btnUp_Click()
        {
            if (CurrentElement > 0)
            {
                int index = CurrentElement;
                Switch(CurrentElement, CurrentElement - 1);
                CurrentElement = index - 1;
            }
        }

        private void btnDown_Click()
        {
            if (CurrentElement > -1 && CurrentElement < Collection.Count - 1)
            {
                int index = CurrentElement;
                Switch(CurrentElement, CurrentElement + 1);
                CurrentElement = index + 1;
            }
        }

    }
}
