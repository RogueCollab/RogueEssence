using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence
{
    
    public abstract class BaseScene
    {
        
        public List<IFinishableSprite>[] Anims;
        
        public ScreenMover ScreenShake;

        protected float windowScale;
        protected float matrixScale;
        protected float scale;
        protected float drawScale;

        public Rect ViewRect { get; protected set; }


        public BaseScene()
        {
            Anims = new List<IFinishableSprite>[6];
            for (int n = (int)DrawLayer.Bottom; n <= (int)DrawLayer.NoDraw; n++)
                Anims[n] = new List<IFinishableSprite>();
        }

        public abstract void Exit();
        public abstract void Begin();

        public virtual void UpdateMeta() { }

        public abstract IEnumerator<YieldInstruction> ProcessInput();
        

        public void CreateAnim(IFinishableSprite anim, DrawLayer priority)
        {
            Anims[(int)priority].Add(anim);
        }

        public void ResetAnims()
        {
            for (int nn = (int)DrawLayer.Bottom; nn <= (int)DrawLayer.NoDraw; nn++)
                Anims[nn].Clear();
        }

        public void SetScreenShake(ScreenMover shake)
        {
            if (shake.MaxShake == 0)
                return;
            if (ScreenShake != null && shake.MaxShake < ScreenShake.MaxShake)
                return;

            ScreenShake = shake;
        }

        public virtual void UpdateCamMod(FrameTick elapsedTime, ref Loc focusedLoc)
        {
            if (ScreenShake != null)
            {
                ScreenShake.Update(elapsedTime, ref focusedLoc);
                if (ScreenShake.Finished)
                    ScreenShake = null;
            }
        }

        public virtual void Update(FrameTick elapsedTime)
        {
            for (int nn = (int)DrawLayer.Bottom; nn <= (int)DrawLayer.NoDraw; nn++)
            {
                for (int ii = Anims[nn].Count - 1; ii >= 0; ii--)
                {
                    Anims[nn][ii].Update(this, elapsedTime);
                    if (Anims[nn][ii].Finished)
                        Anims[nn].RemoveAt(ii);
                }
            }

        }

        public abstract void Draw(SpriteBatch spriteBatch);

        public virtual void DrawDebug(SpriteBatch spriteBatch) { }




        public bool CanSeeSprite(Rect viewBounds, IDrawableSprite sprite)
        {
            if (sprite == null)
                return false;

            Loc drawSize = sprite.GetDrawSize();
            if (drawSize == new Loc(-1))
                return true;
            Rect spriteRect = new Rect(sprite.GetDrawLoc(Loc.Zero), drawSize);

            if (spriteRect.Size == Loc.Zero)
                return false;

            return Collision.Collides(spriteRect, viewBounds);
        }

        public void AddToDraw(List<IDrawableSprite> sprites, IDrawableSprite sprite)
        {
            CollectionExt.AddToSortedList(sprites, sprite, CompareSpriteCoords);
        }



        public int CompareSpriteCoords(IDrawableSprite sprite1, IDrawableSprite sprite2)
        {
            return Math.Sign(sprite1.MapLoc.Y - sprite2.MapLoc.Y);
        }
    }
}
