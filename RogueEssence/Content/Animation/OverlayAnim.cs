using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RogueEssence.Content
{

    class OverlayAnim : BaseAnim
    {

        public OverlayAnim(Loc mapLoc, BGAnimData anim, Color color, bool omnipresent, Loc movement, int totalTime, int fadeIn, int fadeOut, bool repeatX, bool repeatY)
        {
            Anim = anim;
            this.mapLoc = mapLoc;
            Color = color;
            Omnipresent = omnipresent;
            Movement = movement;
            TotalTime = totalTime;
            FadeIn = fadeIn;
            FadeOut = fadeOut;
            RepeatX = repeatX;
            RepeatY = repeatY;
        }

        private BGAnimData Anim;

        /// <summary>
        /// In frames
        /// </summary>
        public int TotalTime;

        public int FadeIn;//in render frames
        public int FadeOut;//in render frames

        public FrameTick Time;

        /// <summary>
        /// In pixels per second
        /// </summary>
        public Loc Movement;


        public bool RepeatX;
        public bool RepeatY;


        public Color Color;

        public bool Omnipresent;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            Time += elapsedTime;
            if (TotalTime >= 0)
            {
                if (Time >= TotalTime)
                    finished = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            if (Finished)
                return;

            DirSheet sheet = GraphicsManager.GetBackground(Anim.AnimIndex);

            int frame = Time.ToFrames();
            Loc diff = MapLoc + Movement * frame / 60 - offset;

            float alpha = 1f;
            if (frame < FadeIn)
                alpha = Math.Min(alpha, (float)Math.Min(FadeIn, frame) / Math.Max(FadeIn, 1));
            if (TotalTime - frame < FadeOut)
                alpha = Math.Min(alpha, (float)Math.Min(FadeOut, TotalTime - frame) / Math.Max(FadeOut, 1));

            if (sheet.Width == 1 && sheet.Height == 1)
                sheet.DrawTile(spriteBatch, new Rectangle(0, 0, GraphicsManager.ScreenWidth, GraphicsManager.ScreenHeight), 0, 0, Color * ((float)Anim.Alpha * alpha / 255f));
            else
            {
                if (RepeatX)
                {
                    for (int x = diff.X % sheet.TileWidth - sheet.TileWidth; x < GraphicsManager.ScreenWidth; x += sheet.TileWidth)
                        drawTileY(spriteBatch, diff.Y, sheet, alpha, x);
                }
                else
                    drawTileY(spriteBatch, diff.Y, sheet, alpha, diff.X);

            }
        }

        private void drawTileY(SpriteBatch spriteBatch, int diffY, DirSheet sheet, float alpha, int x)
        {
            if (RepeatY)
            {
                for (int y = diffY % sheet.TileHeight - sheet.TileHeight; y < GraphicsManager.ScreenHeight; y += sheet.TileHeight)
                    sheet.DrawDir(spriteBatch, new Vector2(x, y), Anim.GetCurrentFrame(Time, sheet.TotalFrames), Anim.GetDrawDir(Dir8.None), Color * ((float)Anim.Alpha * alpha / 255f));
            }
            else
                sheet.DrawDir(spriteBatch, new Vector2(x, diffY), Anim.GetCurrentFrame(Time, sheet.TotalFrames), Anim.GetDrawDir(Dir8.None), Color * ((float)Anim.Alpha * alpha / 255f));
        }


        public override Loc GetDrawLoc(Loc offset)
        {
            return MapLoc;
        }

        public override Loc GetDrawSize()
        {
            if (Omnipresent)
                return new Loc(-1);
            return new Loc(GraphicsManager.TileSize);
        }

    }
}