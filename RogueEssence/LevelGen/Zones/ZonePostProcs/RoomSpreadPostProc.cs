using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class RoomSpreadPostProc : ZonePostProc
    {
        public RandRange FloorSpacing;
        public IntRange FloorRange;
        //this is heavily hardcoded
        public SpawnList<RoomGenOption> Spawns;
        public int PriorityGrid;
        public int PriorityList;

        [NonSerialized]
        private HashSet<int> dropPoints;
        //spreads an sure-to-happen room through the floors
        //ensures that there is a total between certain values
        //ensures that the space in floors between occurrences is kept tame
        public RoomSpreadPostProc()
        {
            Spawns = new SpawnList<RoomGenOption>();
        }
        public RoomSpreadPostProc(int priorityGrid, int priorityList) : this()
        {
            PriorityGrid = priorityGrid;
            PriorityList = priorityList;
        }

        public RoomSpreadPostProc(int priorityGrid, int priorityList, RandRange spacing, IntRange floorRange) : this(priorityGrid, priorityList)
        {
            FloorSpacing = spacing;
            FloorRange = floorRange;
        }

        protected RoomSpreadPostProc(RoomSpreadPostProc other, ulong seed) : this()
        {
            Spawns = other.Spawns;

            FloorSpacing = other.FloorSpacing;
            FloorRange = other.FloorRange;
            PriorityGrid = other.PriorityGrid;
            PriorityList = other.PriorityList;


            ReRandom rand = new ReRandom(seed);
            dropPoints = new HashSet<int>();

            int currentFloor = FloorRange.Min;
            currentFloor += rand.Next(FloorSpacing.Max);

            while (currentFloor < FloorRange.Max)
            {
                dropPoints.Add(currentFloor);
                currentFloor += FloorSpacing.Pick(rand);
            }
        }
        public override ZonePostProc Instantiate(ulong seed) { return new RoomSpreadPostProc(this, seed); }

        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<int, IGenStep> queue)
        {
            //find the first postproc that is a GridRoom postproc and add this to its special rooms
            //NOTE: if a room-based generator is not found as the generation step, it will just skip this floor but treat it as though it was placed.
            if (dropPoints.Contains(zoneContext.CurrentID))
            {
                //TODO: allow arbitrary components to be added
                RoomGenOption genDuo = Spawns.Pick(context.Rand);
                SetGridSpecialRoomStep<MapGenContext> specialStep = new SetGridSpecialRoomStep<MapGenContext>();
                SetSpecialRoomStep<ListMapGenContext> listSpecialStep = new SetSpecialRoomStep<ListMapGenContext>();

                specialStep.Filters = genDuo.Filters;
                if (specialStep.CanApply(context))
                {
                    specialStep.Rooms = new PresetPicker<RoomGen<MapGenContext>>(genDuo.GridOption);
                    specialStep.RoomComponents.Set(new ImmutableRoom());
                    queue.Enqueue(PriorityGrid, specialStep);
                }
                else if (listSpecialStep.CanApply(context))
                {
                    listSpecialStep.Rooms = new PresetPicker<RoomGen<ListMapGenContext>>(genDuo.ListOption);
                    listSpecialStep.RoomComponents.Set(new ImmutableRoom());
                    PresetPicker<PermissiveRoomGen<ListMapGenContext>> picker = new PresetPicker<PermissiveRoomGen<ListMapGenContext>>();
                    picker.ToSpawn = new RoomGenAngledHall<ListMapGenContext>(0);
                    listSpecialStep.Halls = picker;
                    queue.Enqueue(PriorityList, listSpecialStep);
                }
            }
        }
    }

    [Serializable]
    public class RoomGenOption
    {
        public RoomGen<MapGenContext> GridOption;
        public RoomGen<ListMapGenContext> ListOption;

        public List<BaseRoomFilter> Filters;

        public RoomGenOption(RoomGen<MapGenContext> gridOption, RoomGen<ListMapGenContext> listOption, List<BaseRoomFilter> filters)
        {
            GridOption = gridOption;
            ListOption = listOption;
            Filters = filters;
        }
    }
}
