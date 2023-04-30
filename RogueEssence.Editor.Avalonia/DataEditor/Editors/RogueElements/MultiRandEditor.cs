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
using System.Reflection;

namespace RogueEssence.Dev
{
    public class LoopedRandEditor : Editor<ILoopedRand>
    {

        public override string GetString(ILoopedRand obj, Type type, object[] attributes)
        {
            if (obj.AmountSpawner == null)
                return string.Format("{0}[EMPTY]", type.GetFormattedTypeName());

            PropertyInfo memberInfo = typeof(ILoopedRand).GetProperty(nameof(obj.AmountSpawner));
            return string.Format("{0}[{1}]", type.GetFormattedTypeName(), DataEditor.GetString(obj.AmountSpawner, memberInfo.GetMemberInfoType(), memberInfo.GetCustomAttributes(false)));
        }
    }

    public class PresetMultiRandEditor : Editor<IPresetMultiRand>
    {
        public override string GetString(IPresetMultiRand obj, Type type, object[] attributes)
        {
            if (obj.Count == 1)
            {
                object spawn = obj.ToSpawn[0];
                return string.Format("{{{0}}}", spawn.ToString());
            }
            return string.Format("{0}[{1}]", this.GetType().GetFormattedTypeName(), obj.Count);
        }
    }
}
