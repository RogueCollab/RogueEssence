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
        TileEditMode Mode { get; }

        void PaintTile(Loc loc, TileLayer anim);
        TileLayer GetBrush();
        void EyedropTile(Loc loc);
        void FillTile(Loc loc, TileLayer anim);
    }
}