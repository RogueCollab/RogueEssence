using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Content
{

    [Serializable]
    public class StreamEmitter : ShootingEmitter
    {
        public override bool Finished { get { return (CurrentShots >= Shots); } }

        public StreamEmitter()
        {
            Anims = new List<IParticleEmittable>();
            Layer = DrawLayer.Normal;
        }
        public StreamEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new ParticleAnim(anim));
        }

        public StreamEmitter(StreamEmitter other)
        {
            Anims = new List<IParticleEmittable>();
            foreach (IParticleEmittable anim in other.Anims)
                Anims.Add((IParticleEmittable)anim.CloneIEmittable());
            Shots = other.Shots;
            BurstTime = other.BurstTime;
            StartDistance = other.StartDistance;
            Range = other.Range;
            Speed = other.Speed;
            LocHeight = other.LocHeight;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new StreamEmitter(this); }

        public List<IParticleEmittable> Anims;
        public int Shots;
        public int BurstTime;
        public int StartDistance;
        public DrawLayer Layer;


        [NonSerialized]
        private FrameTick CurrentShotTime;
        [NonSerialized]
        private int CurrentShots;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            CurrentShotTime += elapsedTime;
            while (CurrentShotTime >= BurstTime)
            {
                CurrentShotTime -= BurstTime;

                Loc particleSpeed = Dir.GetLoc() * Speed;
                Loc startDelta = Dir.GetLoc() * StartDistance;

                int range = Range;
                if (Dir.IsDiagonal())
                    range = (int)(range * 1.4142136);

                int totalTime = range - StartDistance;
                if (Speed > 0)
                {
                    totalTime *= GraphicsManager.MAX_FPS;
                    totalTime /= Speed;
                }

                if (Anims.Count > 0)
                {
                    IParticleEmittable chosenAnim = Anims[CurrentShots % Anims.Count];
                    scene.Anims[(int)Layer].Add(chosenAnim.CreateParticle(totalTime, Origin + startDelta, particleSpeed, Loc.Zero, LocHeight, 0, 0, Dir));
                }
                
                CurrentShots++;
                if (CurrentShots >= Shots)
                    break;
            }
        }
    }
}