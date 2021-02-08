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
            map.Map.BlankBG = new AutoTile(BlockTileset);
            map.Map.FloorBG = new AutoTile(GroundTileset);
            map.Map.TextureMap[0] = new AutoTile(GroundTileset);
            if (IndependentGround)
            {
                map.Map.TextureMap[1] = new AutoTile(BlockTileset, GroundTileset);
                map.Map.TextureMap[2] = new AutoTile(BlockTileset, GroundTileset);
            }
            else
            {
                map.Map.TextureMap[1] = new AutoTile(BlockTileset);
                map.Map.TextureMap[2] = new AutoTile(BlockTileset);
            }
            map.Map.TextureMap[3] = new AutoTile(WaterTileset, GroundTileset);
            map.Map.TextureMap[4] = new AutoTile(WaterTileset, GroundTileset);
            map.Map.TextureMap[5] = new AutoTile(WaterTileset, GroundTileset);
            map.Map.Element = GroundElement;
        }

    }

}
