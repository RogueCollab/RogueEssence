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
    public class SpreadPlanBaseEditor : Editor<SpreadPlanBase>
    {
        public override string GetString(SpreadPlanBase obj, Type type, object[] attributes)
        {
            MemberInfo floorRangeInfo = type.GetMember("FloorRange")[0];
            return string.Format("{0}F, {1}",
                DataEditor.GetString(obj.FloorRange, floorRangeInfo.GetMemberInfoType(), floorRangeInfo.GetCustomAttributes(false)),
                obj.ToString());
        }
    }
}
