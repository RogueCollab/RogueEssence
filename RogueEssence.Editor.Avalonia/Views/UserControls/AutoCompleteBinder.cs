using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace RogueEssence.Dev.Views
{
    public class AutoCompleteBinder<T> : ReactiveObject
    {
        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        private T _chosenItem;

        public T ChosenItem
        {
            get => _chosenItem;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_chosenItem, value)) return;
                this.RaiseAndSetIfChanged(ref _chosenItem, value);
                AcceptChosenItem(value);
            }
        }

        public IList<T> Items { get; }


        private readonly Action<T> _updateModel;

        public AutoCompleteBinder(IList<T> items, Action<T> updateModel = null)
        {
            Items = items;
            _updateModel = updateModel;
        }
        
        public AutoCompleteBinder(IList<T> items, T initialSelection = default, Action<T> updateModel = null)
        {
            Items = items;
            _updateModel = updateModel;

            if (initialSelection != null)
                ChosenItem = initialSelection;
            
            _searchText = _chosenItem?.ToString() ?? string.Empty;
        }


        private void AcceptChosenItem(T item)
        {
            if (item == null) return;
            _updateModel?.Invoke(item);
            SearchText = item.ToString();
        }


        public void AcceptSearchText()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;

            var match = Items.FirstOrDefault(x =>
                x.ToString().StartsWith(SearchText, StringComparison.InvariantCultureIgnoreCase));

            if (match != null)
                ChosenItem = match;
        }
    }
}
