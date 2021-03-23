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

        public bool LayeredGround;
        public bool IndependentGround;

        [Dev.DataType(0, DataManager.DataType.Element, false)]
        public int GroundElement;

        public MapTextureStep() { }

        public override void Apply(T map)
        {
            map.Map.BlankBG = new AutoTile(BlockTileset);
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
            for(int ii = 3; ii < DataManager.Instance.DataIndices[DataManager.DataType.Terrain].Count; ii++)
                map.Map.TextureMap[ii] = new AutoTile(WaterTileset, GroundTileset);

            map.Map.Element = GroundElement;
            if (LayeredGround)
            {
                for (int xx = 0; xx < map.Width; xx++)
                {
                    for (int yy = 0; yy < map.Height; yy++)
                        map.Tiles[xx][yy].FloorTile = new AutoTile(GroundTileset);
                }
            }
        }


        public override string ToString()
        {
            string ground = DataManager.Instance.DataIndices[DataManager.DataType.AutoTile].Entries[GroundTileset].Name.ToLocal();
            string wall = DataManager.Instance.DataIndices[DataManager.DataType.AutoTile].Entries[BlockTileset].Name.ToLocal();
            string secondary = DataManager.Instance.DataIndices[DataManager.DataType.AutoTile].Entries[WaterTileset].Name.ToLocal();
            return String.Format("{0}: {1}/{2}/{3}", this.GetType().Name, ground, wall, secondary);
        }
    }

}
