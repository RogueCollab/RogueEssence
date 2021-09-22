using System;
using RogueElements;
using System.Collections.Generic;

namespace RogueEssence
{
    [Serializable]
    public class AddTunnelStep<T> : GenStep<T>
        where T : class, ITiledGenContext
    {
        public AddTunnelStep()
        {
            this.Brush = new DefaultHallBrush();
        }

        public RandRange TurnLength { get; set; }

        public RandRange MaxLength { get; set; }

        public bool AllowDeadEnd { get; set; }

        public RandRange Halls { get; set; }

        public BaseHallBrush Brush { get; set; }

        public override void Apply(T map)
        {
            Grid.LocTest checkGround = (Loc testLoc) =>
            {
                if (!Collision.InBounds(map.Width, map.Height, testLoc))
                    return false;
                return (map.GetTile(testLoc).TileEquivalent(map.RoomTerrain));
            };
            Grid.LocTest checkBlock = (Loc testLoc) =>
            {
                if (!Collision.InBounds(map.Width, map.Height, testLoc))
                    return false;
                return map.GetTile(testLoc).TileEquivalent(map.WallTerrain);
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
