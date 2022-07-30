using System;
using RogueElements;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Content
{
    //TODO: move this variable around so that NoDraw is -2?
    public enum DrawLayer
    {
        /// <summary>
        /// Draws on the floor, behind all entities and terrain.
        /// </summary>
        Under = -1,
        /// <summary>
        /// Draws on the floor, behind all entities but not terrain.
        /// </summary>
        Bottom = 0,
        /// <summary>
        /// Draws in front of entities if placed at a higher Y coordinate, but draws behind entities in a tie.
        /// </summary>
        Back = 1,
        /// <summary>
        /// Draws in behind of entities if placed at a lower Y coordinate, but draws in front of entities in a tie.
        /// </summary>
        Normal = 2,
        /// <summary>
        /// Draws in front of entities.
        /// </summary>
        Front = 3,
        /// <summary>
        /// Draws on top of everything else.  Often used for overlay.
        /// </summary>
        Top = 4,
        /// <summary>
        /// Does not draw.
        /// </summary>
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
        void GetCurrentSprite(out CharID currentForm, out Loc currentOffset, out int currentHeight, out int currentAnim, out int currentTime, out int currentFrame);
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
