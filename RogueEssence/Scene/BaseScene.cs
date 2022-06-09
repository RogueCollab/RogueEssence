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

        public void AddToDraw(List<(IDrawableSprite, Loc)> sprites, IDrawableSprite sprite)
        {
            AddToDraw(sprites, sprite, ViewRect.Start);
        }

        public void AddToDraw(List<(IDrawableSprite, Loc)> sprites, IDrawableSprite sprite, Loc viewOffset)
        {
            CollectionExt.AddToSortedList(sprites, (sprite, viewOffset), CompareSpriteCoords);
        }

        /// <summary>
        /// Add to draw the sprites in the view rect; one for each divided part of it.
        /// Divisions are due to map wrapping.
        /// </summary>
        public void AddDivRectDraw(List<(IDrawableSprite, Loc)> sprites, Rect[][] divRects, IDrawableSprite sprite)
        {
            foreach (Loc viewOffset in IterateDivRectDraw(divRects, sprite))
                AddToDraw(sprites, sprite, viewOffset);
        }

        public IEnumerable<Loc> IterateDivRectDraw(Rect[][] divRects, IDrawableSprite sprite)
        {
            int xOffset = 0;
            for (int xx = 0; xx < divRects.Length; xx++)
            {
                int yOffset = 0;
                for (int yy = 0; yy < divRects[xx].Length; yy++)
                {
                    if (CanSeeSprite(divRects[xx][yy], sprite))
                        yield return divRects[xx][yy].Start - new Loc(xOffset, yOffset);
                    yOffset += divRects[xx][yy].Height;
                }
                xOffset += divRects[xx][0].Width;
            }
        }


        /// <summary>
        /// Converts out of bounds coords to wrapped-around coords.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Loc WrapLoc(Loc loc, Loc size)
        {
            return (loc % size + size) % size;
        }

        /// <summary>
        /// Slices a rectangle at the wrapped map boundaries.
        /// </summary>
        /// <returns></returns>
        public static Rect[][] WrapSplitRect(Rect rect, Loc size)
        {
            Loc topLeftBounds = new Loc(MathUtils.DivDown(rect.Start.X, size.X), MathUtils.DivDown(rect.Start.Y, size.Y));
            Loc bottomRightBounds = new Loc(MathUtils.DivUp(rect.End.X, size.X), MathUtils.DivUp(rect.End.Y, size.Y));

            Rect[][] choppedGrid = new Rect[bottomRightBounds.X - topLeftBounds.X][];
            for (int xx = 0; xx < bottomRightBounds.X - topLeftBounds.X; xx++)
            {
                choppedGrid[xx] = new Rect[bottomRightBounds.Y - topLeftBounds.Y];
                for (int yy = 0; yy < bottomRightBounds.Y - topLeftBounds.Y; yy++)
                {
                    Rect subRect = new Rect((topLeftBounds + new Loc(xx, yy)) * size, size);
                    Rect interRect = Rect.Intersect(rect, subRect);
                    Rect wrappedRect = new Rect(WrapLoc(interRect.Start, size), interRect.Size);
                    choppedGrid[xx][yy] = wrappedRect;
                }
            }
            return choppedGrid;
        }

        public int CompareSpriteCoords((IDrawableSprite sprite, Loc viewOffset) sprite1, (IDrawableSprite sprite, Loc viewOffset) sprite2)
        {
            return Math.Sign((sprite1.sprite.MapLoc.Y - sprite1.viewOffset.Y) - (sprite2.sprite.MapLoc.Y - sprite2.viewOffset.Y));
        }
    }
}
