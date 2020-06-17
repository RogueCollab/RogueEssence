using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class RoomGenPostProcSpecific<T> : RoomGenSpecific<T> where T : ITiledGenContext, IPostProcGenContext
    {
        public PostProcTile[][] PostProcMask;

        public RoomGenPostProcSpecific() { }
        public RoomGenPostProcSpecific(int width, int height) : base(width, height)
        {
            PostProcMask = new PostProcTile[width][];
            for (int xx = 0; xx < width; xx++)
                PostProcMask[xx] = new PostProcTile[height];
        }
        public RoomGenPostProcSpecific(int width, int height, ITile roomTerrain, bool fulfillAll) : this(width, height)
        {
            RoomTerrain = roomTerrain;
            FulfillAll = fulfillAll;
        }

        public override void DrawOnMap(T map)
        {
            if (Draw.Width != Tiles.Length || Draw.Height != Tiles[0].Length)
            {
                DrawMapDefault(map);
                return;
            }

            base.DrawOnMap(map);

            for (int xx = 0; xx < Draw.Width; xx++)
            {
                for (int yy = 0; yy < Draw.Height; yy++)
                    map.PostProcGrid[Draw.X + xx][Draw.Y + yy].AddMask(PostProcMask[xx][yy]);
            }
        }
    }
}
