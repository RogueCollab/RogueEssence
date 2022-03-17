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
    public class AddConnectedRoomsStepEditor : Editor<IAddConnectedRoomsStep>
    {
        public override string GetString(IAddConnectedRoomsStep obj, Type type, object[] attributes)
        {
            //TODO: find a way to get member info without using a string literal of the member name
            PropertyInfo amountInfo = type.GetProperty("Amount");
            PropertyInfo hallInfo = type.GetProperty("HallPercent");
            return string.Format("{0}: Add:{1} Hall:{2}%", obj.GetType().Name,
                DataEditor.GetString(obj.Amount, amountInfo.GetMemberInfoType(), amountInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.HallPercent, hallInfo.GetMemberInfoType(), hallInfo.GetCustomAttributes(false)));
        }
    }

    public class AddDisconnectedRoomsStepEditor : Editor<IAddDisconnectedRoomsStep>
    {
        public override string GetString(IAddDisconnectedRoomsStep obj, Type type, object[] attributes)
        {
            PropertyInfo amountInfo = type.GetProperty("Amount");
            return string.Format("{0}: Add:{1}", obj.GetType().Name,
                DataEditor.GetString(obj.Amount, amountInfo.GetMemberInfoType(), amountInfo.GetCustomAttributes(false)));
        }
    }

    public class ConnectRoomStepEditor : Editor<IConnectRoomStep>
    {
        public override string GetString(IConnectRoomStep obj, Type type, object[] attributes)
        {
            PropertyInfo connectInfo = type.GetProperty("ConnectFactor");
            return string.Format("{0}: {1}%", obj.GetType().Name,
                DataEditor.GetString(obj.ConnectFactor, connectInfo.GetMemberInfoType(), connectInfo.GetCustomAttributes(false)));
        }
    }

    public class FloorPathBranchEditor : Editor<IFloorPathBranch>
    {
        public override string GetString(IFloorPathBranch obj, Type type, object[] attributes)
        {
            PropertyInfo fillInfo = type.GetProperty("FillPercent");
            PropertyInfo hallInfo = type.GetProperty("HallPercent");
            PropertyInfo branchInfo = type.GetProperty("BranchRatio");
            return string.Format("{0}: Fill:{1}% Hall:{2}% Branch:{3}%", obj.GetType().Name,
                DataEditor.GetString(obj.FillPercent, fillInfo.GetMemberInfoType(), fillInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.HallPercent, hallInfo.GetMemberInfoType(), hallInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.BranchRatio, branchInfo.GetMemberInfoType(), branchInfo.GetCustomAttributes(false)));
        }
    }
}
