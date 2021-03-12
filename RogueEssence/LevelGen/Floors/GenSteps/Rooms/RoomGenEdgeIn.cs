using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class RoomGenEdgeIn<T> : PermissiveRoomGen<T> where T : ITiledGenContext
    {

        public RandRange Width;
        public RandRange Height;

        public RoomGenEdgeIn() { }

        public RoomGenEdgeIn(RandRange width, RandRange height)
        {
            Width = width;
            Height = height;
        }
        protected RoomGenEdgeIn(RoomGenEdgeIn<T> other)
        {
            Width = other.Width;
            Height = other.Height;
        }
        public override RoomGen<T> Copy() { return new RoomGenEdgeIn<T>(this); }

        public override Loc ProposeSize(IRandom rand)
        {
            return new Loc(Width.Pick(rand), Height.Pick(rand));
        }

        public override void DrawOnMap(T map)
        {
            for (int x = 0; x < Draw.Size.X; x++)
            {
                for (int y = 0; y < Draw.Size.Y; y++)
                {
                    if ((x == 0 || x == Draw.Size.X-1) && (y == 0 || y == Draw.Size.Y - 1))
                    {

                    }
                    else
                        map.SetTile(new Loc(Draw.X + x, Draw.Y + y), map.RoomTerrain.Copy());
                }
            }

            //fulfill existing borders since this room doesn't cover the entire square
            FulfillRoomBorders(map, true);
            //hall restrictions
            SetRoomBorders(map);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}x{2}", this.GetType().Name, this.Width, this.Height);
        }
    }
}
