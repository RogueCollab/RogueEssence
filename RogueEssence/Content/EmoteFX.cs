using System;

namespace RogueEssence.Content
{
    [Serializable]
    public class EmoteFX
    {
        public int Delay;

        [Dev.Sound(0)]
        public string Sound;

        public int LocHeight;
        public AnimData Anim;

        public EmoteFX()
        {
            Sound = "";
        }
        public EmoteFX(AnimData anim, int locHeight, string sound, int delay)
        {
            Anim = anim;
            LocHeight = locHeight;
            Sound = sound;
            Delay = delay;
        }

        public EmoteFX(EmoteFX other)
        {
            Anim = other.Anim;
            LocHeight = other.LocHeight;
            Delay = other.Delay;
            Sound = other.Sound;
        }


        public override string ToString()
        {
            string result = Anim.ToString();
            if (Sound != "")
                result += ", SE:" + Sound;
            if (Delay > 0)
                result += " +" + Delay;
            return result;
        }
    }
}
