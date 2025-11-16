using System;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev
{
    public class TabEvents
    {
        private readonly PageFactory _pageFactory;

        public event Action<EditorPageViewModel, EditorPageViewModel>? AddChildTabEvent;
        public event Action<EditorPageViewModel>? AddTopLevelTabEvent;
        public event Action<EditorPageViewModel>? RemoveTabEvent;
        public event Action<EditorPageViewModel>? NavigateToTabEvent;

        // Navigate tab to ID

        public TabEvents(PageFactory pageFactory)
        {
            _pageFactory = pageFactory;
        }

        public void AddChildPage<T>(EditorPageViewModel parent) where T : EditorPageViewModel
        {
            var child = _pageFactory.CreatePage<T>();
            AddChildTabEvent?.Invoke(parent, child);
        }

        public void AddChildPage(EditorPageViewModel parent, EditorPageViewModel child)
            => AddChildTabEvent?.Invoke(parent, child);

        public void AddTopLevelTab<T>() where T : EditorPageViewModel
        {
            var tab = _pageFactory.CreatePage<T>();
            AddTopLevelTabEvent?.Invoke(tab);
        }

        public void AddTopLevelTab(EditorPageViewModel tab)
            => AddTopLevelTabEvent?.Invoke(tab);


        // TODO: maybe add async callback for closing tab to confirm?
        public void RemoveTab(EditorPageViewModel page)
            => RemoveTabEvent?.Invoke(page);

        // TODO: remove this?
        public void NavigateToTab(EditorPageViewModel page)
            => NavigateToTabEvent?.Invoke(page);

    }
}
