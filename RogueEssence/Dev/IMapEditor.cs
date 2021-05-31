using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public interface IMapEditor
    {
        public enum TileEditMode
        {
            Draw = 0,
            Fill = 1,
            Eyedrop = 2
        }

        bool Active { get; }
        public UndoStack Edits { get; }
        void ProcessInput(InputManager input);
    }
}