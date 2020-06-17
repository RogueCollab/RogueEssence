using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    public interface IPostProcGenContext : ITiledGenContext
    {
        PostProcTile[][] PostProcGrid { get; }
    }

    public interface IUnbreakableGenContext : ITiledGenContext
    {
        ITile UnbreakableTerrain { get; }
    }

    public interface IGroupPlaceableGenContext<E> : ITiledGenContext
        where E : IGroupSpawnable
    {
        List<Loc> GetFreeTiles(Rect rect);
        bool CanPlaceItem(Loc loc);
        void PlaceItems(E itemBatch, Loc[] locs);
    }

    public interface IGroupSpawnable
    {

    }
}
