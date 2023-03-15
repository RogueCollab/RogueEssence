using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Generates a rectangular room with the specified width and height, and blocks off the tiles at the edges.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class RoomGenPlus<T> : PermissiveRoomGen<T> where T : ITiledGenContext
    {

        [NonSerialized]
        private int chosenCorner;

        /// <summary>
        /// Width of the room.
        /// </summary>
        public RandRange Width;

        /// <summary>
        /// Height of the room.
        /// </summary>
        public RandRange Height;

        /// <summary>
        /// Amount of tiles to remove from corner.
        /// </summary>
        public RandRange Corner;

        public RoomGenPlus() { }

        public RoomGenPlus(RandRange width, RandRange height, RandRange corner)
        {
            Width = width;
            Height = height;
            Corner = corner;
        }
        protected RoomGenPlus(RoomGenPlus<T> other)
        {
            Width = other.Width;
            Height = other.Height;
            Corner = other.Corner;
        }
        public override RoomGen<T> Copy() { return new RoomGenPlus<T>(this); }

        public override Loc ProposeSize(IRandom rand)
        {
            chosenCorner = this.Corner.Pick(rand);
            return new Loc(this.Width.Pick(rand), this.Height.Pick(rand));
        }

        public override void DrawOnMap(T map)
        {
            int diameter = Math.Min(this.Draw.Width, this.Draw.Height);

            for (int ii = 0; ii < this.Draw.Width; ii++)
            {
                for (int jj = 0; jj < this.Draw.Height; jj++)
                {
                    if (IsTileWithinRoom(ii, jj, diameter, this.chosenCorner, this.Draw.Size))
                        map.SetTile(new Loc(this.Draw.X + ii, this.Draw.Y + jj), map.RoomTerrain.Copy());
                }
            }

            // hall restrictions
            this.SetRoomBorders(map);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}x{2}", this.GetType().GetFormattedTypeName(), this.Width.ToString(), this.Height.ToString());
        }

        protected override void PrepareFulfillableBorders(IRandom rand)
        {
            int diameter = Math.Min(this.Draw.Width, this.Draw.Height);
            for (int jj = 0; jj < this.Draw.Width; jj++)
            {
                if (IsTileWithinRoom(jj, 0, diameter, this.chosenCorner, this.Draw.Size))
                {
                    this.FulfillableBorder[Dir4.Up][jj] = true;
                    this.FulfillableBorder[Dir4.Down][jj] = true;
                }
            }

            for (int jj = 0; jj < this.Draw.Height; jj++)
            {
                if (IsTileWithinRoom(0, jj, diameter, this.chosenCorner, this.Draw.Size))
                {
                    this.FulfillableBorder[Dir4.Left][jj] = true;
                    this.FulfillableBorder[Dir4.Right][jj] = true;
                }
            }
        }

        private static bool IsTileWithinRoom(int baseX, int baseY, int diameter, int baseCorner, Loc size)
        {
            int corner = baseCorner * 2;
            Loc sizeX2 = size * 2;
            int x = (baseX * 2) + 1;
            int y = (baseY * 2) + 1;

            if (x < diameter)
            {
                int xdiff = diameter - x;
                if (y < diameter)
                {
                    int ydiff = diameter - y;
                    if (Math.Min(xdiff, ydiff) < diameter - corner)
                        return true;
                }
                else if (y > sizeX2.Y - diameter)
                {
                    int ydiff = y - (sizeX2.Y - diameter);
                    if (Math.Min(xdiff, ydiff) < diameter - corner)
                        return true;
                }
                else
                {
                    return true;
                }
            }
            else if (x > sizeX2.X - diameter)
            {
                int xdiff = x - (sizeX2.X - diameter);
                if (y < diameter)
                {
                    int ydiff = diameter - y;
                    if (Math.Min(xdiff, ydiff) < diameter - corner)
                        return true;
                }
                else if (y > sizeX2.Y - diameter)
                {
                    int ydiff = y - (sizeX2.Y - diameter);
                    if (Math.Min(xdiff, ydiff) < diameter - corner)
                        return true;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }

            return false;
        }
    }
}
