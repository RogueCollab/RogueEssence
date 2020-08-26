namespace RogueEssence.Dev
{
    public interface IRootEditor
    {
        bool Loaded { get; }
        IGroundEditor GroundEditor { get; }
        IMapEditor MapEditor { get; }

        void Load();
        void OpenGround();
        void CloseGround();
    }
}