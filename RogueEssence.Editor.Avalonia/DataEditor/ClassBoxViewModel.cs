﻿using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Controls;
using RogueElements;
using System.Collections;

namespace RogueEssence.Dev.ViewModels
{
    public class ClassBoxViewModel : ViewModelBase
    {
        public object Object { get; private set; }
        public string Name { get; private set; }

        public delegate void EditElementOp(object element);
        public delegate void ElementOp(object element, EditElementOp op);

        public event ElementOp OnEditItem;
        public event Action OnMemberChanged;

        public ClassBoxViewModel()
        {

        }

        public T GetObject<T>()
        {
            return (T)Object;
        }

        public void LoadFromSource(object source)
        {
            Object = source;
            Name = DataEditor.GetClassEntryString(Object);
        }

        private void updateSource(object source)
        {
            LoadFromSource(source);
            OnMemberChanged?.Invoke();
        }

        private void btnEdit_Click()
        {
            object element = Object;
            OnEditItem?.Invoke(element, updateSource);
        }
    }
}
