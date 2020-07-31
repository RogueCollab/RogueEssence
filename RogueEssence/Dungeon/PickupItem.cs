using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Dungeon
{

    public class PickupItem : IDrawableSprite
    {
        public string PickupMessage;
        public int SpriteIndex;
        public string Sound;
        public Loc TileLoc;
        public Character WaitingChar;
        public bool LocalMsg;

        public Loc MapLoc { get { return TileLoc * GraphicsManager.TileSize; } }
        public int LocHeight { get { return 0; } }

        public PickupItem(string msg, int sprite, string sound, Loc loc, Character waitChar, bool localMsg)
        {
            PickupMessage = msg;
            SpriteIndex = sprite;
            Sound = sound;
            TileLoc = loc;
            WaitingChar = waitChar;
            LocalMsg = localMsg;
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            Draw(spriteBatch, offset, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset, Color color)
        {
            if (SpriteIndex > -1)
            {
                Loc drawLoc = GetDrawLoc(offset);

                DirSheet sheet = GraphicsManager.GetItem(SpriteIndex);
                sheet.DrawDir(spriteBatch, new Vector2(drawLoc.X, drawLoc.Y), 0, Dir8.Down, color);
            }
        }

        public Loc GetDrawLoc(Loc offset)
        {
            return new Loc(MapLoc.X + GraphicsManager.TileSize / 2 - GraphicsManager.GetItem(SpriteIndex).TileWidth / 2,
                MapLoc.Y + GraphicsManager.TileSize / 2 - GraphicsManager.GetItem(SpriteIndex).TileHeight / 2) - offset;
        }

        public Loc GetDrawSize()
        {
            return new Loc(GraphicsManager.GetItem(SpriteIndex).TileWidth,
                GraphicsManager.GetItem(SpriteIndex).TileHeight);
        }

    }
}
