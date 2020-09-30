using System;
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
    }

    [Serializable]
    public class RoomFloorGen : FloorMapGen<ListMapGenContext>
    {
        //public Map ExtractMap(ulong seed)
        //{
        //    return ((MapGenContext)GenMap(seed)).Map;
        //}
    }

    [Serializable]
    public class StairsFloorGen : FloorMapGen<StairsMapGenContext>
    {
        //public Map ExtractMap(ulong seed)
        //{
        //    return ((MapGenContext)GenMap(seed)).Map;
        //}
    }

    [Serializable]
    public class LoadGen : FloorMapGen<MapLoadContext>
    {
        //public Map ExtractMap(ulong seed)
        //{
        //    return ((MapGenContext)GenMap(seed)).Map;
        //}
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

            foreach (ZonePostProc zoneStep in zoneContext.ZoneSteps)
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
