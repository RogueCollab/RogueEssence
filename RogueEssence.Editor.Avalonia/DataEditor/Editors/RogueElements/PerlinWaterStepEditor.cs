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
    public class PerlinWaterStepEditor : Editor<IPerlinWaterStep>
    {
        public override string GetString(IPerlinWaterStep obj, Type type, object[] attributes)
        {
            PropertyInfo waterInfo = typeof(IPerlinWaterStep).GetProperty(nameof(obj.WaterPercent));
            PropertyInfo terrainInfo = typeof(IPerlinWaterStep).GetProperty(nameof(obj.Terrain));
            return string.Format("{0}: {1}% {2}", obj.GetType().GetFormattedTypeName(),
                DataEditor.GetString(obj.WaterPercent, waterInfo.GetMemberInfoType(), waterInfo.GetCustomAttributes(false)),
                DataEditor.GetString(obj.Terrain, terrainInfo.GetMemberInfoType(), terrainInfo.GetCustomAttributes(false)));
        }
    }
}
