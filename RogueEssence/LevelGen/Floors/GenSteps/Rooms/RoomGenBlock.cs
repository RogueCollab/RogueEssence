using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    ///// <summary>
    ///// Draws the base room, then draws a block on top of it.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    //[Serializable]
    //public class RoomGenBlock<T> : RoomGen<T>
    //    where T : ITiledGenContext
    //{
    //    public RoomGenBlock()
    //    {
    //    }

    //    public RoomGenBlock(ITile blockTerrain, RoomGen<T> gen, RandRange blockWidth, RandRange blockHeight)
    //    {
    //        this.BlockTerrain = blockTerrain;
    //        this.Gen = gen;
    //        this.BlockWidth = blockWidth;
    //        this.BlockHeight = blockHeight;
    //    }

    //    protected RoomGenBlock(RoomGenBlock<T> other)
    //    {
    //        this.BlockTerrain = other.BlockTerrain.Copy();
    //        this.Gen = other.Gen.Copy();
    //        this.BlockWidth = other.BlockWidth;
    //        this.BlockHeight = other.BlockHeight;
    //    }

    //    public RandRange BlockWidth { get; set; }

    //    public RandRange BlockHeight { get; set; }

    //    public ITile BlockTerrain { get; set; }

    //    public RoomGen<T> Gen { get; set; }

    //    public override RoomGen<T> Copy() => new RoomGenBlock<T>(this);

    //    public override void ReceiveOpenedBorder(IRoomGen sourceRoom, Dir4 dir)
    //    {
    //        Gen.ReceiveOpenedBorder(sourceRoom, dir);
    //    }
    //    public override void ReceiveFulfillableBorder(IRoomGen sourceRoom, Dir4 dir)
    //    {
    //        Gen.ReceiveFulfillableBorder(sourceRoom, dir);
    //    }
    //    public override void ReceiveBorderRange(IntRange range, Dir4 dir)
    //    {
    //        Gen.ReceiveBorderRange(range, dir);
    //    }

    //    public override Loc ProposeSize(IRandom rand)
    //    {
    //        return Gen.ProposeSize(rand);
    //    }

    //    public override void PrepareSize(IRandom rand, Loc size)
    //    {
    //        Gen.PrepareSize(rand, size);
    //    }

    //    protected override void PrepareFulfillableBorders(IRandom rand)
    //    {
    //        //TODO: resolve permission issue for this
    //        //Gen.PrepareFulfillableBorders(rand);
    //    }

    //    public override void DrawOnMap(T map)
    //    {
    //        Gen.DrawOnMap(map);

    //        GenContextDebug.DebugProgress("Room Drawn");

    //        //TODO: do not cover forks in the road
    //        Loc blockSize = new Loc(Math.Min(this.BlockWidth.Pick(map.Rand), this.Draw.Size.X - 2), Math.Min(this.BlockHeight.Pick(map.Rand), this.Draw.Size.Y - 2));
    //        Loc blockStart = new Loc(this.Draw.X + map.Rand.Next(1, this.Draw.Size.X - blockSize.X - 1), this.Draw.Y + map.Rand.Next(1, this.Draw.Size.Y - blockSize.Y - 1));
    //        for (int x = 0; x < blockSize.X; x++)
    //        {
    //            for (int y = 0; y < blockSize.Y; y++)
    //                map.SetTile(new Loc(blockStart.X + x, blockStart.Y + y), this.BlockTerrain.Copy());
    //        }

    //        GenContextDebug.DebugProgress("Block Rect");

    //        // hall restrictions
    //        this.SetRoomBorders(map);
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("{0}: {1}", this.GetType().Name);
    //    }
    //}
}
