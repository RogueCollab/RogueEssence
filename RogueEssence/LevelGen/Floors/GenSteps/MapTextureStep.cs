using System;
using RogueEssence.Dungeon;
using RogueElements;
using RogueEssence.Data;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        [JsonConverter(typeof(Dev.AutotileConverter))]
        [Dev.DataType(0, DataManager.DataType.AutoTile, false)]
        public string GroundTileset;

        /// <summary>
        /// The tileset used for walls, both breakable and unbreakable.
        /// </summary>
        [JsonConverter(typeof(Dev.AutotileConverter))]
        [Dev.DataType(0, DataManager.DataType.AutoTile, false)]
        public string BlockTileset;

        /// <summary>
        /// The tileset used for water, lava, etc.
        /// </summary>
        [JsonConverter(typeof(Dev.AutotileConverter))]
        [Dev.DataType(0, DataManager.DataType.AutoTile, false)]
        public string WaterTileset;

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
            map.Map.TextureMap[DataManager.Instance.GenFloor] = new AutoTile(GroundTileset);
            if (IndependentGround)
            {
                map.Map.TextureMap[DataManager.Instance.GenUnbreakable] = new AutoTile(BlockTileset, GroundTileset);
                map.Map.TextureMap[DataManager.Instance.GenWall] = new AutoTile(BlockTileset, GroundTileset);
            }
            else
            {
                map.Map.TextureMap[DataManager.Instance.GenUnbreakable] = new AutoTile(BlockTileset);
                map.Map.TextureMap[DataManager.Instance.GenWall] = new AutoTile(BlockTileset);
            }

            foreach (string key in DataManager.Instance.DataIndices[DataManager.DataType.Terrain].Entries.Keys)
            {
                if (key != DataManager.Instance.GenFloor && key != DataManager.Instance.GenUnbreakable && key != DataManager.Instance.GenWall)
                    map.Map.TextureMap[key] = new AutoTile(WaterTileset, GroundTileset);
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
        [JsonConverter(typeof(Dev.TerrainDictAutotileDataConverter))]
        [Dev.DataType(1, DataManager.DataType.Terrain, false)]
        [Dev.DataType(2, DataManager.DataType.AutoTile, false)]
        public Dictionary<string, string> TextureMap;

        /// <summary>
        /// The repeated texture used for the border.
        /// </summary>
        [JsonConverter(typeof(Dev.AutotileConverter))]
        public string BlankBG;

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

        public MapDictTextureStep()
        {
            TextureMap = new Dictionary<string, string>();
            BlankBG = "";
        }

        public override void Apply(T map)
        {
            map.Map.BlankBG = new AutoTile(BlankBG);
            foreach (string terrain in TextureMap.Keys)
            {
                if (terrain == DataManager.Instance.GenFloor)//assume ground
                    map.Map.TextureMap[terrain] = new AutoTile(TextureMap[terrain]);
                else
                    map.Map.TextureMap[terrain] = new AutoTile(TextureMap[terrain], TextureMap[DataManager.Instance.GenFloor]);
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
                        layer.Tiles[xx][yy] = new AutoTile(TextureMap[DataManager.Instance.GenFloor]);
                }
            }
        }


        public override string ToString()
        {
            return String.Format("{0}[{1}]", this.GetType().Name, TextureMap.Count);
        }
    }
}
