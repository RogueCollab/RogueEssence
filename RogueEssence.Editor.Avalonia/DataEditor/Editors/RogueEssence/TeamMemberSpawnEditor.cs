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
    public class TeamMemberSpawnEditor : Editor<TeamMemberSpawn>
    {
        public override string GetString(TeamMemberSpawn obj, Type type, object[] attributes)
        {
            MemberInfo[] spawnInfo = type.GetMember(nameof(obj.Spawn));
            return DataEditor.GetString(obj.Spawn, spawnInfo[0].GetMemberInfoType(), spawnInfo[0].GetCustomAttributes(false));
        }
    }
}
