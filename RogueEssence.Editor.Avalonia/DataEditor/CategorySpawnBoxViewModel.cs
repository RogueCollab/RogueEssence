using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Input;
using RogueElements;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{
    public class CategorySpawnElement : ViewModelBase
    {
        private int weight;
        public int Weight
        {
            get { return weight; }
            set
            {
                this.SetIfChanged(ref weight, value);
                DisplayWeight = DisplayWeight;
            }
        }
        public string DisplayWeight
        {
            get { return IntPrefix + weight; }
            set { this.RaisePropertyChanged(); }
        }
        private double chance;
        public double Chance
        {
            get { return chance; }
            set
            {
                this.SetIfChanged(ref chance, value);
                DisplayChance = DisplayChance;
            }
        }
        public string DisplayChance
        {
            get { return IntPrefix + String.Format("{0:0.00}%", (double)chance * 100); }
            set { this.RaisePropertyChanged(); }
        }
        private object val;
        public object Value
        {
            get { return val; }
        }
        public string DisplayValue
        {
            get { return Prefix + conv.GetString(val); }
        }
        public string IntPrefix
        {
            get { return (category ? "" : "  "); }
        }
        public string Prefix
        {
            get { return (category ? "" : "  \u2022"); }
        }

        private StringConv conv;
        private bool category;

        public CategorySpawnElement(StringConv conv, bool category, int weight, double chance, object val)
        {
            this.conv = conv;
            this.category = category;
            this.weight = weight;
            this.chance = chance;
            this.val = val;
        }
    }

    public class CategorySpawnBoxViewModel : ViewModelBase
    {
        public delegate void EditElementOp(int index, object element);
        public delegate void ElementOp(int index, object element, bool advancedEdit, EditElementOp op);

        public event ElementOp OnEditItem;
        public event ElementOp OnEditKey;

        public StringConv CategoryConv;
        public StringConv StringConv;


        private Window parent;

        public bool ConfirmDelete;

        public CategorySpawnBoxViewModel(Window parent, StringConv categoryConv, StringConv conv)
        {
            CategoryConv = categoryConv;
            StringConv = conv;
            this.parent = parent;
            heirarchy = new List<(CategorySpawnElement, List<CategorySpawnElement>)>();
            Collection = new ObservableCollection<CategorySpawnElement>();
        }

        private List<(CategorySpawnElement, List<CategorySpawnElement>)> heirarchy;
        public ObservableCollection<CategorySpawnElement> Collection { get; }

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
                    updatePercentages();
                }
            }
        }

        private void updatePercentages()
        {
            int categoryTotal = 0;
            int[] spawnTotal = new int[heirarchy.Count];
            for (int ii = 0; ii < heirarchy.Count; ii++)
            {
                (CategorySpawnElement, List<CategorySpawnElement>) category = heirarchy[ii];
                categoryTotal += category.Item1.Weight;
                foreach (CategorySpawnElement item in category.Item2)
                    spawnTotal[ii] += item.Weight;
            }
            for (int ii = 0; ii < heirarchy.Count; ii++)
            {
                (CategorySpawnElement, List<CategorySpawnElement>) category = heirarchy[ii];
                category.Item1.Chance = (double)category.Item1.Weight / categoryTotal;
                foreach (CategorySpawnElement item in category.Item2)
                    item.Chance = (double)item.Weight / spawnTotal[ii] * category.Item1.Weight / categoryTotal;
            }
        }

        public ISpawnDict GetDict(Type type)
        {
            Type spawnListType = ReflectionExt.GetBaseTypeArg(typeof(ISpawnDict<,>), type, 1);
            ISpawnDict result = (ISpawnDict)Activator.CreateInstance(type);
            foreach ((CategorySpawnElement, List<CategorySpawnElement>) category in heirarchy)
            {
                object key = category.Item1.Value;
                int rate = category.Item1.Weight;
                ISpawnList spawnList = (ISpawnList)Activator.CreateInstance(spawnListType);
                foreach (CategorySpawnElement item in category.Item2)
                    spawnList.Add(item.Value, item.Weight);
                result.Add(key, spawnList, rate);
            }
            return result;
        }

        public void LoadFromDict(ISpawnDict source)
        {
            heirarchy.Clear();
            Collection.Clear();
            foreach(object key in source.GetKeys())
            {
                ISpawnList list = (ISpawnList)source.GetSpawn(key);
                int rate = source.GetSpawnRate(key);
                CategorySpawnElement categorySpawn = new CategorySpawnElement(CategoryConv, true, rate, 0, key);
                List<CategorySpawnElement> elements = new List<CategorySpawnElement>();
                Collection.Add(categorySpawn);

                for (int ii = 0; ii < list.Count; ii++)
                {
                    object obj = list.GetSpawn(ii);
                    int objrate = list.GetSpawnRate(ii);
                    CategorySpawnElement spawn = new CategorySpawnElement(StringConv, false, objrate, 0, obj);
                    Collection.Add(spawn);
                    elements.Add(spawn);
                }
                heirarchy.Add((categorySpawn, elements));
            }
            updatePercentages();
        }


        private async void editCategory(int index, object element)
        {
            int existingIndex = getCategoryFromKey(element);
            if (existingIndex > -1)
            {
                await MessageBox.Show(parent, "Spawnlist already contains this category!", "Error", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            index = Math.Min(Math.Max(0, index), Collection.Count);
            int categoryIndex = getCategoryIndex(index);
            CategorySpawnElement listElement = new CategorySpawnElement(CategoryConv, true, Collection[index].Weight, Collection[index].Chance, element);
            heirarchy[categoryIndex] = (listElement, heirarchy[categoryIndex].Item2);
            Collection[index] = listElement;
        }

        private void editItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count);
            (List<CategorySpawnElement>, int) listIndex = getHeirarchyListIndex(index);
            CategorySpawnElement listElement = new CategorySpawnElement(StringConv, false, Collection[index].Weight, Collection[index].Chance, element);
            listIndex.Item1[listIndex.Item2] = listElement;
            Collection[index] = listElement;
        }

        private async void insertCategory(int index, object element)
        {
            int existingIndex = getCategoryFromKey(element);
            if (existingIndex > -1)
            {
                await MessageBox.Show(parent, "Spawnlist already contains this category!", "Error", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            index = Math.Min(Math.Max(0, index), Collection.Count + 1);
            int categoryIndex = getCategoryIndex(index);
            //account for inserting while non-selecting
            if (categoryIndex < 0)
                categoryIndex = heirarchy.Count;

            CategorySpawnElement listElement = new CategorySpawnElement(CategoryConv, true, 10, 0, element);
            heirarchy.Insert(categoryIndex, (listElement, new List<CategorySpawnElement>()));
            Collection.Insert(index, listElement);
            updatePercentages();
        }

        private void insertItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count + 1);
            (List<CategorySpawnElement>, int) listIndex = getHeirarchyListIndex(index);
            CategorySpawnElement listElement = new CategorySpawnElement(StringConv, false, 10, 0, element);

            listIndex.Item1.Insert(listIndex.Item2, listElement);
            Collection.Insert(index, listElement);
            updatePercentages();
        }

        public void gridCollection_DoubleClick(object sender, PointerReleasedEventArgs e)
        {
            //int index = lbxCollection.IndexFromPoint(e.X, e.Y);
            int index = CurrentElement;
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            if (index > -1)
            {
                CategorySpawnElement element = Collection[index];
                if (isCategory(index))
                    OnEditKey?.Invoke(index, element.Value, advancedEdit, editCategory);
                else
                    OnEditItem?.Invoke(index, element.Value, advancedEdit, editItem);
            }
        }

        public void btnAddCategory_Click(bool advancedEdit)
        {
            int index = CurrentElement;
            if (index < 0)
                index = Collection.Count;
            object element = null;
            if (index == Collection.Count)
            {
                //we're fine, the insert will handle the edge case
            }
            else if (isCategory(index))
            {
                //we're fine, just insert at the index directly
            }
            else
            {
                //find the index of the owning category
                index = getOwningCategoryIndex(index);
            }
            OnEditKey?.Invoke(index, element, advancedEdit, insertCategory);
        }

        public async void btnAddItem_Click(bool advancedEdit)
        {
            int index = CurrentElement;
            if (index < 0)
                index = Collection.Count;
            
            if (index == Collection.Count)
            {
                await MessageBox.Show(parent, "Choose a category first!", "Error", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            object element = null;
            if (isCategory(index))
            {
                //find the last index of the category
                int categoryIndex = getCategoryIndex(index);
                index += 1 + heirarchy[categoryIndex].Item2.Count;
            }
            else
            {
                //we're fine, just insert at the index directly
            }
            OnEditItem?.Invoke(index, element, advancedEdit, insertItem);
        }

        private async void btnDelete_Click()
        {
            if (CurrentElement > -1 && CurrentElement < Collection.Count)
            {
                if (ConfirmDelete)
                {
                    MessageBox.MessageBoxResult result = await MessageBox.Show(parent, "Are you sure you want to delete this item:\n" + Collection[currentElement].DisplayValue, "Confirm Delete",
                    MessageBox.MessageBoxButtons.YesNo);
                    if (result == MessageBox.MessageBoxResult.No)
                        return;
                }

                if (isCategory(CurrentElement))
                {
                    int categoryIndex = getCategoryIndex(CurrentElement);
                    List<CategorySpawnElement> list = heirarchy[categoryIndex].Item2;
                    Collection.RemoveAt(CurrentElement);
                    for (int ii = 0; ii < list.Count; ii++)
                        Collection.RemoveAt(CurrentElement);
                    heirarchy.RemoveAt(categoryIndex);
                }
                else
                {
                    //find the index of the owning category
                    (List<CategorySpawnElement>, int) listIndex = getHeirarchyListIndex(CurrentElement);
                    listIndex.Item1.RemoveAt(listIndex.Item2);
                    Collection.RemoveAt(CurrentElement);
                }
            }
        }

        private void Switch(int a, int b)
        {
            CategorySpawnElement obj = Collection[a];
            Collection[a] = Collection[b];
            Collection[b] = obj;
        }

        private void SwitchRange(IntRange a, IntRange b)
        {
            CategorySpawnElement[] aCache = new CategorySpawnElement[a.Length];
            for (int ii = 0; ii < a.Length; ii++)
            {
                aCache[ii] = Collection[a.Min + ii];
                Collection[a.Min + ii] = new CategorySpawnElement(StringConv, false, 0, 0, "");
            }
            CategorySpawnElement[] bCache = new CategorySpawnElement[b.Length];
            for (int ii = 0; ii < b.Length; ii++)
            {
                bCache[ii] = Collection[b.Min + ii];
                Collection[b.Min + ii] = new CategorySpawnElement(StringConv, false, 0, 0, "");
            }


            for (int ii = 0; ii < b.Length; ii++)
                Collection[a.Min + ii] = bCache[ii];
            for (int ii = 0; ii < a.Length; ii++)
                Collection[a.Min + b.Length + ii] = aCache[ii];


            //CategorySpawnElement[] aCache = new CategorySpawnElement[a.Length];
            //for (int ii = 0; ii < a.Length; ii++)
            //{
            //    aCache[ii] = Collection[a.Min];
            //    Collection.RemoveAt(a.Min + ii);
            //}
            //for (int ii = 0; ii < a.Length; ii++)
            //    Collection.Insert(a.Min + b.Length + ii, aCache[ii]);
        }

        private void SwitchCategory(int a, int b)
        {
            (CategorySpawnElement, List<CategorySpawnElement>) category = heirarchy[a];
            heirarchy[a] = heirarchy[b];
            heirarchy[b] = category;
        }

        private void btnUp_Click()
        {
            if (isCategory(CurrentElement))
            {
                int categoryIndex = getCategoryIndex(CurrentElement);
                if (categoryIndex > 0)
                {
                    (CategorySpawnElement, List<CategorySpawnElement>) categorytop = heirarchy[categoryIndex - 1];
                    (CategorySpawnElement, List<CategorySpawnElement>) categorybot = heirarchy[categoryIndex];
                    IntRange topRange = new IntRange(CurrentElement - 1 - categorytop.Item2.Count, CurrentElement);
                    IntRange botRange = new IntRange(CurrentElement, CurrentElement + 1 + categorybot.Item2.Count);
                    // move the entire category up with the one above it
                    SwitchCategory(categoryIndex - 1, categoryIndex);
                    SwitchRange(topRange, botRange);
                    CurrentElement = topRange.Min;
                }
            }
            else
            {
                //check against being top element of the category
                (List<CategorySpawnElement>, int) listIndex = getHeirarchyListIndex(CurrentElement);
                if (listIndex.Item2 > 0)
                {
                    int index = CurrentElement;
                    Switch(CurrentElement, CurrentElement - 1);
                    CurrentElement = index - 1;
                }
            }
        }

        private void btnDown_Click()
        {
            if (CurrentElement < 0)
                return;

            if (isCategory(CurrentElement))
            {
                // check against being the last category
                int categoryIndex = getCategoryIndex(CurrentElement);
                if (categoryIndex < heirarchy.Count - 1)
                {
                    (CategorySpawnElement, List<CategorySpawnElement>) categorytop = heirarchy[categoryIndex];
                    (CategorySpawnElement, List<CategorySpawnElement>) categorybot = heirarchy[categoryIndex + 1];
                    IntRange topRange = new IntRange(CurrentElement, CurrentElement + 1 + categorytop.Item2.Count);
                    IntRange botRange = new IntRange(CurrentElement + 1 + categorytop.Item2.Count, CurrentElement + 1 + categorytop.Item2.Count + 1 + categorybot.Item2.Count);
                    // move the entire category down with the one below it
                    SwitchCategory(categoryIndex, categoryIndex + 1);
                    SwitchRange(topRange, botRange);
                    CurrentElement = topRange.Min + botRange.Length;
                }
            }
            else
            {
                // check against being the last element in the category
                (List<CategorySpawnElement>, int) listIndex = getHeirarchyListIndex(CurrentElement);
                if (listIndex.Item2 < listIndex.Item1.Count - 1)
                {
                    int index = CurrentElement;
                    Switch(CurrentElement, CurrentElement + 1);
                    CurrentElement = index + 1;
                }
            }
        }

        /// <summary>
        /// Determines if the index points to a category
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool isCategory(int index)
        {
            CategorySpawnElement element = Collection[index];
            for (int ii = 0; ii < heirarchy.Count; ii++)
            {
                if (heirarchy[ii].Item1 == element)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get the index in the heirarchy given an index in collection
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int getCategoryIndex(int index)
        {
            if (index < 0 || index >= Collection.Count)
                return -1;

            CategorySpawnElement element = Collection[index];
            for (int ii = 0; ii < heirarchy.Count; ii++)
            {
                if (heirarchy[ii].Item1 == element)
                    return ii;
            }
            return -1;
        }

        private int getCategoryFromKey(object obj)
        {
            for (int ii = 0; ii < heirarchy.Count; ii++)
            {
                (CategorySpawnElement, List<CategorySpawnElement>) category = heirarchy[ii];
                if (category.Item1.Value.Equals(obj))
                    return ii;
            }
            return -1;
        }

        /// <summary>
        /// Gets the Collections index of the category owning the input Collections index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int getOwningCategoryIndex(int index)
        {
            //go backwards until the item is found in heirarchy
            for (int ii = index; ii >= 0; ii--)
            {
                if (isCategory(ii))
                    return ii;
            }
            return -1;
        }

        /// <summary>
        /// Get the index in the heirarchy given an index in collection.  If choosing a category itself, return the length of the list as the index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private (List<CategorySpawnElement>, int) getHeirarchyListIndex(int index)
        {
            int categoryIndex;
            //go backwards until the item is found in heirarchy
            for (int ii = index - 1; ii >= 0; ii--)
            {
                categoryIndex = getCategoryIndex(ii);
                if (categoryIndex > -1)
                    return (heirarchy[categoryIndex].Item2, index - ii - 1);
            }
            return (null, -1);
        }
    }
}
