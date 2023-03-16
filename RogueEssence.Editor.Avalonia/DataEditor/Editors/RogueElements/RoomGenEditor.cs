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
            PropertyInfo widthInfo = type.GetProperty(nameof(obj.Width));
            PropertyInfo heightInfo = type.GetProperty(nameof(obj.Height));
            return string.Format("{0}: {1}x{2}", obj.GetType().GetFormattedTypeName(),
                DataEditor.GetString(obj.Width, widthInfo.GetMemberInfoType(), widthInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.Height, heightInfo.GetMemberInfoType(), heightInfo.GetCustomAttributes(false)));
        }
    }

    public class RoomGenCrossEditor : Editor<IRoomGenCross>
    {
        public override string GetString(IRoomGenCross obj, Type type, object[] attributes)
        {
            PropertyInfo majorWidthInfo = type.GetProperty(nameof(obj.MajorWidth));
            PropertyInfo minorHeightInfo = type.GetProperty(nameof(obj.MinorHeight));
            PropertyInfo minorWidthInfo = type.GetProperty(nameof(obj.MinorWidth));
            PropertyInfo majorHeightInfo = type.GetProperty(nameof(obj.MajorHeight));

            return string.Format("{0}: {1}x{2}+{3}x{4}", obj.GetType().GetFormattedTypeName(),
                DataEditor.GetString(obj.MajorWidth, majorWidthInfo.GetMemberInfoType(), majorWidthInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.MinorHeight, minorHeightInfo.GetMemberInfoType(), minorHeightInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.MinorWidth, minorWidthInfo.GetMemberInfoType(), minorWidthInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.MajorHeight, majorHeightInfo.GetMemberInfoType(), majorHeightInfo.GetCustomAttributes(false)));
        }
    }
}
