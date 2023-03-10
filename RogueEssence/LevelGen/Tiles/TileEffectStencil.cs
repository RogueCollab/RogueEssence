using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence;
using RogueEssence.Data;
using RogueEssence.Dev;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// A filter for determining the eligible tiles for an operation.
    /// Tiles must or must not have a panel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class TileEffectStencil<TGenContext> : ITerrainStencil<TGenContext>
        where TGenContext : BaseMapGenContext
    {
        public TileEffectStencil()
        {
        }

        public TileEffectStencil(bool not)
        {
            this.Not = not;
        }

        /// <summary>
        /// If turned on, test will pass for empty tiles.
        /// </summary>
        public bool Not { get; private set; }

        public bool Test(TGenContext map, Loc loc)
        {
            Tile checkTile = (Tile)map.GetTile(loc);
            return (String.IsNullOrEmpty(checkTile.Effect.ID) == this.Not);
        }
    }

    /// <summary>
    /// A filter for determining the eligible tiles for an operation.
    /// Tiles must have a specific panel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MatchTileEffectStencil<TGenContext> : ITerrainStencil<TGenContext>
        where TGenContext : BaseMapGenContext
    {
        public MatchTileEffectStencil()
        {
        }

        public MatchTileEffectStencil(string effect)
        {
            this.Effect = effect;
        }


        [DataType(0, DataManager.DataType.Tile, false)]
        public string Effect;

        public bool Test(TGenContext map, Loc loc)
        {
            Tile checkTile = (Tile)map.GetTile(loc);
            return (checkTile.Effect.ID == this.Effect);
        }
    }
}
