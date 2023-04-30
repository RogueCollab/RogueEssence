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
            MemberInfo[] floorRangeInfo = type.GetMember(nameof(obj.FloorRange));
            return string.Format("{0}F, {1}",
                DataEditor.GetString(obj.FloorRange, floorRangeInfo[0].GetMemberInfoType(), floorRangeInfo[0].GetCustomAttributes(false)),
                obj.ToString());
        }
    }

    public class SpreadPlanSpacedEditor : Editor<SpreadPlanSpaced>
    {
        public override string GetString(SpreadPlanSpaced obj, Type type, object[] attributes)
        {
            MemberInfo[] floorRangeInfo = type.GetMember(nameof(obj.FloorRange));
            MemberInfo[] spaceInfo = type.GetMember(nameof(obj.FloorSpacing));
            return string.Format("{0}F, Every {1} Floors",
                DataEditor.GetString(obj.FloorRange, floorRangeInfo[0].GetMemberInfoType(), floorRangeInfo[0].GetCustomAttributes(false)),
                DataEditor.GetString(obj.FloorSpacing, spaceInfo[0].GetMemberInfoType(), spaceInfo[0].GetCustomAttributes(false)));
        }
    }

    public class SpreadPlanQuotaEditor : Editor<SpreadPlanQuota>
    {
        public override string GetString(SpreadPlanQuota obj, Type type, object[] attributes)
        {
            MemberInfo[] floorRangeInfo = type.GetMember(nameof(obj.FloorRange));
            MemberInfo[] quotaInfo = type.GetMember(nameof(obj.Quota));
            return string.Format("{0}F, At a quota of {1}",
                DataEditor.GetString(obj.FloorRange, floorRangeInfo[0].GetMemberInfoType(), floorRangeInfo[0].GetCustomAttributes(false)),
                DataEditor.GetString(obj.Quota, quotaInfo[0].GetMemberInfoType(), quotaInfo[0].GetCustomAttributes(false)));
        }
    }
}
