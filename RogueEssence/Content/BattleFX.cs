using System;

namespace RogueEssence.Content
{
    [Serializable]
    public class BattleFX
    {
        /// <summary>
        /// After playing this VFX, will wait this many milliseconds before moving to the next one.
        /// </summary>
        public int Delay;

        /// <summary>
        /// The sound effect of the VFX
        /// </summary>
        [Dev.Sound(0)]
        public string Sound;

        /// <summary>
        /// The Particle FX
        /// </summary>
        [Dev.SubGroup]
        public FiniteEmitter Emitter;

        /// <summary>
        /// Screen shake and other effects.
        /// </summary>
        [Dev.SubGroup]
        public ScreenMover ScreenMovement;

        public BattleFX()
        {
            Emitter = new EmptyFiniteEmitter();
            ScreenMovement = new ScreenMover();
            Sound = "";
        }
        public BattleFX(FiniteEmitter emitter, string sound, int delay)
        {
            Emitter = emitter;
            Sound = sound;
            Delay = delay;
            ScreenMovement = new ScreenMover();
        }

        public BattleFX(BattleFX other)
        {
            Delay = other.Delay;
            Emitter = (FiniteEmitter)other.Emitter.Clone();
            ScreenMovement = new ScreenMover(other.ScreenMovement);
            Sound = other.Sound;
        }


        public override string ToString()
        {
            string result = Emitter.ToString();
            if (Sound != "")
                result += ", SE:" + Sound;
            if (Delay > 0)
                result += " +" + Delay;
            return result;
        }
    }
}
