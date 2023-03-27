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
    public class BaseSpawnStepEditor : Editor<IBaseSpawnStep>
    {
        public override string GetString(IBaseSpawnStep obj, Type type, object[] attributes)
        {
            PropertyInfo memberInfo = typeof(IBaseSpawnStep).GetProperty(nameof(obj.Spawn));
            if (obj.Spawn == null)
                return string.Format("{0}<{1}>: [EMPTY]", obj.GetType().GetFormattedTypeName(), obj.SpawnType.Name);
            return string.Format("{0}<{1}>: {2}", obj.GetType().GetFormattedTypeName(), obj.SpawnType.Name, DataEditor.GetString(obj.Spawn, memberInfo.GetMemberInfoType(), memberInfo.GetCustomAttributes(false)));
        }
    }
    public class PlaceMobsStepEditor : Editor<IPlaceMobsStep>
    {
        public override string GetString(IPlaceMobsStep obj, Type type, object[] attributes)
        {
            PropertyInfo memberInfo = typeof(IPlaceMobsStep).GetProperty(nameof(obj.Spawn));
            if (obj.Spawn == null)
                return string.Format("{0}: [EMPTY]", obj.GetType().GetFormattedTypeName());
            return string.Format("{0}: {1}", obj.GetType().GetFormattedTypeName(), DataEditor.GetString(obj.Spawn, memberInfo.GetMemberInfoType(), memberInfo.GetCustomAttributes(false)));
        }
    }
}
