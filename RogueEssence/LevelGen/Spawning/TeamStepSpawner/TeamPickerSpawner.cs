using System;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class TeamPickerSpawner<T> : ITeamStepSpawner<T>
        where T : IGenContext, IMobSpawnMap
    {
        public TeamSpawner Picker;

        public TeamPickerSpawner() { }

        public TeamPickerSpawner(TeamSpawner picker)
        {
            Picker = picker;
        }

        public Team GetSpawn(T map)
        {
            return Picker.Spawn(map);
        }
    }
}
