using System;
using RogueEssence.Dungeon;
using RogueElements;
using RogueEssence.Data;
using System.Collections.Generic;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Decides the tileset for the walls, ground, etc.
    /// This is done by setting the map's TextureMap to the specified textures.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MapTextureStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        /// <summary>
        /// The tileset used for walkable tiles.
        /// </summary>
        [Dev.DataType(0, DataManager.DataType.AutoTile, false)]
        public int GroundTileset;

        /// <summary>
        /// The tileset used for walls, both breakable and unbreakable.
        /// </summary>
        [Dev.DataType(0, DataManager.DataType.AutoTile, false)]
        public int BlockTileset;

        /// <summary>
        /// The tileset used for water, lava, etc.
        /// </summary>
        [Dev.DataType(0, DataManager.DataType.AutoTile, false)]
        public int WaterTileset;

        /// <summary>
        /// Adds an additional ground texture beneath all textures.
        /// Useful for wall textures that contain transparent pixels.
        /// </summary>
        public bool LayeredGround;

        /// <summary>
        /// Turns off border textures for the ground tileset when near walls.
        /// </summary>
        public bool IndependentGround;

        /// <summary>
        /// The map's elemental aligment.
        /// </summary>
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
                map.Map.AddLayer("Under");
                MapLayer layer = map.Map.Layers[map.Map.Layers.Count - 1];
                layer.Layer = Content.DrawLayer.Under;
                for (int xx = 0; xx < map.Width; xx++)
                {
                    for (int yy = 0; yy < map.Height; yy++)
                        layer.Tiles[xx][yy] = new AutoTile(GroundTileset);
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


    /// <summary>
    /// Decides the tileset for the walls, ground, etc.
    /// This is done by setting the map's TextureMap to the specified textures.
    /// A more fine-tuned version of MapTextureStep that allows mapping of more than just Ground+Wall+Secondary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MapDictTextureStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        /// <summary>
        /// Maps the terrain type to the specified autotile.
        /// </summary>
        [Dev.DataType(1, DataManager.DataType.Terrain, false)]
        [Dev.DataType(2, DataManager.DataType.AutoTile, false)]
        public Dictionary<int, int> TextureMap;

        /// <summary>
        /// The texture considered to be the ground for this map.
        /// </summary>
        public int GroundTexture;

        /// <summary>
        /// The repeated texture used for the border.
        /// </summary>
        public int BlankBG;

        /// <summary>
        /// Adds an additional ground texture beneath all textures.
        /// Useful for wall textures that contain transparent pixels.
        /// </summary>
        public bool LayeredGround;

        /// <summary>
        /// Turns off border textures for the ground tileset when near walls.
        /// </summary>
        public bool IndependentGround;

        /// <summary>
        /// The map's elemental aligment.
        /// </summary>
        [Dev.DataType(0, DataManager.DataType.Element, false)]
        public int GroundElement;

        public MapDictTextureStep() { TextureMap = new Dictionary<int, int>(); }

        public override void Apply(T map)
        {
            map.Map.BlankBG = new AutoTile(BlankBG);
            foreach (int terrain in TextureMap.Keys)
            {
                if (terrain == 0)//assume ground
                    map.Map.TextureMap[terrain] = new AutoTile(TextureMap[terrain]);
                else
                    map.Map.TextureMap[terrain] = new AutoTile(TextureMap[terrain], TextureMap[GroundTexture]);
            }

            map.Map.Element = GroundElement;
            if (LayeredGround)
            {
                map.Map.AddLayer("Under");
                MapLayer layer = map.Map.Layers[map.Map.Layers.Count - 1];
                layer.Layer = Content.DrawLayer.Under;
                for (int xx = 0; xx < map.Width; xx++)
                {
                    for (int yy = 0; yy < map.Height; yy++)
                        layer.Tiles[xx][yy] = new AutoTile(TextureMap[GroundTexture]);
                }
            }
        }


        public override string ToString()
        {
            return String.Format("{0}[{1}]", this.GetType().Name, TextureMap.Count);
        }
    }
}
