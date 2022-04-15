using System;
using System.Runtime.Serialization;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class GridFloorGen : FloorMapGen<MapGenContext>
    {
        //public Map ExtractMap(ulong seed)
        //{
        //    return ((MapGenContext)GenMap(seed)).Map;
        //}


        public override string ToString()
        {
            string startInfo = "[EMPTY]";
            foreach (GenStep<MapGenContext> step in GenSteps.EnumerateInOrder())
            {
                var startStep = step as InitGridPlanStep<MapGenContext>;
                if (startStep != null)
                {
                    startInfo = string.Format("Cells:{0}x{1} CellSize:{2}x{3}", startStep.CellX, startStep.CellY, startStep.CellWidth, startStep.CellHeight);
                    break;
                }
            }
            return String.Format("{0}: {1}", this.GetType().Name, startInfo);
        }

        //TODO: Created v0.5.11, delete on v1.0.0
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            foreach (GenStep<MapGenContext> step in this.GenSteps.EnumerateInOrder())
            {
                WaterStep<MapGenContext> waterStep = step as WaterStep<MapGenContext>;
                if (waterStep != null && waterStep.TerrainStencil == null)
                    waterStep.TerrainStencil = new MapTerrainStencil<MapGenContext>(false, true, false);
            }
        }
    }

    [Serializable]
    public class RoomFloorGen : FloorMapGen<ListMapGenContext>
    {
        //public Map ExtractMap(ulong seed)
        //{
        //    return ((MapGenContext)GenMap(seed)).Map;
        //}

        public override string ToString()
        {
            string startInfo = "[EMPTY]";
            foreach (GenStep<ListMapGenContext> step in GenSteps.EnumerateInOrder())
            {
                var startStep = step as InitFloorPlanStep<ListMapGenContext>;
                if (startStep != null)
                {
                    startInfo = string.Format("Size:{0}x{1}", startStep.Width, startStep.Height);
                    break;
                }
            }
            return String.Format("{0}: {1}", this.GetType().Name, startInfo);
        }

        //TODO: Created v0.5.11, delete on v1.0.0
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            foreach (GenStep<ListMapGenContext> step in this.GenSteps.EnumerateInOrder())
            {
                WaterStep<ListMapGenContext> waterStep = step as WaterStep<ListMapGenContext>;
                if (waterStep != null && waterStep.TerrainStencil == null)
                    waterStep.TerrainStencil = new MapTerrainStencil<ListMapGenContext>(false, true, false);
            }
        }
    }

    [Serializable]
    public class StairsFloorGen : FloorMapGen<StairsMapGenContext>
    {
        //public Map ExtractMap(ulong seed)
        //{
        //    return ((MapGenContext)GenMap(seed)).Map;
        //}

        public override string ToString()
        {
            string startInfo = "[EMPTY]";
            foreach (GenStep<StairsMapGenContext> step in GenSteps.EnumerateInOrder())
            {
                var startStep = step as InitTilesStep<StairsMapGenContext>;
                if (startStep != null)
                {
                    startInfo = string.Format("Size:{0}x{1}", startStep.Width, startStep.Height);
                    break;
                }
            }
            return String.Format("{0}: {1}", this.GetType().Name, startInfo);
        }

        //TODO: Created v0.5.11, delete on v1.0.0
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            foreach (GenStep<StairsMapGenContext> step in this.GenSteps.EnumerateInOrder())
            {
                WaterStep<StairsMapGenContext> waterStep = step as WaterStep<StairsMapGenContext>;
                if (waterStep != null && waterStep.TerrainStencil == null)
                    waterStep.TerrainStencil = new MapTerrainStencil<StairsMapGenContext>(false, true, false);
            }
        }
    }

    [Serializable]
    public class LoadGen : FloorMapGen<MapLoadContext>
    {
        //public Map ExtractMap(ulong seed)
        //{
        //    return ((MapGenContext)GenMap(seed)).Map;
        //}

        public override string ToString()
        {
            string startInfo = "[EMPTY]";
            foreach (GenStep<MapLoadContext> step in GenSteps.EnumerateInOrder())
            {
                var startStep = step as MappedRoomStep<MapLoadContext>;
                if (startStep != null)
                {
                    startInfo = string.Format("Map:{0}", startStep.MapID);
                    break;
                }
            }
            return String.Format("{0}: {1}", this.GetType().Name, startInfo);
        }

        //TODO: Created v0.5.11, delete on v1.0.0
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            foreach (GenStep<MapLoadContext> step in this.GenSteps.EnumerateInOrder())
            {
                WaterStep<MapLoadContext> waterStep = step as WaterStep<MapLoadContext>;
                if (waterStep != null && waterStep.TerrainStencil == null)
                    waterStep.TerrainStencil = new MapTerrainStencil<MapLoadContext>(false, true, false);
            }
        }
    }

    [Serializable]
    public class FloorMapGen<T> : MapGen<T>, IFloorGen
        where T : class, IGenContext
    {
        public IGenContext GenMap(ZoneGenContext zoneContext)
        {
            //it will first create the instance of the context,
            //and set up a local List<GenPriority<IGenStep>> variable 
            //the zonecontext members include runtime zonepostprocs
            //that will appreciate a IGenContext + List<GenPriority<IGenStep>> being passed into them
            //then, gensteps will continue on as per usual

            T map = (T)Activator.CreateInstance(typeof(T));
            map.InitSeed(zoneContext.Seed);

            GenContextDebug.DebugInit(map);

            //postprocessing steps:
            StablePriorityQueue<Priority, IGenStep> queue = new StablePriorityQueue<Priority, IGenStep>();
            foreach (Priority priority in GenSteps.GetPriorities())
            {
                foreach (IGenStep genStep in GenSteps.GetItems(priority))
                    queue.Enqueue(priority, genStep);
            }

            foreach (ZoneStep zoneStep in zoneContext.ZoneSteps)
                zoneStep.Apply(zoneContext, map, queue);

            ApplyGenSteps(map, queue);

            map.FinishGen();

            return map;
        }
    }

    public interface IFloorGen
    {
        //Map ExtractMap(ulong seed);
        IGenContext GenMap(ZoneGenContext zoneContext);
    }
}
