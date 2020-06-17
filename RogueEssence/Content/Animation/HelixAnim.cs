using System;
using RogueElements;

namespace RogueEssence.Content
{
    public class HelixAnim : LoopingAnim
    {
        public HelixAnim(AnimData anim, int totalTime, Loc origin, int radius, int degreesStart, int cycleSpeed, int locHeight, int heightSpeed)
            : base(anim, totalTime)
        {
            Origin = origin;
            Radius = radius;
            DegreesStart = degreesStart;
            CycleSpeed = cycleSpeed;
            StartHeight = locHeight;
            HeightSpeed = heightSpeed;

            locHeight = StartHeight;
            this.mapLoc = Origin + GetCycle(Radius, Radius / 2, DegreesStart);
            Direction = GetCycle(Radius, Radius / 2, DegreesStart + 90 * (CycleSpeed < 0 ? -1 : 1)).ApproximateDir8();
        }


        public Loc Origin;

        public int Radius;

        public int DegreesStart;

        /// <summary>
        /// Degrees Per Second
        /// </summary>
        public int CycleSpeed;

        public int StartHeight;

        /// <summary>
        /// Pixels Per Second
        /// </summary>
        public int HeightSpeed;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            base.Update(scene, elapsedTime);

            int degrees = DegreesStart + (int)ActionTime.FractionOf(CycleSpeed, GraphicsManager.MAX_FPS);
            locHeight = StartHeight + (int)ActionTime.FractionOf(HeightSpeed, GraphicsManager.MAX_FPS);
            mapLoc = Origin + GetCycle(Radius, Radius / 2, degrees);
            Direction = GetCycle(Radius, Radius / 2, degrees + 90 * (CycleSpeed < 0 ? -1 : 1)).ApproximateDir8();

        }


        public static Loc GetCycle(int radiusX, int radiusY, int degrees)
        {
            double x = Math.Cos(degrees * Math.PI / 180) * radiusX;
            double y = Math.Sin(degrees * Math.PI / 180) * radiusY;
            return new Loc((int)Math.Round(x), (int)Math.Round(y));
        }
    }
}