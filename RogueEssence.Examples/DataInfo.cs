using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;
using RogueEssence.Content;
using RogueElements;
using RogueEssence;
using RogueEssence.Data;
using System.IO;
using RogueEssence.Ground;
using RogueEssence.LevelGen;
using RogueEssence.Script;

namespace RogueEssence.Examples
{
    public static class DataInfo
    {
        public static void AddUniversalEvent()
        {
            File.Delete(PathMod.ModPath(DataManager.DATA_PATH + "Universal.bin"));
            ActiveEffect universalEvent = new ActiveEffect();
            DataManager.SaveData(PathMod.ModPath(DataManager.DATA_PATH + "Universal.bin"), universalEvent);
        }

        public static void AddUniversalData()
        {
            File.Delete(PathMod.ModPath(DataManager.MISC_PATH + "Index.bin"));
            TypeDict<BaseData> baseData = new TypeDict<BaseData>();
            DataManager.SaveData(PathMod.ModPath(DataManager.MISC_PATH + "Index.bin"), baseData);
        }

        public static void AddEditorOps()
        {
            DeleteData(Path.Combine(PathMod.RESOURCE_PATH, "Extensions"));
        }

        public static void AddSystemFX()
        {
            DeleteData(PathMod.ModPath(DataManager.FX_PATH));
            {
                BattleFX fx = new BattleFX();
                DataManager.SaveData(PathMod.ModPath(DataManager.FX_PATH + "Heal.fx"), fx);
            }
            {
                BattleFX fx = new BattleFX();
                DataManager.SaveData(PathMod.ModPath(DataManager.FX_PATH + "RestoreCharge.fx"), fx);
            }
            {
                BattleFX fx = new BattleFX();
                DataManager.SaveData(PathMod.ModPath(DataManager.FX_PATH + "LoseCharge.fx"), fx);
            }
            {
                EmoteFX fx = new EmoteFX();
                DataManager.SaveData(PathMod.ModPath(DataManager.FX_PATH + "NoCharge.fx"), fx);
            }
            {
                BattleFX fx = new BattleFX();
                DataManager.SaveData(PathMod.ModPath(DataManager.FX_PATH + "Element.fx"), fx);
            }
            {
                BattleFX fx = new BattleFX();
                DataManager.SaveData(PathMod.ModPath(DataManager.FX_PATH + "Intrinsic.fx"), fx);
            }
            {
                BattleFX fx = new BattleFX();
                DataManager.SaveData(PathMod.ModPath(DataManager.FX_PATH + "SendHome.fx"), fx);
            }
            {
                BattleFX fx = new BattleFX();
                DataManager.SaveData(PathMod.ModPath(DataManager.FX_PATH + "ItemLost.fx"), fx);
            }
            {
                BattleFX fx = new BattleFX();
                DataManager.SaveData(PathMod.ModPath(DataManager.FX_PATH + "Warp.fx"), fx);
            }
            {
                BattleFX fx = new BattleFX();
                DataManager.SaveData(PathMod.ModPath(DataManager.FX_PATH + "Knockback.fx"), fx);
            }
            {
                BattleFX fx = new BattleFX();
                DataManager.SaveData(PathMod.ModPath(DataManager.FX_PATH + "Jump.fx"), fx);
            }
            {
                BattleFX fx = new BattleFX();
                DataManager.SaveData(PathMod.ModPath(DataManager.FX_PATH + "Throw.fx"), fx);
            }
        }

        public static void DeleteIndexedData(string subPath)
        {
            DeleteData(PathMod.ModPath(DataManager.DATA_PATH + subPath));
        }
        public static void DeleteData(string path)
        {
            string[] filePaths = Directory.GetFiles(path);
            foreach (string filePath in filePaths)
                File.Delete(filePath);
        }


