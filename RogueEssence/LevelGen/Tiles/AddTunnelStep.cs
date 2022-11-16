using System;
using RogueElements;
using System.Collections.Generic;

namespace RogueEssence
{
    /// <summary>
    /// Draws a specified number of angular tunnels starting from the edge of any room or hall.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class AddTunnelStep<T> : GenStep<T>
        where T : class, ITiledGenContext
    {
        public AddTunnelStep()
        {
            this.Brush = new DefaultHallBrush();
        }

        /// <summary>
        /// The number of tiles to dig the tunnel forward before changing direction.
        /// </summary>
        public RandRange TurnLength { get; set; }

        /// <summary>
        /// The expected length of the whole tunnel.
        /// Actual tunnels can be shorter if they dig into a room or hall, but cannot exceed the chosen max.
        /// </summary>
        public RandRange MaxLength { get; set; }

        /// <summary>
        /// Allows tunnels to be dead ends.  Forces tunnels to touch another room or hall if turned off.
        /// </summary>
        public bool AllowDeadEnd { get; set; }

        /// <summary>
        /// The number of tunnels to draw.
        /// </summary>
        public RandRange Halls { get; set; }

        /// <summary>
        /// The brush to draw the halls with.
        /// </summary>
        public BaseHallBrush Brush { get; set; }

        public override void Apply(T map)
        {
            Grid.LocTest checkGround = (Loc testLoc) =>
            {
                return map.RoomTerrain.TileEquivalent(map.GetTile(testLoc));
            };
            Grid.LocTest checkBlock = (Loc testLoc) =>
            {
                return map.WallTerrain.TileEquivalent(map.GetTile(testLoc));
            };

            Rect fullRect = new Rect(0, 0, map.Width, map.Height);
            int finalHalls = Halls.Pick(map.Rand);
            for (int ii = 0; ii < finalHalls; ii++)
            {
                List<LocRay4> possibleDirs = Detection.DetectWalls(fullRect, checkBlock, checkGround);

                //no more places to add halls
                if (possibleDirs.Count == 0)
                    break;

                for (int nn = 0; nn < 10; nn++)
                {
                    //pick a direction to tunnel in
                    LocRay4 tunnelDir = possibleDirs[map.Rand.Next(possibleDirs.Count)];

                    int curLength = 0;
                    int finalLength = MaxLength.Pick(map.Rand);

                    List<LocRay4> drawnLocs = new List<LocRay4>();
                    bool crossedSelf = false;
                    bool bonk = false;
                    bool bonkFloor = false;
                    while (curLength < finalLength)
                    {
                        int addLength = TurnLength.Pick(map.Rand);
                        //length bust always be at least 2
                        addLength = Math.Max(2, addLength);

                        //traverse the length in the specified tunnelDir until we hit a border or a new walkable
                        for (int jj = 0; jj < addLength; jj++)
                        {
                            //draw brush
                            drawnLocs.Add(new LocRay4(tunnelDir.Loc, tunnelDir.Dir));

                            //move forward
                            tunnelDir.Loc = tunnelDir.Traverse(1);

                            foreach (LocRay4 ray in drawnLocs)
                            {
                                if (ray.Loc == tunnelDir.Loc)
                                {
                                    crossedSelf = true;
                                    break;
                                }
                            }

                            if (!checkBlock(tunnelDir.Loc) || crossedSelf)
                            {
                                bonk = true;
                                if (checkGround(tunnelDir.Loc) || crossedSelf)
                                    bonkFloor = true;
                                break;
                            }
                        }

                        if (bonk)
                            break;

                        curLength += addLength;

                        //make a turn
                        if (map.Rand.Next(2) == 0)
                            tunnelDir.Dir = DirExt.AddAngles(tunnelDir.Dir, Dir4.Left);
                        else
                            tunnelDir.Dir = DirExt.AddAngles(tunnelDir.Dir, Dir4.Right);
                    }

                    if (!AllowDeadEnd)
                    {
                        if (!bonkFloor || crossedSelf)
                            continue;
                    }

                    foreach(LocRay4 ray in drawnLocs)
                        Brush.DrawHallBrush(map, fullRect, ray.Loc, ray.Dir.ToAxis() == Axis4.Vert);
                    break;
                }
                GenContextDebug.DebugProgress("Added Tunnel");

            }
        }
    }
}
