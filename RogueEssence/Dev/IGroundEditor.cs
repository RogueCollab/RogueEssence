using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public interface IGroundEditor
    {
        public enum TileEditMode
        {
            Draw = 0,
            Fill = 1,
            Eyedrop = 2,
            PlaceEntity = 3,
            PlaceTemplateEntity = 4,
            SelectEntity = 5,
            MoveEntity = 6,
            Disabled, //Added this so on tabs that don't interact with the map, people don't accidently paint or place entities.
        }

        bool Active { get; }
        TileEditMode Mode { get; }

        void PaintTile(Loc loc, TileLayer anim);
        TileLayer GetBrush();
        void EyedropTile(Loc loc);
        void FillTile(Loc loc, TileLayer anim);
        void PlaceEntity(Loc position);
        void PlaceTemplateEntity(Loc position);
        void SelectEntity(Loc position);
        void EntityContext(Loc mousepos, Loc mappos);
    }
}