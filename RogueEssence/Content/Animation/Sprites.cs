using System;
using RogueElements;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Content
{

    public enum DrawLayer
    {
        Bottom = 0,
        Back = 1,
        Normal = 2,
        Front = 3,
        Top = 4,
        NoDraw = 5
    }

    public interface IDrawableSprite
    {
        Loc MapLoc { get; }
        int LocHeight { get; }

        void DrawDebug(SpriteBatch spriteBatch, Loc offset);

        void Draw(SpriteBatch spriteBatch, Loc offset);

        Loc GetDrawLoc(Loc offset);
        Loc GetDrawSize();
    }

    public interface IBackgroundSprite : IDrawableSprite
    {

    }

    public interface ICharSprite : IDrawableSprite
    {
        void GetCurrentSprite(out Dungeon.MonsterID currentForm, out Loc currentOffset, out int currentHeight, out int currentAnim, out int currentTime, out int currentFrame);
    }

    public interface IParticleEmittable : IEmittable
    {
        IParticleEmittable CreateParticle(Loc startLoc, Loc speed, Loc acceleration, int startHeight, int heightSpeed, int heightAcceleration, Dir8 dir);
        IParticleEmittable CreateParticle(int totalTime, Loc startLoc, Loc speed, Loc acceleration, int startHeight, int heightSpeed, int heightAcceleration, Dir8 dir);
    }

    public interface IEmittable : IFinishableSprite
    {
        IEmittable CloneIEmittable();
        IEmittable CreateStatic(Loc mapLoc, int locHeight, Dir8 dir);
    }

    public interface IFinishableSprite : IDrawableSprite
    {
        bool Finished { get; }
        void Update(BaseScene scene, FrameTick elapsedTime);
    }

    [Serializable]
    public abstract class BaseAnim : IFinishableSprite
    {
        [NonSerialized]
        protected Loc mapLoc;
        [NonSerialized]
        protected int locHeight;
        [NonSerialized]
        protected bool finished;

        public Loc MapLoc { get { return mapLoc; } }
        public int LocHeight { get { return locHeight; } }
        public bool Finished { get { return finished; } }

        public abstract void Update(BaseScene scene, FrameTick elapsedTime);

        public void DrawDebug(SpriteBatch spriteBatch, Loc offset) { }
        public abstract void Draw(SpriteBatch spriteBatch, Loc offset);

        public abstract Loc GetDrawLoc(Loc offset);
        public abstract Loc GetDrawSize();
    }
}
