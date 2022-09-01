// <copyright file="IntrudingStencil.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence
{
    /// <summary>
    /// A filter for determining the eligible tiles for an operation.
    /// Tiles at the border of 2 terrain types are eligible
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class TerrainBorderStencil<T> : ITerrainStencil<T>
        where T : class, ITiledGenContext
    {
        public TerrainBorderStencil()
        {
            this.MatchTiles = new List<ITile>();
            this.BorderTiles = new List<ITile>();
        }

        /// <summary>
        /// The allowed tile types that the blob can be placed on.
        /// </summary>
        public List<ITile> MatchTiles { get; private set; }


        /// <summary>
        /// The tile types that the blob must be touching to allow placement.
        /// </summary>
        public List<ITile> BorderTiles { get; private set; }

        /// <summary>
        /// If left off, the blob will only paint over the match tiles.
        /// If turned on, the blob will be allowed to paint over border tiles.
        /// </summary>
        public bool Intrude;

        /// <summary>
        /// Counts diagonal adjacent tiles if they are a border tile.
        /// </summary>
        public bool AllowDiagonal;

        /// <summary>
        /// The adjacent side must fully touch border.
        /// </summary>
        public bool FullSide;

        public bool Test(T map, Rect rect)
        {
            Rect borderRect = rect;
            borderRect.Inflate(1, 1);
            // confirm all tiles are of the requested tile type
            // confirm at least one bordering tile is not the requested tile type
            bool foundMatch = false;
            bool foundBorder = false;
            bool[] sideHasNonBorder = new bool[4];

            for (int xx = borderRect.X; xx < borderRect.End.X; xx++)
            {
                for (int yy = borderRect.Y; yy < borderRect.End.Y; yy++)
                {
                    Loc loc = new Loc(xx, yy);
                    if (rect.Contains(loc))
                    {
                        if (!Intrude)
                        {
                            if (!this.testTile(map, loc))
                                return false;
                            else
                                foundMatch = true;
                        }
                        else if (Intrude)
                        {
                            if (this.testTile(map, loc))
                                foundMatch = true;
                            else if (!this.testBorder(map, loc))
                                return false;

                            if (this.testBorder(map, loc))
                                foundBorder = true;
                        }
                    }
                    else
                    {
                        bool isDiagonal = (xx == borderRect.X || xx == borderRect.End.X - 1) && (yy == borderRect.Y || yy == borderRect.End.Y - 1);
                        if (!AllowDiagonal)
                        {
                            if (isDiagonal)
                                continue;
                        }
                        if (this.testBorder(map, loc))
                            foundBorder = true;
                        else if (!isDiagonal)
                        {
                            Dir4 dir = DirExt.GetBoundsDir(rect.Start, rect.Size, loc).ToDir4();
                            sideHasNonBorder[(int)dir] = true;
                        }
                    }
                }
            }
            if (!foundBorder)
                return false;
            if (!foundMatch)
                return false;

            if (FullSide)
            {
                // we require at least one side that is fully bordered by border tiles
                foreach (bool side in sideHasNonBorder)
                {
                    if (!side)
                        return true;
                }
                return false;
            }
            else
                return true;
        }

        protected bool testTile(T map, Loc loc)
        {
            ITile checkTile = map.GetTile(loc);
            foreach (ITile tile in this.MatchTiles)
            {
                if (checkTile.TileEquivalent(tile))
                    return true;
            }

            return false;
        }

        protected bool testBorder(T map, Loc loc)
        {
            if (!Collision.InBounds(map.Width, map.Height, loc))
                return false;

            ITile checkTile = map.GetTile(loc);
            foreach (ITile tile in this.BorderTiles)
            {
                if (checkTile.TileEquivalent(tile))
                    return true;
            }

            return false;
        }
    }
}
