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
            PropertyInfo fillInfo = typeof(IGridPathCircle).GetProperty(nameof(obj.CircleRoomRatio));
            PropertyInfo pathsInfo = typeof(IGridPathCircle).GetProperty(nameof(obj.Paths));
            return string.Format("{0}: Fill:{1}% Paths:{2}%", obj.GetType().GetFormattedTypeName(),
                DataEditor.GetString(obj.CircleRoomRatio, fillInfo.GetMemberInfoType(), fillInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.Paths, pathsInfo.GetMemberInfoType(), pathsInfo.GetCustomAttributes(false)));
        }
    }

    public class GridPathBranchEditor : Editor<IGridPathBranch>
    {
        public override string GetString(IGridPathBranch obj, Type type, object[] attributes)
        {
            PropertyInfo fillInfo = typeof(IGridPathBranch).GetProperty(nameof(obj.RoomRatio));
            PropertyInfo branchInfo = typeof(IGridPathBranch).GetProperty(nameof(obj.BranchRatio));
            return string.Format("{0}: Fill:{1}% Branch:{2}%", obj.GetType().GetFormattedTypeName(),
                DataEditor.GetString(obj.RoomRatio, fillInfo.GetMemberInfoType(), fillInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.BranchRatio, branchInfo.GetMemberInfoType(), branchInfo.GetCustomAttributes(false)));
        }
    }

    public class GridPathGridEditor : Editor<IGridPathGrid>
    {
        public override string GetString(IGridPathGrid obj, Type type, object[] attributes)
        {
            PropertyInfo fillInfo = typeof(IGridPathGrid).GetProperty(nameof(obj.RoomRatio));
            PropertyInfo branchInfo = typeof(IGridPathGrid).GetProperty(nameof(obj.HallRatio));
            return string.Format("{0}: Fill:{1}% Branch:{2}%", obj.GetType().GetFormattedTypeName(),
                DataEditor.GetString(obj.RoomRatio, fillInfo.GetMemberInfoType(), fillInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.HallRatio, branchInfo.GetMemberInfoType(), branchInfo.GetCustomAttributes(false)));
        }
        
        public override string GetTypeString()
        {
            return "Grid Path Crossroads";
        }
    }
}
