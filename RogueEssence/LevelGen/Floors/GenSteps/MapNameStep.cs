using System;
using RogueElements;


namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MapNameStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        public LocalText Name;

        public MapNameStep()
        {
            Name = new LocalText();

        }
        public MapNameStep(LocalText name)
        {
            Name = new LocalText(name);
        }

        public override void Apply(T map)
        {
            map.Map.Name = new LocalText(Name);
            map.DropTitle = true;
        }
    }


}
