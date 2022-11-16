using System;
using RogueElements;
using Microsoft.Xna.Framework;

namespace RogueEssence.Content
{
    /// <summary>
    /// Flashes a background image once.
    /// </summary>
    [Serializable]
    public class FlashEmitter : FiniteEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public FlashEmitter()
        {
            Anim = new BGAnimData();
            Layer = DrawLayer.Normal;
            StartColor = Color.White;
            EndColor = Color.White;
        }
        public FlashEmitter(FlashEmitter other)
        {
            Anim = new BGAnimData(other.Anim);
            FadeInTime = other.FadeInTime;
            HoldTime = other.HoldTime;
            FadeOutTime = other.FadeOutTime;
            Offset = other.Offset;
            Layer = other.Layer;
            StartColor = other.StartColor;
            EndColor = other.EndColor;
        }

        public override BaseEmitter Clone() { return new FlashEmitter(this); }

        public int Offset;
        public BGAnimData Anim;
        public int FadeInTime;
        public int HoldTime;
        public int FadeOutTime;
        public DrawLayer Layer;
        public Color StartColor;
        public Color EndColor;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            if (Anim.AnimIndex != "")
                scene.Anims[(int)Layer].Add(new FlashAnim(Origin + Dir.GetLoc() * Offset, Anim, StartColor, EndColor, false, FadeInTime, HoldTime, FadeOutTime));
            finished = true;
        }
    }
}
