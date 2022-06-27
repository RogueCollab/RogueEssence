using System;

namespace RogueEssence.LevelGen
{
    [Flags]
    public enum PostProcType
    {
        None = 0,
        Terrain = 1,
        Panel = 2,
        Item = 4,
    }

    public class PostProcTile
    {
        public PostProcType Status;

        public PostProcTile()
        { }

        public PostProcTile(PostProcType status)
        {
            Status = status;
        }

        public PostProcTile(PostProcTile other) : this()
        {
            Status = other.Status;
        }

        public void AddMask(PostProcTile other)
        {
            Status |= other.Status;
        }
    }
}
