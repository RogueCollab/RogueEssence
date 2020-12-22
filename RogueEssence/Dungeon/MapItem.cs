using System;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class MapItem : IDrawableSprite, ISpawnable
    {
        public const int MONEY_SPRITE = 10;

        public bool IsMoney;
        public bool Cursed;
        public int Value;
        public int HiddenValue;

        public int SpriteIndex
        {
            get
            {
                if (IsMoney)
                    return MONEY_SPRITE;
                else
                    return Data.DataManager.Instance.GetItem(Value).Sprite;
            }
        }

        public Loc TileLoc;
        public Loc MapLoc { get { return TileLoc * GraphicsManager.TileSize; } }
        public int LocHeight { get { return 0; } }

        public MapItem()
        {
            Value = -1;
            TileLoc = new Loc();
        }

        public MapItem(int value)
        {
            Value = value;
        }

        public MapItem(bool isMoney, int value)
            : this(value)
        {
            IsMoney = isMoney;
        }

        public MapItem(int value, int hiddenValue)
            : this(value)
        {
            HiddenValue = hiddenValue;
        }

        public MapItem(MapItem other)
        {
            IsMoney = other.IsMoney;
            Cursed = other.Cursed;
            Value = other.Value;
            HiddenValue = other.HiddenValue;
        }
        public ISpawnable Copy() { return new MapItem(this); }

        public MapItem(InvItem item) : this(item, new Loc()) { }

        public MapItem(InvItem item, Loc loc)
        {
            Value = item.ID;
            Cursed = item.Cursed;
            HiddenValue = item.HiddenValue;
            TileLoc = loc;
        }

        public InvItem MakeInvItem()
        {
            return new InvItem(Value, Cursed, HiddenValue);
        }


        public string GetDungeonName()
        {
            if (IsMoney)
                return Text.FormatKey("MONEY_AMOUNT", Value);
            else
            {
                Data.ItemData entry = Data.DataManager.Instance.GetItem(Value);
                if (entry.MaxStack > 1)
                    return (entry.Icon > -1 ? ((char)(entry.Icon + 0xE0A0)).ToString() : "") + (Cursed ? "\uE10B" : "") + entry.Name.ToLocal() + " (" + HiddenValue + ")";
                else
                    return (entry.Icon > -1 ? ((char)(entry.Icon + 0xE0A0)).ToString() : "") + (Cursed ? "\uE10B" : "") + entry.Name.ToLocal();
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch, Loc offset) { }
        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            Draw(spriteBatch, offset, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset, Color color)
        {
            Loc drawLoc = GetDrawLoc(offset);

            DirSheet sheet = GraphicsManager.GetItem(SpriteIndex);
            sheet.DrawDir(spriteBatch, new Vector2(drawLoc.X, drawLoc.Y), 0, Dir8.Down, color);
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
