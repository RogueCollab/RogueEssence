using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Content
{
    public class WrappedRainAnim : ParticleAnim
    {
        public WrappedRainAnim() { }
        public WrappedRainAnim(AnimData anim, int totalTime) : base(anim, 0, totalTime) { ResultAnim = new AnimData(); }
        public WrappedRainAnim(AnimData anim, AnimData resultAnim, DrawLayer layer, int totalTime)
            : base(anim, 0, totalTime)
        {
            Layer = layer;
            ResultAnim = resultAnim;
        }

        public AnimData ResultAnim;
        public DrawLayer Layer;
        
        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            base.Update(scene, elapsedTime);

            if (Finished && ResultAnim.AnimIndex != "")
            {
                int totalTime = ResultAnim.FrameTime * ResultAnim.GetTotalFrames(GraphicsManager.GetAttackSheet(ResultAnim.AnimIndex).TotalFrames);
                WrappedRainAnim anim = new WrappedRainAnim(ResultAnim, totalTime);
                anim.SetupEmitted(MapLoc, 0, Dir8.None);
                scene.Anims[(int)Layer].Add(anim);
            }
        }


        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            if (Finished)
                return;

            Loc drawLoc = GetDrawLoc(offset);
            Loc modOffset = new Loc(GraphicsManager.ScreenWidth - offset.X % GraphicsManager.ScreenWidth, GraphicsManager.ScreenHeight - offset.Y % GraphicsManager.ScreenHeight);
            drawLoc = drawLoc + modOffset;
            Vector2 drawDest = new Vector2(drawLoc.X % GraphicsManager.ScreenWidth, (drawLoc.Y - LocHeight) % GraphicsManager.ScreenHeight);

            DirSheet sheet = GraphicsManager.GetAttackSheet(Anim.AnimIndex);
            sheet.DrawDir(spriteBatch, drawDest, Anim.GetCurrentFrame(ActionTime, GraphicsManager.GetAttackSheet(Anim.AnimIndex).TotalFrames), Anim.GetDrawDir(Direction), Color.White * ((float)Anim.Alpha / 255));

        }

        public override Loc GetDrawLoc(Loc offset)
        {
            return new Loc(MapLoc.X - GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileWidth / 2, MapLoc.Y);
        }

        public override Loc GetDrawSize()
        {
            return new Loc(-1);
        }
    }
}