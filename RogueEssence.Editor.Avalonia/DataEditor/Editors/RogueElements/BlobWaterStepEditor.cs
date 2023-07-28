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
    public class BlobWaterStepEditor : Editor<IBlobWaterStep>
    {
        public override string GetString(IBlobWaterStep obj, Type type, object[] attributes)
        {
            PropertyInfo blobInfo = typeof(IBlobWaterStep).GetProperty(nameof(obj.Blobs));
            PropertyInfo areaInfo = typeof(IBlobWaterStep).GetProperty(nameof(obj.AreaScale));
            return string.Format("{0}: Amt: {1} Size: {2}", obj.GetType().GetFormattedTypeName(),
                DataEditor.GetString(obj.Blobs, blobInfo.GetMemberInfoType(), blobInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.AreaScale, areaInfo.GetMemberInfoType(), areaInfo.GetCustomAttributes(false)));
        }
    }
}
