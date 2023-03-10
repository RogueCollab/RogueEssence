using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Picks tiles of a terrain spawns the mobs there.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class PlaceTerrainMobsStep<T> : PlaceMobsStep<T>
        where T : BaseMapGenContext, ITiledGenContext
    {
        /// <summary>
        /// The terrain types to spawn the mobs in.
        /// </summary>
        public List<ITile> AcceptedTiles;

        public PlaceTerrainMobsStep()
        {
            AcceptedTiles = new List<ITile>();
        }

        public PlaceTerrainMobsStep(IMultiTeamSpawner<T> spawn) : base(spawn)
        {
            AcceptedTiles = new List<ITile>();
        }

        public override void Apply(T map)
        {

            List<Team> spawns = Spawn.GetSpawns(map);

            if (spawns.Count == 0)
                return;

            //obtain all tiles not in the main path
            List<Loc> freeTiles = new List<Loc>();

            for (int xx = 0; xx < map.Width; xx++)
            {
                for (int yy = 0; yy < map.Height; yy++)
                {
                    bool allowPlacement = false;
                    foreach (ITile tile in AcceptedTiles)
                    {
                        if (tile.TileEquivalent(map.GetTile(new Loc(xx, yy))))
                            allowPlacement = true;
                    }
                    if (allowPlacement)
                        freeTiles.Add(new Loc(xx, yy));
                }
            }


            int trials = 10 * spawns.Count;
            for (int ii = 0; ii < trials; ii++)
            {
                int teamIndex = map.Rand.Next(spawns.Count);
                Team newTeam = spawns[teamIndex];
                if (newTeam == null)
                    continue;

                if (freeTiles.Count >= newTeam.MemberGuestCount)
                {
                    Loc[] locs = new Loc[newTeam.MemberGuestCount];
                    for (int jj = 0; jj < locs.Length; jj++)
                    {
                        int randIndex = map.Rand.Next(freeTiles.Count);
                        locs[jj] = freeTiles[randIndex];
                        freeTiles.RemoveAt(randIndex);
                    }
                    ((IGroupPlaceableGenContext<TeamSpawn>)map).PlaceItems(new TeamSpawn(newTeam, Ally), locs);
                    spawns.RemoveAt(teamIndex);
                }

                if (freeTiles.Count == 0 || spawns.Count == 0)
                    break;
            }

        }

    }
}
