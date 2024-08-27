using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Content
{
    /// <summary>
    /// A simple emitter that releases a single animation, or particle emitter.
    /// </summary>
    [Serializable]
    public class SingleEmitter : FiniteEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public SingleEmitter()
        {
            Anim = new EmptyFiniteEmitter();
            Layer = DrawLayer.Normal;
        }
        public SingleEmitter(AnimData anim)
        {
            Anim = new StaticAnim(anim);
            Layer = DrawLayer.Normal;
        }
        public SingleEmitter(AnimData anim, int cycles)
        {
            Anim = new StaticAnim(anim, cycles, 0);
            Layer = DrawLayer.Normal;
        }
        public SingleEmitter(BeamAnimData anim)
        {
            Anim = new ColumnAnim(anim);
            Layer = DrawLayer.Normal;
        }
        public SingleEmitter(SingleEmitter other)
        {
            Anim = other.Anim.CloneIEmittable();
            Offset = other.Offset;
            LocHeight = other.LocHeight;
            Layer = other.Layer;
            UseDest = other.UseDest;
        }

        public override BaseEmitter Clone() { return new SingleEmitter(this); }

        /// <summary>
        /// Shifts the animation in the given number of pixels, based on the direction of the origin entity.
        /// </summary>
        public int Offset;

        /// <summary>
        /// The animation to play.  Can also be an emitter.
        /// </summary>
        public IEmittable Anim;

        /// <summary>
        /// The layer to put the animation on.
        /// </summary>
        public DrawLayer Layer;

        /// <summary>
        /// Uses the other entity as the origin point of the animation, if there is one.
        /// </summary>
        public bool UseDest;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            scene.Anims[(int)Layer].Add(Anim.CreateStatic(UseDest ? Destination : Origin + Dir.GetLoc() * Offset, LocHeight, Dir));
            finished = true;
        }


        public override string ToString()
        {
            return "Single["+Anim.ToString() + "]";
        }
    }



    [Serializable]
    public class MultiSwitchEmitter : SwitchOffEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public MultiSwitchEmitter()
        {
            Emitters = new List<SwitchOffEmitter>();
        }
        public MultiSwitchEmitter(MultiSwitchEmitter other)
        {
            Emitters = new List<SwitchOffEmitter>();
            foreach (SwitchOffEmitter emittable in other.Emitters)
                Emitters.Add((SwitchOffEmitter)emittable.Clone());
        }

        public List<SwitchOffEmitter> Emitters;

        public override BaseEmitter Clone() { return new MultiSwitchEmitter(this); }

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            foreach (SwitchOffEmitter emitter in Emitters)
            {
                emitter.Update(scene, elapsedTime);
            }
        }


        public override void SwitchOff()
        {
            finished = true;
            foreach (SwitchOffEmitter emitter in Emitters)
                emitter.SwitchOff();
        }

        public override string ToString()
        {
            return "[Multiple]";
        }

    }

    /// <summary>
    /// A simple emitter that releases a single animation, and then continues it until turned off.
    /// </summary>
    [Serializable]
    public class SingleSwitchEmitter : SwitchOffEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public SingleSwitchEmitter()
        {
            Anim = new StaticAnim();
            Layer = DrawLayer.Normal;
        }
        public SingleSwitchEmitter(AnimData anim)
        {
            Anim = new StaticAnim(anim);
            Layer = DrawLayer.Normal;
        }
        public SingleSwitchEmitter(AnimData anim, int cycles)
        {
            Anim = new StaticAnim(anim, cycles, 0);
            Layer = DrawLayer.Normal;
        }
        public SingleSwitchEmitter(SingleSwitchEmitter other)
        {
            Anim = (StaticAnim)other.Anim.CloneIEmittable();
            Offset = other.Offset;
            LocHeight = other.LocHeight;
            Layer = other.Layer;
            UseDest = other.UseDest;
        }

        public override BaseEmitter Clone() { return new SingleSwitchEmitter(this); }

        /// <summary>
        /// Shifts the animation in the given number of pixels, based on the direction of the origin entity.
        /// </summary>
        public int Offset;

        /// <summary>
        /// The animation to play.  Can also be an emitter.
        /// </summary>
        public StaticAnim Anim;

        /// <summary>
        /// The layer to put the animation on.
        /// </summary>
        public DrawLayer Layer;

        /// <summary>
        /// Uses the other entity as the origin point of the animation, if there is one.
        /// </summary>
        public bool UseDest;


        [NonSerialized]
        private StaticAnim runningAnim;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            if (runningAnim == null)
            {
                runningAnim = (StaticAnim)Anim.CreateStatic(UseDest ? Destination : Origin + Dir.GetLoc() * Offset, LocHeight, Dir);
                scene.Anims[(int)Layer].Add(runningAnim);
            }
        }


        public override void SwitchOff()
        {
            finished = true;
            if (runningAnim != null)
                runningAnim.EndAnim();
        }

        public override string ToString()
        {
            return "SingleSwitch[" + Anim.ToString() + "]";
        }
    }
}
