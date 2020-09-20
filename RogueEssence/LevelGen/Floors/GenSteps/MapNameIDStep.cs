using System;
using RogueElements;


namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MapNameIDStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        public int ID;
        public LocalText Name;

        public MapNameIDStep()
        {
            Name = new LocalText();

        }
        public MapNameIDStep(int id, LocalText name)
        {
            ID = id;
            Name = new LocalText(name);
        }

        public override void Apply(T map)
        {
            map.Map.ID = ID;
            map.Map.Name = new LocalText(Name);
            map.DropTitle = true;
        }
    }


}
