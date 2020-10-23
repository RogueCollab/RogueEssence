namespace RogueEssence.Dev
{
    public interface IRootEditor
    {
        bool LoadComplete { get; }
        IGroundEditor GroundEditor { get; }
        IMapEditor MapEditor { get; }

        void Load();
        void OpenGround();
        void CloseGround();
    }
}