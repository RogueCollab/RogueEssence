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
    public class CombinedGridRoomStepEditor : Editor<ICombineGridRoomStep>
    {
        public override string GetString(ICombineGridRoomStep obj, Type type, object[] attributes)
        {
            PropertyInfo mergeRateInfo = typeof(ICombineGridRoomStep).GetProperty(nameof(obj.MergeRate));
            return string.Format("{0}[{1}]: Amount:{2}", obj.GetType().GetFormattedTypeName(),
                obj.Combos.Count,
                DataEditor.GetString(obj.MergeRate, mergeRateInfo.GetMemberInfoType(), mergeRateInfo.GetCustomAttributes(false)));
        }
    }
}