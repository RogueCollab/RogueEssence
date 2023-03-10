using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Generates a room, and then coats it with additional tiles.
    /// INCOMPLETE
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class RoomGenCoated<T> : RoomGen<T> where T : ITiledGenContext
    {
        public int AddWidth;
        public int AddHeight;
        public bool FulfillAll;
        public RoomGen<T> Gen;

        public RoomGenCoated() { }
        public RoomGenCoated(int addWidth, int addHeight)
        {
            AddWidth = addWidth;
            AddHeight = addHeight;
        }
        public RoomGenCoated(int addWidth, int addHeight, RoomGen<T> gen, bool fulfillAll) : this(addWidth, addHeight)
        {
            Gen = gen;
            FulfillAll = fulfillAll;
        }
        protected RoomGenCoated(RoomGenCoated<T> other)
        {
            AddWidth = other.AddWidth;
            AddHeight = other.AddHeight;
            FulfillAll = other.FulfillAll;
            Gen = other.Gen.Copy();
        }
        public override RoomGen<T> Copy() { return new RoomGenCoated<T>(this); }



        public override void AskBorderFromRoom(Rect sourceRect, Func<Dir4, int, bool> borderQuery, Dir4 dir)
        {
            //TODO: transfer to internal roomgen 
        }

        public override void AskBorderRange(IntRange range, Dir4 dir)
        {
            //TODO: transfer to internal roomgen 
        }

        public override Loc ProposeSize(IRandom rand)
        {
            return Gen.ProposeSize(rand) + new Loc(AddWidth * 2, AddHeight * 2);
        }

        public override void PrepareSize(IRandom rand, Loc size)
        {
            //transfer to internal roomgen
            Gen.PrepareSize(rand, size - new Loc(AddWidth * 2, AddHeight * 2));
        }

        protected override void PrepareFulfillableBorders(IRandom rand)
        {
            //TODO: translate the already fulfilled borders
            //Gen.GetFulfillableBorder
        }

        public override void DrawOnMap(T map)
        {
            //TODO: call internal room draw
            FulfillRoomBorders(map, FulfillAll);
            //Gen.DrawOnMap(map);
            SetRoomBorders(map);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}+{2}x{3}", this.GetType().Name, this.Gen.ToString(), this.AddWidth, this.AddHeight);
        }
    }
}
