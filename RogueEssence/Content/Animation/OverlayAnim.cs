using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Content
{

    class OverlayAnim : BaseAnim
    {

        public OverlayAnim(Loc mapLoc, BGAnimData anim, Color color, bool omnipresent, Loc movement, int totalTime/*, int fadeTime*/)
        {
            Anim = anim;
            this.mapLoc = mapLoc;
            Color = color;
            Omnipresent = omnipresent;
            Movement = movement;
            TotalTime = totalTime;
            //FadeTime = fadeTime;
        }

        private BGAnimData Anim;

        /// <summary>
        /// In frames
        /// </summary>
        public int TotalTime;
        //private int FadeTime;//in render frames

        public FrameTick Time;

        /// <summary>
        /// In pixels per frame
        /// </summary>
        public Loc Movement;

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
            Loc diff = Movement * frame - offset;
            float fade = 1f;// (float)Math.Min(Math.Min(FadeTime, frame), (TotalTime > 0) ? Math.Min(FadeTime, TotalTime - frame) : FadeTime) / FadeTime;
            if (sheet.Width == 1 && sheet.Height == 1)
                sheet.DrawTile(spriteBatch, new Rectangle(0, 0, GraphicsManager.ScreenWidth, GraphicsManager.ScreenHeight), 0, 0, Color * ((float)Anim.Alpha * fade / 255f));
            else
            {
                for (int x = diff.X % sheet.TileWidth - sheet.TileWidth; x < GraphicsManager.ScreenWidth; x += sheet.TileWidth)
                {
                    for (int y = diff.Y % sheet.TileHeight - sheet.TileHeight; y < GraphicsManager.ScreenHeight; y += sheet.TileHeight)
                        sheet.DrawDir(spriteBatch, new Vector2(x, y), Anim.GetCurrentFrame(Time, sheet.TotalFrames), Anim.AnimDir, Color * ((float)Anim.Alpha * fade / 255f));
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