using System;
using RogueElements;
using Microsoft.Xna.Framework;

namespace RogueEssence.Content
{
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
            Offset = other.Offset;
            Layer = other.Layer;
            Color = other.Color;
        }

        public override BaseEmitter Clone() { return new FiniteOverlayEmitter(this); }

        public int Offset;
        public BGAnimData Anim;
        public Loc Movement;
        public int TotalTime;
        public DrawLayer Layer;
        public Color Color;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            if (Anim.AnimIndex != "")
                scene.Anims[(int)Layer].Add(new OverlayAnim(Origin + Dir.GetLoc() * Offset, Anim, Color, false, Movement, TotalTime));
            finished = true;
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
        public Loc Movement;
        public DrawLayer Layer;
        public Color Color;

        [NonSerialized]
        private OverlayAnim runningAnim;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            if (runningAnim == null && Anim.AnimIndex != "")
            {
                runningAnim = new OverlayAnim(Origin + Dir.GetLoc() * Offset, Anim, Color, true, Movement, -1);
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
