using Microsoft.Xna.Framework;

namespace RogueEssence.Dev
{
    public class EmptyEditor : IRootEditor
    {
        public bool LoadComplete => true;
        public IGroundEditor GroundEditor => null;
        public IMapEditor MapEditor => null;
        public bool AteMouse { get { return false; } }
        public bool AteKeyboard { get { return false; } }

        public void Load(GameBase game) { }
        public void Update(GameTime gameTime) { }
        public void Draw() { }
        public void OpenGround() { }
        public void CloseGround() { }
    }
}