using RogueElements;
using System;

namespace RogueEssence.Content
{
    public class SwingAnim : LoopingAnim
    {
        public SwingAnim(AnimData anim, int moveTime, Loc startLoc, Loc newEndPos, float axisRatio, Dir8 dir)
            : base(anim, moveTime)
        {
            MovingTime = moveTime;
            StartLoc = startLoc;
            EndLoc = newEndPos;
            AxisRatio = axisRatio;
            Direction = dir;
            mapLoc = StartLoc;
        }

        public int MovingTime;
        public float AxisRatio;


        public Loc StartLoc;
        public Loc EndLoc;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            base.Update(scene, elapsedTime);

            //a=EndLoc - StartLoc
            //b=(-a.y, a.x)/||a||*r*||a||
            //b=(-a.y, a.x)*r
            //Va=t*a
            //Vb=4*b*t*(1-t)
            
            FrameTick midTime = ActionTime;
            Loc majorVector = EndLoc - StartLoc;
            Loc minorVector = new Loc(-majorVector.Y, majorVector.X);
            Loc majorDiff = majorVector * ActionTime.ToFrames() / MovingTime;
            double div = Math.Sqrt(majorDiff.DistSquared());
            Loc minorDiff = 4 * minorVector * ActionTime.ToFrames() * (MovingTime - ActionTime.ToFrames()) / MovingTime / MovingTime;
            mapLoc = StartLoc + majorDiff + new Loc((int)(minorDiff.X * AxisRatio), (int)(minorDiff.Y * AxisRatio));
        }
    }
}