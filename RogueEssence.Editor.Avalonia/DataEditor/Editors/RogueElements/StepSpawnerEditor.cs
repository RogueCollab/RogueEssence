using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using RogueElements;
using Avalonia.Controls;
using RogueEssence.Dev.Views;
using System.Collections;
using Avalonia;
using System.Reactive.Subjects;
using RogueEssence.LevelGen;
using System.Reflection;

namespace RogueEssence.Dev
{
    public class StepSpawnerEditor : Editor<IStepSpawner>
    {
        public override bool DefaultSubgroup => true;

    }

    public class MultiStepSpawnerEditor : Editor<IMultiStepSpawner>
    {
        public override bool DefaultSubgroup => true;


        public override string GetString(IMultiStepSpawner obj, Type type, object[] attributes)
        {
            //TODO: find a way to get member info without using a string literal of the member name
            PropertyInfo memberInfo = type.GetProperty("Picker");
            return string.Format("{0}: {1}", obj.GetType().GetFormattedTypeName(), DataEditor.GetString(obj.Picker, memberInfo.GetMemberInfoType(), memberInfo.GetCustomAttributes(false)));
        }
    }
    public class PickerSpawnerEditor : Editor<IPickerSpawner>
    {
        public override bool DefaultSubgroup => true;


        public override string GetString(IPickerSpawner obj, Type type, object[] attributes)
        {
            //TODO: find a way to get member info without using a string literal of the member name
            PropertyInfo memberInfo = type.GetProperty("Picker");
            return string.Format("{0}: {1}", obj.GetType().GetFormattedTypeName(), DataEditor.GetString(obj.Picker, memberInfo.GetMemberInfoType(), memberInfo.GetCustomAttributes(false)));
        }
    }
    public class ContextSpawnerEditor : Editor<IContextSpawner>
    {
        public override bool DefaultSubgroup => true;


        public override string GetString(IContextSpawner obj, Type type, object[] attributes)
        {
            //TODO: find a way to get member info without using a string literal of the member name
            PropertyInfo memberInfo = type.GetProperty("Amount");
            return string.Format("{0}: {1}", obj.GetType().GetFormattedTypeName(), DataEditor.GetString(obj.Amount, memberInfo.GetMemberInfoType(), memberInfo.GetCustomAttributes(false)));
        }
    }
}
