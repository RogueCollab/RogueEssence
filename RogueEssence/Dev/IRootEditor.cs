using Microsoft.Xna.Framework;
using RogueEssence.Data;

namespace RogueEssence.Dev
{
    public interface IRootEditor
    {
        bool LoadComplete { get; }
        bool AteMouse { get; }
        bool AteKeyboard { get; }
        IGroundEditor GroundEditor { get; }
        IMapEditor MapEditor { get; }

        void ReloadData(DataManager.DataType dataType);
        void Load(GameBase game);
        void Update(GameTime gameTime);
        void Draw();
        void OpenGround();
        void CloseGround();
    }
}