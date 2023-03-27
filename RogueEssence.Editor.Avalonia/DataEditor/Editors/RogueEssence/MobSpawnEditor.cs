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
    public class MobSpawnEditor : Editor<MobSpawn>
    {
        public override string GetString(MobSpawn obj, Type type, object[] attributes)
        {
            string monName = "[EMPTY]";
            if (!String.IsNullOrEmpty(obj.BaseForm.Species))
            {
                MonsterData entry = DataManager.Instance.GetMonster(obj.BaseForm.Species);
                entry.Name.ToLocal();
            }
            MemberInfo[] spawnInfo = type.GetMember(nameof(obj.Level));
            return String.Format("{0} Lv.{1}", monName, DataEditor.GetString(obj.Level, spawnInfo[0].GetMemberInfoType(), spawnInfo[0].GetCustomAttributes(false)));
        }
    }
}
