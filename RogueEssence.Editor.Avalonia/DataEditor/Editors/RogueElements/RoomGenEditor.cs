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
    public class SizedRoomGenEditor : Editor<ISizedRoomGen>
    {
        public override string GetString(ISizedRoomGen obj, Type type, object[] attributes)
        {
            //TODO: find a way to get member info without using a string literal of the member name
            PropertyInfo widthInfo = type.GetProperty("Width");
            PropertyInfo heightInfo = type.GetProperty("Height");
            return string.Format("{0}: {1}x{2}", obj.GetType().Name,
                DataEditor.GetString(obj.Width, widthInfo.GetMemberInfoType(), widthInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.Height, heightInfo.GetMemberInfoType(), heightInfo.GetCustomAttributes(false)));
        }
    }

    public class RoomGenCrossEditor : Editor<IRoomGenCross>
    {
        public override string GetString(IRoomGenCross obj, Type type, object[] attributes)
        {
            //TODO: find a way to get member info without using a string literal of the member name
            PropertyInfo majorWidthInfo = type.GetProperty("MajorWidth");
            PropertyInfo minorHeightInfo = type.GetProperty("MinorHeight");
            PropertyInfo minorWidthInfo = type.GetProperty("MinorWidth");
            PropertyInfo majorHeightInfo = type.GetProperty("MajorHeight");

            return string.Format("{0}: {1}x{2}+{3}x{4}", obj.GetType().Name,
                DataEditor.GetString(obj.MajorWidth, majorWidthInfo.GetMemberInfoType(), majorWidthInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.MinorHeight, minorHeightInfo.GetMemberInfoType(), minorHeightInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.MinorWidth, minorWidthInfo.GetMemberInfoType(), minorWidthInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.MajorHeight, majorHeightInfo.GetMemberInfoType(), majorHeightInfo.GetCustomAttributes(false)));
        }
    }
}
