using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Generates a Right Triangle-shaped room, wih its right angle at the top-left.  Flipping can cause it to be angled other directions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class RoomGenTriangle<T> : RoomGen<T>, ISizedRoomGen
        where T : ITiledGenContext
    {
        public RoomGenTriangle()
        {
        }

        public RoomGenTriangle(RandRange size, bool flipH, bool flipV)
        {
            this.Size = size;
            this.FlipH = flipH;
            this.FlipV = flipV;
        }

        protected RoomGenTriangle(RoomGenTriangle<T> other)
        {
            this.Size = other.Size;
            this.FlipH = other.FlipH;
            this.FlipV = other.FlipV;
        }

        /// <summary>
        /// Flips the triangle horizontally
        /// </summary>
        public bool FlipH { get; set; }

        /// <summary>
        /// Flips the triangle vertically
        /// </summary>
        public bool FlipV { get; set; }

        public RandRange Size { get; set; }

        /// <summary>
        /// Width of the room.
        /// </summary>
        public RandRange Width { get { return Size; } set { Size = value; } }

        /// <summary>
        /// Height of the room.
        /// </summary>
        public RandRange Height { get { return Size; } set { Size = value; } }

        public override RoomGen<T> Copy() => new RoomGenTriangle<T>(this);

        public override Loc ProposeSize(IRandom rand)
        {
            return new Loc(this.Size.Pick(rand));
        }

        public override void DrawOnMap(T map)
        {
            int diameter = Math.Min(this.Draw.Width, this.Draw.Height);

            for (int ii = 0; ii < this.Draw.Width; ii++)
            {
                for (int jj = 0; jj < this.Draw.Height; jj++)
                {
                    if (IsTileWithinRoom(ii, jj, diameter))
                        map.SetTile(new Loc(this.Draw.X + ii, this.Draw.Y + jj), map.RoomTerrain.Copy());
                }
            }

            // hall restrictions
            this.SetRoomBorders(map);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}x{1}", this.GetType().GetFormattedTypeName(), this.Size.ToString());
        }

        protected override void PrepareFulfillableBorders(IRandom rand)
        {
            int diameter = Math.Min(this.Draw.Width, this.Draw.Height);
            for (int jj = 0; jj < this.Draw.Width; jj++)
            {
                if (IsTileWithinRoom(jj, 0, diameter))
                    this.FulfillableBorder[Dir4.Up][jj] = true;

                if (IsTileWithinRoom(jj, this.Draw.Height-1, diameter))
                    this.FulfillableBorder[Dir4.Down][jj] = true;
            }

            for (int jj = 0; jj < this.Draw.Height; jj++)
            {
                if (IsTileWithinRoom(0, jj, diameter))
                    this.FulfillableBorder[Dir4.Left][jj] = true;
                if (IsTileWithinRoom(this.Draw.Width - 1, jj, diameter))
                    this.FulfillableBorder[Dir4.Right][jj] = true;
            }
        }

        private bool IsTileWithinRoom(int x, int y, int diameter)
        {
            if (FlipH)
                x = diameter - x - 1;
            if (FlipV)
                y = diameter - y - 1;
            return (x + y) < diameter;
        }
    }
}
