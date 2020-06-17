using System;
using RogueEssence.Dungeon;
using RogueElements;
using RogueEssence.Data;


namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MapTextureStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        [Dev.DataType(0, DataManager.DataType.AutoTile, false)]
        public int GroundTileset;
        [Dev.DataType(0, DataManager.DataType.AutoTile, false)]
        public int BlockTileset;
        [Dev.DataType(0, DataManager.DataType.AutoTile, false)]
        public int WaterTileset;

        public bool IndependentGround;

        [Dev.DataType(0, DataManager.DataType.Element, false)]
        public int GroundElement;

        public MapTextureStep() { }

        public override void Apply(T map)
        {

            AutoTileData entry = DataManager.Instance.GetAutoTile(BlockTileset);
            map.Map.BlankBG = new AutoTile(entry.Tiles.Generic);
            map.Map.FloorBG = new AutoTile(GroundTileset, IndependentGround ? -1 : BlockTileset);
            map.Map.TextureMap[1] = new AutoTile(BlockTileset, -1);
            map.Map.TextureMap[2] = new AutoTile(BlockTileset, -1);
            map.Map.TextureMap[3] = new AutoTile(WaterTileset, -1);
            map.Map.TextureMap[4] = new AutoTile(WaterTileset, -1);
            map.Map.TextureMap[5] = new AutoTile(WaterTileset, -1);
            map.Map.Element = GroundElement;
        }

    }

}
