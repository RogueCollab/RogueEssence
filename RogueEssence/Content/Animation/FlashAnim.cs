using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Content
{

    class FlashAnim : BaseAnim
    {

        public FlashAnim(Loc mapLoc, BGAnimData anim, Color startColor, Color endColor, bool omnipresent, int fadeInTime, int holdTime, int fadeOutTime)
        {
            Anim = anim;
            this.mapLoc = mapLoc;
            StartColor = startColor;
            EndColor = endColor;
            Omnipresent = omnipresent;
            HoldTime = holdTime;
            FadeInTime = fadeInTime;
            FadeOutTime = fadeOutTime;
        }

        private BGAnimData Anim;

        /// <summary>
        /// In frames
        /// </summary>
        public int FadeInTime;
        public int HoldTime;
        public int FadeOutTime;

        public FrameTick Time;

        public Color StartColor;

        public Color EndColor;

        public bool Omnipresent;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            Time += elapsedTime;
            if (FadeInTime + HoldTime + FadeOutTime >= 0)
            {
                if (Time >= FadeInTime + HoldTime + FadeOutTime)
                    finished = true;
            }
        }

        private Color interpolateColor(Color start, Color end, int current, int total)
        {
            return new Color(MathUtils.Interpolate(start.R,end.R,current,total),
                MathUtils.Interpolate(start.G, end.G, current, total),
                MathUtils.Interpolate(start.B, end.B, current, total),
                MathUtils.Interpolate(start.A, end.A, current, total));
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            if (Finished)
                return;

            DirSheet sheet = GraphicsManager.GetBackground(Anim.AnimIndex);

            int frame = Time.ToFrames();
            Color color = StartColor;
            if (frame < FadeInTime)
                color = interpolateColor(StartColor, EndColor, frame, FadeInTime);
            else if (frame < FadeInTime + HoldTime)
                color = EndColor;
            else if (frame < FadeInTime + HoldTime + FadeOutTime)
            {
                int endTime = FadeInTime + HoldTime + FadeOutTime;
                color = interpolateColor(StartColor, EndColor, endTime - frame, FadeOutTime);
            }

            Loc diff = -offset;
            if (sheet.Width == 1 && sheet.Height == 1)
                sheet.DrawTile(spriteBatch, new Rectangle(0, 0, GraphicsManager.ScreenWidth, GraphicsManager.ScreenHeight), 0, 0, color);
            else
            {
                for (int x = diff.X % sheet.TileWidth - sheet.TileWidth; x < GraphicsManager.ScreenWidth; x += sheet.TileWidth)
                {
                    for (int y = diff.Y % sheet.TileHeight - sheet.TileHeight; y < GraphicsManager.ScreenHeight; y += sheet.TileHeight)
                        sheet.DrawDir(spriteBatch, new Vector2(x, y), Anim.GetCurrentFrame(Time, sheet.TotalFrames), Anim.GetDrawDir(Dir8.None), color);
                }
            }
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