        public static void AddElementData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.Element.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                ElementData element = new ElementData(new LocalText(String.Format("{0} {1}", DataManager.DataType.Element.ToString(), ii)), (char)(ii + 0xE080));
                DataManager.SaveData(ii, DataManager.DataType.Element.ToString(), element);
            }
        }

        public static void AddGrowthGroupData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.GrowthGroup.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                List<int> exp = new List<int>();
                for (int jj = 0; jj < 100; jj++)
                    exp.Add(jj * 10);
                GrowthData skillGroup = new GrowthData(new LocalText(String.Format("{0} {1}", DataManager.DataType.GrowthGroup.ToString(), ii)), exp.ToArray());
                DataManager.SaveData(ii, DataManager.DataType.GrowthGroup.ToString(), skillGroup);
            }
        }

        public static void AddSkillGroupData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.SkillGroup.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                SkillGroupData skillGroup = new SkillGroupData(new LocalText(String.Format("{0} {1}", DataManager.DataType.SkillGroup.ToString(), ii)));
                DataManager.SaveData(ii, DataManager.DataType.SkillGroup.ToString(), skillGroup);
            }
        }

        public static void AddEmoteData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.Emote.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                EmoteData emote = new EmoteData(new LocalText(String.Format("{0} {1}", DataManager.DataType.Emote.ToString(), ii)), new AnimData(), 0);
                DataManager.SaveData(ii, DataManager.DataType.Emote.ToString(), emote);
            }
        }

        public static void AddAIData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.AI.ToString());

            for (int ii = 0; ii < 3; ii++)
            {
                AITactic tactic = new AITactic();
                tactic.Name = new LocalText(String.Format("{0} {1}", DataManager.DataType.AI.ToString(), ii));
                tactic.ID = 0;
                DataManager.SaveData(ii, DataManager.DataType.AI.ToString(), tactic);
            }
        }


        public static void AddTileData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.Tile.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                TileData tile = new TileData();
                tile.Name = new LocalText(String.Format("{0} {1}", DataManager.DataType.Tile.ToString(), ii));
                DataManager.SaveData(ii, DataManager.DataType.Tile.ToString(), tile);
            }
        }


        public static void AddTerrainData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.Terrain.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                TerrainData tile = new TerrainData();
                tile.Name = new LocalText(String.Format("{0} {1}", DataManager.DataType.Terrain.ToString(), ii));
                DataManager.SaveData(ii, DataManager.DataType.Terrain.ToString(), tile);
            }
        }

        public static void AddRankData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.Rank.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                RankData data = new RankData(new LocalText(String.Format("{0} {1}", DataManager.DataType.Terrain.ToString(), ii)), 16, 10 * ii);
                DataManager.SaveData(ii, DataManager.DataType.Rank.ToString(), data);
            }
        }

        public static void AddSkinData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.Skin.ToString());

            for (int ii = 0; ii < 3; ii++)
            {
                SkinData data = new SkinData(new LocalText(String.Format("{0} {1}", DataManager.DataType.Skin.ToString(), ii)), '\0');
                DataManager.SaveData(ii, DataManager.DataType.Skin.ToString(), data);
            }
        }



        public static void AddSkillData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.Skill.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                SkillData skill = new SkillData();
                skill.Name = new LocalText(String.Format("{0} {1}", DataManager.DataType.Skill.ToString(), ii));
                DataManager.SaveData(ii, DataManager.DataType.Skill.ToString(), skill);
            }
        }


        public static void AddIntrinsicData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.Intrinsic.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                IntrinsicData intrinsic = new IntrinsicData();
                intrinsic.Name = new LocalText(String.Format("{0} {1}", DataManager.DataType.Intrinsic.ToString(), ii));
                DataManager.SaveData(ii, DataManager.DataType.Intrinsic.ToString(), intrinsic);
            }
        }

        public static void AddMonsterData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.Monster.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                MonsterData entry = new MonsterData();
                entry.Name = new LocalText(String.Format("{0} {1}", DataManager.DataType.Monster.ToString(), ii));
                entry.JoinRate = 0;
                MonsterFormData formEntry = new MonsterFormData();
                formEntry.FormName = new LocalText("Test Form");
                formEntry.LevelSkills.Add(new LevelUpSkill());
                entry.Forms.Add(formEntry);
                DataManager.SaveData(ii, DataManager.DataType.Monster.ToString(), entry);
            }
        }


        public static void AddStatusData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.Status.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                StatusData status = new StatusData();
                status.Name = new LocalText(String.Format("{0} {1}", DataManager.DataType.Status.ToString(), ii));
                DataManager.SaveData(ii, DataManager.DataType.Status.ToString(), status);
            }
        }

        
        public static void AddMapStatusData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.MapStatus.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                MapStatusData status = new MapStatusData();
                status.Name = new LocalText(String.Format("{0} {1}", DataManager.DataType.MapStatus.ToString(), ii));
                DataManager.SaveData(ii, DataManager.DataType.MapStatus.ToString(), status);
            }
        }

        public static void AddItemData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.Item.ToString());
            for (int ii = 0; ii < 3; ii++)
            {
                ItemData item = new ItemData();
                item.Name = new LocalText(String.Format("{0} {1}", DataManager.DataType.Item.ToString(), ii));
                DataManager.SaveData(ii, DataManager.DataType.Item.ToString(), item);
            }
        }



        public static string[] MapNames = { "test_map" };

        public static void AddMapData()
        {
            DataInfo.DeleteData(PathMod.ModPath(DataManager.MAP_PATH));
            for (int ii = 0; ii < MapNames.Length; ii++)
            {
                Map map = new Map();
                map.AssetName = MapNames[ii];
                map.CreateNew(10, 10);
                map.EntryPoints.Add(new LocRay8(new Loc(), Dir8.Down));
                map.Name = new LocalText(String.Format("Map {0}", ii));
                DataManager.SaveData(PathMod.ModPath(DataManager.MAP_PATH + MapNames[ii] + DataManager.MAP_EXT), map);
            }
        }


        public static void AddGroundData()
        {
            DataInfo.DeleteData(PathMod.ModPath(DataManager.GROUND_PATH));
            for (int ii = 0; ii < MapNames.Length; ii++)
            {
                GroundMap map = new GroundMap();
                map.AssetName = MapNames[ii];
                map.CreateNew(10, 10, 3);
                map.Name = new LocalText(String.Format("Ground {0}", ii));
                DataManager.SaveData(PathMod.ModPath(DataManager.GROUND_PATH + MapNames[ii] + DataManager.GROUND_EXT), map);
            }
        }

        
        public static void AddZoneData()
        {
            DataInfo.DeleteIndexedData(DataManager.DataType.Zone.ToString());

            for (int ii = 0; ii < 2; ii++)
            {
                ZoneData zone = GetZoneData(ii);
                DataManager.SaveData(ii, DataManager.DataType.Zone.ToString(), zone);
            }
        }


        public static ZoneData GetZoneData(int index)
        {
            ZoneData zone = new ZoneData();
            if (index == 0)
            {
                zone.Name = new LocalText("Debug Zone");
                {
                    LayeredSegment structure = new LayeredSegment();
                    //Tests Tilesets, and unlockables
                    #region TILESET TESTS
                    int curTileIndex = 0;
                    for (int kk = 0; kk < 5; kk++)
                    {
                        string[] level = {
                            "...........................................",
                            "..........#.....................~..........",
                            "...###...###...###.......~~~...~~~...~~~...",
                            "..#.#.....#.....#.#.....~.~.....~.....~.~..",
                            "..####...###...####.....~~~~...~~~...~~~~..",
                            "..#.#############.#.....~.~~~~~~~~~~~~~.~..",
                            ".....##.......##...........~~.......~~.....",
                            ".....#..#####..#...........~..~~~~~..~.....",
                            ".....#.#######.#...........~.~~~~~~~.~.....",
                            "..#.##.#######.##.#.....~.~~.~~~~~~~.~~.~..",
                            ".#####.###.###.#####...~~~~~.~~~.~~~.~~~~~.",
                            "..#.##.#######.##.#.....~.~~.~~~~~~~.~~.~..",
                            ".....#.#######.#...........~.~~~~~~~.~.....",
                            ".....#..#####..#...........~..~~~~~..~.....",
                            ".....##.......##...........~~.......~~.....",
                            "..#.#############.#.....~.~~~~~~~~~~~~~.~..",
                            "..####...###...####.....~~~~...~~~...~~~~..",
                            "..#.#.....#.....#.#.....~.~.....~.....~.~..",
                            "...###...###...###.......~~~...~~~...~~~...",
                            "..........#.....................~..........",
                            "...........................................",
                        };

                        StairsFloorGen layout = new StairsFloorGen();
                        InitTilesStep<StairsMapGenContext> startStep = new InitTilesStep<StairsMapGenContext>();
                        int width = level[0].Length;
                        int height = level.Length;
                        startStep.Width = width + 2;
                        startStep.Height = height + 2;

                        layout.GenSteps.Add(0, startStep);

                        SpecificTilesStep<StairsMapGenContext> drawStep = new SpecificTilesStep<StairsMapGenContext>();
                        drawStep.Offset = new Loc(1);

                        drawStep.Tiles = new Tile[width][];
                        for (int ii = 0; ii < width; ii++)
                        {
                            drawStep.Tiles[ii] = new Tile[height];
                            for (int jj = 0; jj < height; jj++)
                            {
                                if (level[jj][ii] == '#')
                                    drawStep.Tiles[ii][jj] = new Tile(2);
                                else if (level[jj][ii] == '~')
                                    drawStep.Tiles[ii][jj] = new Tile(3);
                                else
                                    drawStep.Tiles[ii][jj] = new Tile(0);
                            }
                        }

                        layout.GenSteps.Add(0, drawStep);

                        //add border
                        layout.GenSteps.Add(0, new UnbreakableBorderStep<StairsMapGenContext>(1));

                        {
                            List<(MapGenEntrance, Loc)> items = new List<(MapGenEntrance, Loc)>();
                            items.Add((new MapGenEntrance(Dir8.Down), new Loc(22, 11)));
                            AddSpecificSpawn(layout, items, new Priority(2));
                        }
                        {
                            List<(MapGenExit, Loc)> items = new List<(MapGenExit, Loc)>();
                            items.Add((new MapGenExit(new EffectTile(1, true)), new Loc(22, 12)));
                            AddSpecificSpawn(layout, items, new Priority(2));
                        }

                        MapTextureStep<StairsMapGenContext> texturePostProc = new MapTextureStep<StairsMapGenContext>();
                        texturePostProc.BlockTileset = curTileIndex;
                        curTileIndex++;
                        texturePostProc.GroundTileset = curTileIndex;
                        curTileIndex++;
                        texturePostProc.WaterTileset = curTileIndex;
                        curTileIndex++;
                        layout.GenSteps.Add(5, texturePostProc);

                        structure.Floors.Add(layout);
                    }
                    #endregion

                    //structure.MainExit = new ZoneLoc(2, 0);
                    zone.Segments.Add(structure);
                }

                for (int jj = 0; jj < MapNames.Length; jj++)
                    zone.GroundMaps.Add(MapNames[jj]);
            }
            else if (index == 1)
            {
                zone.Name = new LocalText("Main Zone");

                for (int jj = 0; jj < MapNames.Length; jj++)
                    zone.GroundMaps.Add(MapNames[jj]);

                LayeredSegment staticStructure = new LayeredSegment();
                for (int jj = 0; jj < MapNames.Length; jj++)
                {
                    LoadGen layout = new LoadGen();
                    MappedRoomStep<MapLoadContext> startGen = new MappedRoomStep<MapLoadContext>();
                    startGen.MapID = MapNames[jj];
                    layout.GenSteps.Add(0, startGen);
                    staticStructure.Floors.Add(layout);
                }

                zone.Segments.Add(staticStructure);
            }
            return zone;
        }


        static void AddSpecificSpawn<TGenContext, TSpawnable>(MapGen<TGenContext> layout, List<(TSpawnable item, Loc loc)> items, Priority priority)
            where TGenContext : class, IPlaceableGenContext<TSpawnable>
            where TSpawnable : ISpawnable
        {
            PresetMultiRand<TSpawnable> picker = new PresetMultiRand<TSpawnable>();
            List<Loc> spawnLocs = new List<Loc>();
            for (int ii = 0; ii < items.Count; ii++)
            {
                picker.ToSpawn.Add(new PresetPicker<TSpawnable>(items[ii].item));
                spawnLocs.Add(items[ii].loc);
            }
            PickerSpawner<TGenContext, TSpawnable> spawn = new PickerSpawner<TGenContext, TSpawnable>(picker);
            layout.GenSteps.Add(priority, new SpecificSpawnStep<TGenContext, TSpawnable>(spawn, spawnLocs));
        }
    }

}

