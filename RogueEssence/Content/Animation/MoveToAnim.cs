using RogueElements;

namespace RogueEssence.Content
{
    public class MoveToAnim : LoopingAnim
    {
        public MoveToAnim(AnimData anim, IEmittable emittable, DrawLayer layer, int moveTime, Loc startLoc, Loc newEndPos, int startHeight, int endHeight, int lingerStart, int lingerEnd, Dir8 dir)
            : base(anim, moveTime + lingerStart + lingerEnd)
        {
            MovingTime = moveTime;
            StartLoc = startLoc;
            EndLoc = newEndPos;
            StartHeight = startHeight;
            EndHeight = endHeight;
            Direction = dir;
            LingerStart = lingerStart;
            LingerEnd = lingerEnd;

            locHeight = StartHeight;
            mapLoc = StartLoc;
            
            ResultAnim = emittable;
            Layer = layer;
        }

        public int LingerStart;
        public int MovingTime;
        public int LingerEnd;


        public Loc StartLoc;
        public Loc EndLoc;

        public int StartHeight;
        public int EndHeight;

        public IEmittable ResultAnim;
        public DrawLayer Layer;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            base.Update(scene, elapsedTime);
            if (ActionTime < LingerStart)
            {
                locHeight = StartHeight;
                mapLoc = StartLoc;
            }
            else if (ActionTime >= MovingTime + LingerStart)
            {
                locHeight = EndHeight;
                mapLoc = EndLoc;
            }
            else
            {
                FrameTick midTime = ActionTime - LingerStart;
                locHeight = StartHeight + (int)midTime.FractionOf((EndHeight - StartHeight), MovingTime);
                mapLoc = StartLoc + (EndLoc - StartLoc) * midTime.ToFrames() / MovingTime;
            }

            if (Finished)
                scene.Anims[(int)Layer].Add(ResultAnim.CreateStatic(MapLoc, LocHeight, Direction));
        }
    }
}