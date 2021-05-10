using System;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MapGenEntrance : IEntrance
    {
        public Loc Loc { get; set; }
        public Dir8 Dir { get; set; }

        public MapGenEntrance() { }

        public MapGenEntrance(Dir8 dir)
        {
            Dir = dir;
        }

        public MapGenEntrance(Loc loc, Dir8 dir)
        {
            Loc = loc;
            Dir = dir;
        }

        protected MapGenEntrance(MapGenEntrance other)
        {
            Loc = other.Loc;
            Dir = other.Dir;
        }

        public ISpawnable Copy() { return new MapGenEntrance(this); }

        public override string ToString()
        {
            return String.Format("{0} Dir:{1}", Loc.ToString(), Dir.ToString());
        }
    }

    [Serializable]
    public class MapGenExit : IExit
    {
        public Loc Loc { get; set; }
        public EffectTile Tile { get; set; }

        public MapGenExit() { Tile = new EffectTile(); }

        public MapGenExit(EffectTile tile)
        {
            Tile = tile;
        }

        public MapGenExit(Loc loc, EffectTile tile)
        {
            Loc = loc;
            Tile = tile;
        }

        protected MapGenExit(MapGenExit other)
        {
            Loc = other.Loc;
            Tile = (EffectTile)other.Tile.Copy();
        }
        public ISpawnable Copy() { return new MapGenExit(this); }

        public override string ToString()
        {
            return String.Format("{0} Tile:{1}", Loc.ToString(), Tile.ToString());
        }
    }
}
