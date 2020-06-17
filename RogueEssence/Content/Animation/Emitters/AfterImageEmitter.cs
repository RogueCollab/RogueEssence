using System;
using RogueElements;

namespace RogueEssence.Content
{
    [Serializable]
    public class AfterImageEmitter : AttachPointEmitter
    {
        public AfterImageEmitter()
        {

        }
        public AfterImageEmitter(AfterImageEmitter other)
        {
            AnimTime = other.AnimTime;
            BurstTime = other.BurstTime;
            Alpha = other.Alpha;
            AlphaSpeed = other.AlphaSpeed;
        }

        public override BaseEmitter Clone() { return new AfterImageEmitter(this); }

        public int AnimTime;
        public int BurstTime;
        public byte Alpha;
        public byte AlphaSpeed;

        [NonSerialized]
        private FrameTick CurrentBurstTime;

        [NonSerialized]
        private Dungeon.MonsterID CurrentForm;
        [NonSerialized]
        private int CurrentAnim;
        [NonSerialized]
        private int CurrentFrame;
        [NonSerialized]
        private Loc CurrentOffset;
        [NonSerialized]
        private int CurrentHeight;

        public override void SetupEmit(ICharSprite user, Loc origin, Loc dest, Dir8 dir, int locHeight)
        {
            int currentTime;
            user.GetCurrentSprite(out CurrentForm, out CurrentOffset, out CurrentHeight, out CurrentAnim, out currentTime, out CurrentFrame);

            base.SetupEmit(user, origin, dest, dir, locHeight);
        }

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            CurrentBurstTime += elapsedTime;
            while (CurrentBurstTime >= BurstTime)
            {
                CurrentBurstTime -= BurstTime;
                scene.Anims[(int)DrawLayer.Normal].Add(new CharAfterImage(Origin + CurrentOffset, CurrentForm, CurrentAnim, CurrentFrame, Dir, CurrentHeight, AnimTime, Alpha, AlphaSpeed));
            }
        }
    }
}
