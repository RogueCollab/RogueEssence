namespace RogueEssence.LevelGen
{
    public enum PostProcType
    {
        Terrain,
        Panel,
        Item,
        Count
    }

    public class PostProcTile
    {
        public bool[] Status;

        public PostProcTile()
        {
            Status = new bool[(int)PostProcType.Count];
        }
        public PostProcTile(PostProcTile other) : this()
        {
            other.Status.CopyTo(Status, 0);
        }

        public void AddMask(PostProcTile other)
        {
            for (int ii = 0; ii < (int)PostProcType.Count; ii++)
                Status[ii] |= other.Status[ii];
        }
    }
}
