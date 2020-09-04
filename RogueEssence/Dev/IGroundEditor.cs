using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public interface IGroundEditor
    {

        bool Active { get; }

        void ProcessInput(InputManager input);
    }
}