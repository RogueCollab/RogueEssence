using Microsoft.Xna.Framework;

namespace RogueEssence.Dev
{
    public interface IRootEditor
    {
        bool LoadComplete { get; }
        bool AteMouse { get; }
        bool AteKeyboard { get; }
        IGroundEditor GroundEditor { get; }
        IMapEditor MapEditor { get; }

        void Load(GameBase game);
        void Update(GameTime gameTime);
        void Draw();
        void OpenGround();
        void CloseGround();
    }
}