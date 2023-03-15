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
    public class GridPathCircleEditor : Editor<IGridPathCircle>
    {
        public override string GetString(IGridPathCircle obj, Type type, object[] attributes)
        {
            PropertyInfo fillInfo = type.GetProperty("CircleRoomRatio");
            PropertyInfo pathsInfo = type.GetProperty("Paths");
            return string.Format("{0}: Fill:{1}% Paths:{2}%", obj.GetType().GetFormattedTypeName(),
                DataEditor.GetString(obj.CircleRoomRatio, fillInfo.GetMemberInfoType(), fillInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.Paths, pathsInfo.GetMemberInfoType(), pathsInfo.GetCustomAttributes(false)));
        }
    }

    public class GridPathBranchEditor : Editor<IGridPathBranch>
    {
        public override string GetString(IGridPathBranch obj, Type type, object[] attributes)
        {
            PropertyInfo fillInfo = type.GetProperty("RoomRatio");
            PropertyInfo branchInfo = type.GetProperty("BranchRatio");
            return string.Format("{0}: Fill:{1}% Branch:{2}%", obj.GetType().GetFormattedTypeName(),
                DataEditor.GetString(obj.RoomRatio, fillInfo.GetMemberInfoType(), fillInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.BranchRatio, branchInfo.GetMemberInfoType(), branchInfo.GetCustomAttributes(false)));
        }
    }
}
