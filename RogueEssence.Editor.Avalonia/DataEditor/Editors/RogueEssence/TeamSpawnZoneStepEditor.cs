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
using RogueEssence.Dev;
using RogueEssence.LevelGen;

namespace RogueEssence.Dev
{
    public class TeamSpawnZoneStepEditor : Editor<TeamSpawnZoneStep>
    {
        public override string GetString(TeamSpawnZoneStep obj, Type type, object[] attributes)
        {
            return String.Format("{0} [{1}]", "Spawn NPCs", obj.Spawns.Count.ToString());
        }
    }
}
