using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Avalonia.Controls;
using ReactiveUI;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev
{
    public static class ReactiveExt
    {
        public static bool SetIfChanged<TObj, TRet>(
            this TObj reactiveObject,
            ref TRet backingField,
            TRet newValue,
            [CallerMemberName] string propertyName = null)
            where TObj : IReactiveObject
        {

            if (EqualityComparer<TRet>.Default.Equals(backingField, newValue))
            {
                return false;
            }
            reactiveObject.RaiseAndSetIfChanged(ref backingField, newValue, propertyName);
            return true;
        }

        public static void RaiseAndSet<TObj, TRet>(
            this TObj reactiveObject,
            ref TRet backingField,
            TRet newValue,
            [CallerMemberName] string propertyName = null)
            where TObj : IReactiveObject
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            backingField = newValue;
            reactiveObject.RaisePropertyChanged(propertyName);
        }

        public static ParentForm GetOwningForm(this Control control)
        {
            while (control.Parent != null)
                control = (Control) control.Parent;
            return (ParentForm)control;
        }
    }
}
