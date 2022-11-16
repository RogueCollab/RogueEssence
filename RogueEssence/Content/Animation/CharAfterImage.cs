using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Content
{
    public class CharAfterImage : BaseAnim
    {
        public CharAfterImage(Loc mapLoc, CharID appearance, int currentAnim, int frame, Dir8 dir, int locHeight, int animTime, byte alpha, byte alphaSpeed)
        {
            this.mapLoc = mapLoc;
            Appearance = appearance;
            CurrentAnim = currentAnim;
            Frame = frame;
            Direction = dir;
            this.locHeight = locHeight;
            AnimTime = animTime;
            Alpha = alpha;
            AlphaSpeed = alphaSpeed;
        }

        private CharID Appearance;
        
        private int CurrentAnim;
        
        private int Frame;

        private byte Alpha;
        private byte AlphaSpeed;

        private int AnimTime;

        public FrameTick Time;

        public Dir8 Direction;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            Time += elapsedTime;

            if (Time >= AnimTime)
                finished = true;
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            if (Finished)
                return;

            Loc drawLoc = GetDrawLoc(offset);
            CharSheet sheet = GraphicsManager.GetChara(Appearance);
            sheet.DrawCharFrame(spriteBatch, CurrentAnim, true, Direction, drawLoc.ToVector2(), Frame, Color.White * ((float)Alpha / 255));
        }

        public override Loc GetDrawLoc(Loc offset)
        {
            return new Loc(MapLoc.X + GraphicsManager.TileSize / 2 - GraphicsManager.GetChara(Appearance).TileWidth / 2,
                MapLoc.Y + GraphicsManager.TileSize / 2 - GraphicsManager.GetChara(Appearance).TileHeight / 2) - offset;
        }

        public override Loc GetDrawSize()
        {
            return new Loc(GraphicsManager.GetChara(Appearance).TileWidth, GraphicsManager.GetChara(Appearance).TileHeight);
        }

    }
}