using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Turns the tiles at the edge of the map into the Unbreakable terrain.
    /// Thickness can be customized.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class LoadBlobStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        /// <summary>
        /// Map filea to load.
        /// </summary>
        [Dev.DataFolder(1, "Map/")]
        public SpawnList<string> Maps;

        public PostProcType PreventChanges { get; set; }

        /// <summary>
        /// Determines which tiles are eligible to be painted on.
        /// </summary>
        public ITerrainStencil<T> TerrainStencil { get; set; }

        public RandRange Amount { get; set; }

        public LoadBlobStep()
        {
            Maps = new SpawnList<string>();
        }


        public override void Apply(T map)
        {
            if (!Maps.CanPick)
                return;

            int amt = this.Amount.Pick(map.Rand);
            Dictionary<string, Map> mapCache = new Dictionary<string, Map>();

            for (int ii = 0; ii < amt; ii++)
            {
                for (int jj = 0; jj < 30; jj++)
                {
                    bool placed = false;
                    string chosenMapID = Maps.Pick(map.Rand);
                    Map placeMap;
                    if (!mapCache.TryGetValue(chosenMapID, out placeMap))
                    {
                        placeMap = DataManager.Instance.GetMap(chosenMapID);
                        mapCache[chosenMapID] = placeMap;
                    }

                    Loc offset = new Loc(map.Rand.Next(0, map.Width - placeMap.Width), map.Rand.Next(0, map.Height - placeMap.Height));

                    if (canDrawBlob(map, placeMap, offset))
                        DrawBlob(map, placeMap, offset);

                    if (placed)
                        break;
                }
            }
        }

        protected bool canDrawBlob(T map, Map mapBlob, Loc offset)
        {
            if (!this.TerrainStencil.Test(map, new Rect(offset, new Loc(mapBlob.Width, mapBlob.Height))))
                return false;

            for (int xx = 0; xx < mapBlob.Width; xx++)
            {
                for (int yy = 0; yy < mapBlob.Height; yy++)
                {
                    if (map.GetPostProc(new Loc(offset.X + xx, offset.Y + yy)).Status != PostProcType.None)
                        return false;
                }
            }
            return true;
        }

        protected void DrawBlob(T map, Map mapBlob, Loc offset)
        {
            //add needed layers
            Dictionary<int, int> layerMap = new Dictionary<int, int>();
            Dictionary<Content.DrawLayer, int> drawOrderDict = new Dictionary<Content.DrawLayer, int>();
            for (int ii = 0; ii < mapBlob.Layers.Count; ii++)
            {
                if (!mapBlob.Layers[ii].Visible)
                    continue;

                // find the next layer that has the same draw layer as this one
                int layerStart;
                if (!drawOrderDict.TryGetValue(mapBlob.Layers[ii].Layer, out layerStart))
                    layerStart = 0;
                for (; layerStart < map.Map.Layers.Count; layerStart++)
                {
                    if (map.Map.Layers[layerStart].Layer == mapBlob.Layers[ii].Layer)
                    {
                        //TODO: also check that the region is not drawn on already
                        break;
                    }
                }
                //add it if it doesn't exist
                if (layerStart == map.Map.Layers.Count)
                {
                    map.Map.AddLayer(mapBlob.Layers[ii].Name);
                    map.Map.Layers[map.Map.Layers.Count - 1].Layer = mapBlob.Layers[ii].Layer;
                }
                //set the new layer start variable for which to continue checking from
                layerMap[ii] = layerStart;
                drawOrderDict[mapBlob.Layers[ii].Layer] = layerStart + 1;
            }

            //draw the tiles
            for (int xx = 0; xx < mapBlob.Width; xx++)
            {
                for (int yy = 0; yy < mapBlob.Height; yy++)
                {
                    map.SetTile(new Loc(offset.X + xx, offset.Y + yy), mapBlob.Tiles[xx][yy].Copy());
                    for (int ii = 0; ii < mapBlob.Layers.Count; ii++)
                    {
                        int layerTo;
                        if (layerMap.TryGetValue(ii, out layerTo))
                        {
                            Loc wrapLoc = offset + new Loc(xx, yy);
                            if (map.Map.GetLocInMapBounds(ref wrapLoc))
                                map.Map.Layers[layerTo].Tiles[wrapLoc.X][wrapLoc.Y] = mapBlob.Layers[ii].Tiles[xx][yy].Copy();
                            else
                                throw new IndexOutOfRangeException("Attempted to draw custom room graphics out of range!");
                        }
                    }
                }
            }

            //place items
            foreach (MapItem preItem in mapBlob.Items)
            {
                MapItem item = new MapItem(preItem);
                Loc wrapLoc = item.TileLoc + offset;
                if (map.Map.GetLocInMapBounds(ref wrapLoc))
                    item.TileLoc = wrapLoc;
                else
                    throw new IndexOutOfRangeException("Attempted to draw custom room item out of range!");
                map.Items.Add(item);
            }

            //place mobs
            foreach (Team preTeam in mapBlob.MapTeams)
            {
                Team team = preTeam.Clone();
                foreach (Character member in team.EnumerateChars())
                {
                    Loc wrapLoc = member.CharLoc + offset;
                    if (map.Map.GetLocInMapBounds(ref wrapLoc))
                        member.CharLoc = wrapLoc;
                    else
                        throw new IndexOutOfRangeException("Attempted to draw custom room enemy out of range!");
                }
                map.MapTeams.Add(team);
            }

            //place map entrances
            foreach (LocRay8 entrance in mapBlob.EntryPoints)
            {
                Loc wrapLoc = entrance.Loc + offset;
                if (map.Map.GetLocInMapBounds(ref wrapLoc))
                    map.Map.EntryPoints.Add(new LocRay8(wrapLoc, entrance.Dir));
                else
                    throw new IndexOutOfRangeException("Attempted to draw custom room entrance out of range!");
            }

            //set locks
            for (int xx = 0; xx < mapBlob.Width; xx++)
            {
                for (int yy = 0; yy < mapBlob.Height; yy++)
                    map.GetPostProc(new Loc(offset.X + xx, offset.Y + yy)).AddMask(new PostProcTile(PreventChanges));
            }

            GenContextDebug.DebugProgress("Draw Blob");
        }
    }

}
