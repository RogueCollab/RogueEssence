using System;
using RogueElements;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using RogueEssence.Data;

namespace RogueEssence.Content
{
    /// <summary>
    /// Creates an overlay effect using a background texture.
    /// </summary>
    [Serializable]
    public class FiniteOverlayEmitter : FiniteEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public FiniteOverlayEmitter()
        {
            Anim = new BGAnimData();
            Layer = DrawLayer.Normal;
            Color = Color.White;
        }
        public FiniteOverlayEmitter(FiniteOverlayEmitter other)
        {
            Anim = new BGAnimData(other.Anim);
            Movement = other.Movement;
            TotalTime = other.TotalTime;
            FadeIn = other.FadeIn;
            FadeOut = other.FadeOut;
            Offset = other.Offset;
            RepeatX = other.RepeatX;
            RepeatY = other.RepeatY;
            Layer = other.Layer;
            Color = other.Color;
        }

        public override BaseEmitter Clone() { return new FiniteOverlayEmitter(this); }

        public int Offset;

        /// <summary>
        /// The background texture to show.
        /// </summary>
        public BGAnimData Anim;

        /// <summary>
        /// Pixels per second
        /// </summary>
        public Loc Movement;


        public bool RepeatX;

        [Dev.SharedRow]
        public bool RepeatY;

        /// <summary>
        /// Time to fade in, in render frames.  Cuts into total time.
        /// </summary>
        public int FadeIn;

        /// <summary>
        /// Time to fade out, in render frames.  Cuts into total time.
        /// </summary>
        [Dev.SharedRow]
        public int FadeOut;

        /// <summary>
        /// The total time the animation appears, in frames.
        /// </summary>
        public int TotalTime;
        public DrawLayer Layer;

        /// <summary>
        /// The color to shift the background texture.
        /// </summary>
        public Color Color;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            if (Anim.AnimIndex != "")
                scene.Anims[(int)Layer].Add(new OverlayAnim(Origin + Dir.GetLoc() * Offset, Anim, Color, false, Movement, TotalTime, FadeIn, FadeOut, RepeatX, RepeatY));
            finished = true;
        }



        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (Serializer.OldVersion < new Version(0, 6, 1))
            {
                RepeatX = true;
                RepeatY = true;
            }
        }
    }
    [Serializable]
    public class OverlayEmitter : SwitchOffEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public OverlayEmitter()
        {
            Anim = new BGAnimData();
            Layer = DrawLayer.Normal;
            Color = Color.White;
        }
        public OverlayEmitter(OverlayEmitter other)
        {
            Anim = new BGAnimData(other.Anim);
            Movement = other.Movement;
            Offset = other.Offset;
            Layer = other.Layer;
            Color = other.Color;
        }

        public override BaseEmitter Clone() { return new OverlayEmitter(this); }

        public int Offset;
        public BGAnimData Anim;
        /// <summary>
        /// Pixels per second
        /// </summary>
        public Loc Movement;
        public DrawLayer Layer;
        public Color Color;

        [NonSerialized]
        private OverlayAnim runningAnim;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            if (runningAnim == null && Anim.AnimIndex != "")
            {
                runningAnim = new OverlayAnim(Origin + Dir.GetLoc() * Offset, Anim, Color, true, Movement, -1, 0, 0, true, true);
                scene.Anims[(int)Layer].Add(runningAnim);
            }
        }

        public override void SwitchOff()
        {
            finished = true;
            if (runningAnim != null)
                runningAnim.TotalTime = 0;//TODO: the running anim should just be aborted directly.  We need a standard for this...
        }

        public override string ToString()
        {
            return "Overlay[" + Anim.ToString()+"]";
        }
    }
}
