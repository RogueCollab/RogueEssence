using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Content
{
    public class TextAnim : BaseAnim
    {

        public int TOTAL_SOLID_TIME = 60;
        public int TOTAL_ANIM_TIME = 80;

        public Loc EffectLoc;
        public float Opacity;
        public FrameTick ActionTime;
        public string Text;
        public FontSheet UsedFont;

        public TextAnim(Loc loc, string text, FontSheet font)
        {
            EffectLoc = loc;
            Text = text;
            finished = false;
            UsedFont = font;
            mapLoc = new Loc(EffectLoc.X * GraphicsManager.TileSize, EffectLoc.Y * GraphicsManager.TileSize);
        }
        

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            ActionTime += elapsedTime;
            if (ActionTime >= TOTAL_ANIM_TIME)
                finished = true;
            else
            {
                locHeight = (int)ActionTime.FractionOf(GraphicsManager.TileSize / 3, TOTAL_ANIM_TIME) + GraphicsManager.TileSize / 2;
                if (ActionTime >= TOTAL_SOLID_TIME)
                    Opacity = (float)(255 - (int)(ActionTime - TOTAL_SOLID_TIME).FractionOf(255, TOTAL_ANIM_TIME - TOTAL_SOLID_TIME)) / 255;
                else
                    Opacity = 1f;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            Loc drawLoc = GetDrawLoc(offset);
            UsedFont.DrawText(spriteBatch, drawLoc.X, drawLoc.Y, Text, null, DirV.Up, DirH.Left, Color.White * Opacity);
        }

        public override Loc GetDrawLoc(Loc offset)
        {
            return new Loc(EffectLoc.X * GraphicsManager.TileSize + GraphicsManager.TileSize / 2 - (int)GraphicsManager.TextFont.SubstringWidth(Text) / 2,
                EffectLoc.Y * GraphicsManager.TileSize - LocHeight + GraphicsManager.TileSize / 2 - (int)GraphicsManager.TextFont.StringHeight(Text, 0) / 2) - offset;
        }

        public override Loc GetDrawSize()
        {
            return new Loc((int)GraphicsManager.TextFont.SubstringWidth(Text), (int)GraphicsManager.TextFont.StringHeight(Text, 0));
        }

    }
}
