using System;
using RogueElements;
using System.Collections.Generic;
using RogueEssence.LevelGen;
using RogueEssence.Dev;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using Newtonsoft.Json;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Orients all already-placed compass tiles to point to points of interest.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class DetectTileStep<T> : GenStep<T>
        where T : StairsMapGenContext
    {
        /// <summary>
        /// Tile used as compass.
        /// </summary>
        [JsonConverter(typeof(TileConverter))]
        [DataType(0, DataManager.DataType.Tile, false)]
        public string FindTile;

        public DetectTileStep()
        {
        }

        public DetectTileStep(string tile)
        {
            FindTile = tile;
        }

        public override void Apply(T map)
        {
            for (int xx = 0; xx < map.Width; xx++)
            {
                for (int yy = 0; yy < map.Height; yy++)
                {
                    Loc tileLoc = new Loc(xx, yy);
                    Tile tile = map.Map.GetTile(tileLoc);
                    if (tile.Effect.ID == FindTile)
                        return;
                }
            }

            throw new Exception("Did not find tile " + FindTile + "!  Seed: " + map.Rand.FirstSeed);
        }
    }
}
