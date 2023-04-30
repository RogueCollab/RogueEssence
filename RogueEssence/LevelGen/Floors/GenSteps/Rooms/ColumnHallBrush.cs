// <copyright file="ColumnHallBrush.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using RogueElements;
using System;
using System.Collections.Generic;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// A rectangular brush for painting hallways.
    /// </summary>
    [Serializable]
    public class ColumnHallBrush : BaseHallBrush
    {
        public ColumnHallBrush()
        {
        }

        public override Loc Size { get => new Loc(3); }

        public override Loc Center { get => Loc.One; }

        public override BaseHallBrush Clone()
        {
            return new ColumnHallBrush();
        }

        public override void DrawHallBrush(ITiledGenContext map, Rect bounds, LocRay4 ray, int length)
        {
            for (int ii = 0; ii < length; ii++)
            {
                Loc point = ray.Traverse(ii);
                List<Loc> drawLocs = new List<Loc>();
                drawLocs.Add(point);
                drawLocs.Add(point + DirExt.AddAngles(ray.Dir, Dir4.Left).GetLoc());
                drawLocs.Add(point + DirExt.AddAngles(ray.Dir, Dir4.Right).GetLoc());

                bool allOpen = true;
                foreach (Loc loc in drawLocs)
                {
                    Loc backLoc = loc + ray.Dir.Reverse().GetLoc();
                    if (!Collision.InBounds(map.Width, map.Height, backLoc) || !map.RoomTerrain.TileEquivalent(map.GetTile(backLoc)))
                        allOpen = false;
                }
                if (ii == length - 1)
                {
                    foreach (Loc loc in drawLocs)
                    {
                        Loc forthLoc = loc + ray.Dir.GetLoc();
                        if (!Collision.InBounds(map.Width, map.Height, forthLoc) || !map.RoomTerrain.TileEquivalent(map.GetTile(forthLoc)))
                            allOpen = false;
                    }
                }

                //if all 3 tiles above are open, draw an .X.
                //otherwise, draw ...
                if (allOpen)
                    drawLocs.RemoveAt(0);

                foreach (Loc loc in drawLocs)
                {
                    if (Collision.InBounds(map.Width, map.Height, loc))
                        map.SetTile(loc, map.RoomTerrain.Copy());
                }
            }
        }
    }
}
