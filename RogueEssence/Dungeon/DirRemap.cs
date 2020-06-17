using RogueElements;

namespace RogueEssence.Dungeon
{
    public static class DirRemap
    {

        //526
        //1X3
        //407
        public static readonly Dir8[] WRAPPED_DIR8 = { Dir8.Down, Dir8.Left, Dir8.Up, Dir8.Right, Dir8.DownLeft, Dir8.UpLeft, Dir8.UpRight, Dir8.DownRight };

        //576
        //3X4
        //102
        public static readonly Dir8[] FOCUSED_DIR8 = { Dir8.Down, Dir8.DownLeft, Dir8.DownRight, Dir8.Left, Dir8.Right, Dir8.UpLeft, Dir8.UpRight, Dir8.Up };

    }
}
