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
            if (drawSize == Loc.Zero)
                return false;

            Rect spriteRect = new Rect(sprite.GetDrawLoc(Loc.Zero), drawSize);


            return Collision.Collides(spriteRect, viewBounds);
        }

        public void AddToDraw(List<(IDrawableSprite, Loc)> sprites, IDrawableSprite sprite)
        {
            AddToDraw(sprites, sprite, ViewRect.Start);
        }

        public void AddToDraw(List<(IDrawableSprite, Loc)> sprites, IDrawableSprite sprite, Loc viewOffset)
        {
            CollectionExt.AddToSortedList(sprites, (sprite, viewOffset), CompareSpriteCoords);
        }

        public void AddRelevantDraw(List<(IDrawableSprite, Loc)> sprites, bool wrapped, Loc wrapSize, IDrawableSprite sprite)
        {
            foreach (Loc viewOffset in IterateRelevantDraw(wrapped, wrapSize, sprite))
                AddToDraw(sprites, sprite, viewOffset);
        }

        public IEnumerable<Loc> IterateRelevantDraw(bool wrapped, Loc wrapSize, IDrawableSprite sprite)
        {
            if (sprite == null)
                yield break;

            Loc drawSize = sprite.GetDrawSize();
            if (drawSize == new Loc(-1))
                yield return ViewRect.Start;
            if (drawSize == Loc.Zero)
                yield break;

            Loc baseDrawLoc = sprite.GetDrawLoc(Loc.Zero);
            if (!wrapped)
            {
                Rect spriteRect = new Rect(baseDrawLoc, drawSize);
                if (Collision.Collides(spriteRect, ViewRect))
                    yield return ViewRect.Start;
                yield break;
            }

            //take the topmost Y of the map, subtract the height of the sprite, round down to the lowest whole map.  this is the topmost map to check
            //take the bottom-most Y of the map, round up to the highest whole map.  this is the bottom-most (exclusive) map to check
            //do the same for X
            Loc topLeftBounds = new Loc(MathUtils.DivDown(ViewRect.X - drawSize.X, wrapSize.X), MathUtils.DivDown(ViewRect.Y - drawSize.Y, wrapSize.Y));
            Loc bottomRightBounds = new Loc(MathUtils.DivUp(ViewRect.End.X, wrapSize.X), MathUtils.DivUp(ViewRect.End.Y, wrapSize.Y));
            Loc wrapLoc = Loc.Wrap(baseDrawLoc, wrapSize);

            for (int xx = topLeftBounds.X; xx < bottomRightBounds.X; xx++)
            {
                for (int yy = topLeftBounds.Y; yy < bottomRightBounds.Y; yy++)
                {
                    Loc mapStart = new Loc(xx, yy) * wrapSize;
                    Rect spriteRect = new Rect(mapStart + wrapLoc, drawSize);
                    if (Collision.Collides(spriteRect, ViewRect))
                    {
                        //first compute a loc for which the addition to the original loc would result in this checked loc
                        Loc diffLoc = spriteRect.Start - baseDrawLoc;
                        //that difference is how much the viewRect needs to be shifted by
                        yield return ViewRect.Start - diffLoc;
                    }
                }
            }
        }

        public int CompareSpriteCoords((IDrawableSprite sprite, Loc viewOffset) sprite1, (IDrawableSprite sprite, Loc viewOffset) sprite2)
        {
            return Math.Sign((sprite1.sprite.MapLoc.Y - sprite1.viewOffset.Y) - (sprite2.sprite.MapLoc.Y - sprite2.viewOffset.Y));
        }
    }
}
