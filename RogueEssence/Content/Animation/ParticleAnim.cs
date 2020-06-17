using System;
using RogueElements;

namespace RogueEssence.Content
{
    [Serializable]
    public class ParticleAnim : StaticAnim, IParticleEmittable
    {
        public ParticleAnim() { }
        public ParticleAnim(AnimData anim) : base(anim) { }
        public ParticleAnim(AnimData anim, int cycles) : base(anim, cycles) { }
        public ParticleAnim(AnimData anim, int cycles, int totalTime) : base(anim, cycles, totalTime) { }

        protected ParticleAnim(ParticleAnim other)
            : base(other)
        { }
        public override IEmittable CloneIEmittable() { return new ParticleAnim(this); }

        //acceleration

        [NonSerialized]
        public Loc StartLoc;

        /// <summary>
        /// Pixels Per Second
        /// </summary>
        [NonSerialized]
        public Loc Speed;

        /// <summary>
        /// Pixels Per Second Per Second 
        /// </summary>
        [NonSerialized]
        public Loc Acceleration;

        [NonSerialized]
        public int StartHeight;

        /// <summary>
        /// Pixels Per Second
        /// </summary>
        [NonSerialized]
        public int HeightSpeed;

        /// <summary>
        /// Pixels Per Second Per Second 
        /// </summary>
        [NonSerialized]
        public int HeightAcceleration;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            base.Update(scene, elapsedTime);

            //x = c + b*t + a*(t^2)/2
            locHeight = StartHeight + HeightSpeed * ActionTime.ToFrames() / GraphicsManager.MAX_FPS + HeightAcceleration * ActionTime.ToFrames() * ActionTime.ToFrames() / GraphicsManager.MAX_FPS / GraphicsManager.MAX_FPS / 2;
            mapLoc = StartLoc + Speed * ActionTime.ToFrames() / GraphicsManager.MAX_FPS + Acceleration * ActionTime.ToFrames() * ActionTime.ToFrames() / GraphicsManager.MAX_FPS / GraphicsManager.MAX_FPS / 2;
        }


        public IParticleEmittable CreateParticle(Loc startLoc, Loc speed, Loc acceleration, int startHeight, int heightSpeed, int heightAcceleration, Dir8 dir)
        {
            ParticleAnim anim = (ParticleAnim)CloneIEmittable();
            anim.SetupEmitted(startLoc, speed, acceleration, startHeight, heightSpeed, heightAcceleration, dir);
            return anim;
        }

        public IParticleEmittable CreateParticle(int totalTime, Loc startLoc, Loc speed, Loc acceleration, int startHeight, int heightSpeed, int heightAcceleration, Dir8 dir)
        {
            ParticleAnim anim = (ParticleAnim)CloneIEmittable();
            anim.TotalTime = totalTime;
            anim.Cycles = 0;
            anim.SetupEmitted(startLoc, speed, acceleration, startHeight, heightSpeed, heightAcceleration, dir);
            return anim;
        }

        public override void SetupEmitted(Loc startLoc, int startHeight, Dir8 dir)
        {
            SetupEmitted(startLoc, Loc.Zero, Loc.Zero, startHeight, 0, 0, dir);
        }

        public virtual void SetupEmitted(Loc startLoc, Loc speed, Loc acceleration, int startHeight, int heightSpeed, int heightAcceleration, Dir8 dir)
        {
            StartLoc = startLoc;
            Speed = speed;
            Acceleration = acceleration;
            StartHeight = startHeight;
            HeightSpeed = heightSpeed;
            HeightAcceleration = heightAcceleration;
            Direction = dir;

            locHeight = StartHeight;
            mapLoc = StartLoc;
            SetUp();
        }
    }
}