using System;
using RogueEssence.Dungeon;
using RogueElements;
using System.Collections.Generic;

namespace RogueEssence.LevelGen
{
    public interface ITeamSpawnGenerator<T>
    {
        Team Spawn(T map);
    }
    [Serializable]
    public abstract class TeamSpawner : ITeamSpawnGenerator<IMobSpawnMap>
    {
        public abstract SpawnList<MobSpawn> GetPossibleSpawns();
        public abstract List<MobSpawn> ChooseSpawns(IRandom rand);
        public Team Spawn(IMobSpawnMap map)
        {
            List<MobSpawn> chosenSpawns = ChooseSpawns(map.Rand);

            if (chosenSpawns.Count > 0)
            {
                MonsterTeam team = new MonsterTeam();
                foreach (MobSpawn chosenSpawn in chosenSpawns)
                    chosenSpawn.Spawn(team, map);
                return team;
            }
            else
                return null;
        }

        public abstract TeamSpawner Clone();
    }
}
