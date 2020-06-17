using RogueElements;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Content
{
    public class ItemAnim : BaseAnim
    {

        public const int ITEM_ACTION_TIME = 20;
        
        public ItemAnim(Loc startLoc, Loc endLoc, int sprite) : this(startLoc, endLoc, sprite, GraphicsManager.TileSize / 2, 0) { }
        public ItemAnim(Loc startLoc, Loc endLoc, int sprite, int maxHeight, int waitTime)
        {
            StartLoc = startLoc;
            EndLoc = endLoc;
            Sprite = sprite;
            MaxHeight = maxHeight;
            WaitTime = waitTime;
        }

        public int Sprite;

        public Loc StartLoc;
        public Loc EndLoc;

        public int MaxHeight;
        public int WaitTime;

        public FrameTick Time;

        public virtual void Begin()
        {

        }
        
        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            Time += elapsedTime;

            if (Time >= ITEM_ACTION_TIME)
            {
                if (Time >= ITEM_ACTION_TIME + WaitTime)
                    finished = true;
                locHeight = 0;
                mapLoc = EndLoc * GraphicsManager.TileSize;
            }
            else
            {
                locHeight = AnimMath.GetArc(MaxHeight, FrameTick.FrameToTick(ITEM_ACTION_TIME), Time.Ticks);
                Loc mapDiff = (EndLoc - StartLoc) * GraphicsManager.TileSize;
                mapDiff = new Loc((int)(mapDiff.X * Time.FractionOf(ITEM_ACTION_TIME)), (int)(mapDiff.Y * Time.FractionOf(ITEM_ACTION_TIME)));
                mapLoc = mapDiff + StartLoc * GraphicsManager.TileSize;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            Loc drawLoc = GetDrawLoc(offset);

            DirSheet sheet = GraphicsManager.GetItem(Sprite);
            sheet.DrawDir(spriteBatch, drawLoc.ToVector2(), 0);
        }

        public override Loc GetDrawLoc(Loc offset)
        {
            return new Loc(MapLoc.X + GraphicsManager.TileSize / 2 - GraphicsManager.GetItem(Sprite).TileWidth / 2,
                MapLoc.Y + GraphicsManager.TileSize / 2 - GraphicsManager.GetItem(Sprite).TileHeight / 2 - LocHeight) - offset;
        }

        public override Loc GetDrawSize()
        {
            return new Loc(GraphicsManager.GetItem(Sprite).TileWidth, GraphicsManager.GetItem(Sprite).TileHeight);
        }
    }


    public class ItemDropAnim : BaseAnim
    {
        const int MAX_TILE_HEIGHT = 8;
        public const int ITEM_ACTION_TIME = 20;
        
        public ItemDropAnim(Loc loc, int sprite)
        {
            EndLoc = loc;
            Sprite = sprite;
        }

        public int Sprite;

        public Loc EndLoc;


        public FrameTick Time;

        public virtual void Begin()
        {

        }
        
        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            Time += elapsedTime;

            if (Time >= ITEM_ACTION_TIME)
            {
                finished = true;
                locHeight = 0;
                mapLoc = EndLoc * GraphicsManager.TileSize;
            }
            else
            {
                locHeight = (int)Time.FractionOf(MAX_TILE_HEIGHT * GraphicsManager.TileSize, ITEM_ACTION_TIME);
                mapLoc = EndLoc * GraphicsManager.TileSize;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            Loc drawLoc = GetDrawLoc(offset);

            DirSheet sheet = GraphicsManager.GetItem(Sprite);
            sheet.DrawDir(spriteBatch, drawLoc.ToVector2(), 0);
        }

        public override Loc GetDrawLoc(Loc offset)
        {
            return new Loc(MapLoc.X + GraphicsManager.TileSize / 2 - GraphicsManager.GetItem(Sprite).TileWidth / 2,
                MapLoc.Y + GraphicsManager.TileSize / 2 - GraphicsManager.GetItem(Sprite).TileHeight / 2 - LocHeight) - offset;
        }

        public override Loc GetDrawSize()
        {
            return new Loc(GraphicsManager.GetItem(Sprite).TileWidth, GraphicsManager.GetItem(Sprite).TileHeight);
        }

    }
}
