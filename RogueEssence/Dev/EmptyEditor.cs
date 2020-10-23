namespace RogueEssence.Dev
{
    public class EmptyEditor : IRootEditor
    {
        public bool LoadComplete => true;
        public IGroundEditor GroundEditor => null;
        public IMapEditor MapEditor => null;

        public void Load() { }
        public void OpenGround() { }
        public void CloseGround() { }
    }
}