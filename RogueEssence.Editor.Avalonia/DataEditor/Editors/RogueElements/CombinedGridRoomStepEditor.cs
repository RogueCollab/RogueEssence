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
    public class CombinedGridRoomStepEditor : Editor<ICombinedGridRoomStep>
    {
        public override string GetString(ICombinedGridRoomStep obj, Type type, object[] attributes)
        {
            PropertyInfo mergeRateInfo = typeof(ICombinedGridRoomStep).GetProperty(nameof(obj.MergeRate));
            PropertyInfo combosInfo = typeof(ICombinedGridRoomStep).GetProperty(nameof(obj.Combos)).Count;
            return string.Format("{0}[{1}]: Amount:{2}", obj.GetType().GetFormattedTypeName(),
                DataEditor.GetString(obj.MergeRate, mergeRateInfo.GetMemberInfoType(), mergeRateInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.Combos.Count, combosInfo.GetMemberInfoType(), combosInfo.GetCustomAttributes(false)));
        }
    }
}