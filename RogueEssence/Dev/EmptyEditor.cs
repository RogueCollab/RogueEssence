using Microsoft.Xna.Framework;
using RogueEssence.Data;

namespace RogueEssence.Dev
{
    public class EmptyEditor : IRootEditor
    {
        public bool LoadComplete => true;
        public IGroundEditor GroundEditor => null;
        public IMapEditor MapEditor => null;
        public bool AteMouse { get { return false; } }
        public bool AteKeyboard { get { return false; } }

        public void ReloadData(DataManager.DataType dataType) { }
        public void Load(GameBase game) { }
        public void Update(GameTime gameTime) { }
        public void Draw() { }
        public void OpenGround() { }
        public void CloseGround() { }
        public void OpenMap() { }
        public void CloseMap() { }
    }
}