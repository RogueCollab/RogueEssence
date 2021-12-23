using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace RogueEssence.Content
{
    public class TileSheet : BaseSheet
    {

        public int TotalTiles { get { return TotalX * TotalY; } }
        public int TotalX { get; protected set; }
        public int TotalY { get; protected set; }
        public int TileWidth { get; protected set; }
        public int TileHeight { get; protected set; }

        //public TileSheet(int width, int height, int tileWidth, int tileHeight)
        //    : base(width, height)
        //{
        //    if (ImageWidth % tileWidth != 0 || ImageHeight % tileHeight != 0)
        //        throw new Exception("Invalid tile div!");

        //    MaxX = ImageWidth / tileWidth;
        //    MaxY = ImageHeight / tileHeight;
        //    TileWidth = tileWidth;
        //    TileHeight = tileHeight;

        //}

        protected TileSheet(Texture2D tex, int tileWidth, int tileHeight)
            : base(tex)
        {
            if (Width % tileWidth != 0 || Height % tileHeight != 0)
                throw new ArgumentException(String.Format("Texture dimensions ({0},{1}) cannot be divided by ({2},{3})", Width, Height, TileWidth, TileHeight));

            TotalX = Width / tileWidth;
            TotalY = Height / tileHeight;
            TileWidth = tileWidth;
            TileHeight = tileHeight;

        }

        //frompath (import) will take a raw png with two dimensions as suffix
        //fromstream (load) will take the png and the two dimension arguments from the stream
        //save will save as .tile

        public static new TileSheet Import(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string[] components = fileName.Split('-');
            int tileWidth = Convert.ToInt32(components[components.Length-2]);
            int tileHeight = Convert.ToInt32(components[components.Length-1]);
            return TileSheet.Import(path, tileWidth, tileHeight);
        }

        public static TileSheet Import(string path, int tileWidth, int tileHeight)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Texture2D tex = ImportTex(fileStream);
                return new TileSheet(tex, tileWidth, tileHeight);
            }
        }

        public static void Export(TileSheet sheet, string filepath)
        {
            using (Stream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
                ExportTex(stream, sheet.baseTexture);
        }

        public static new TileSheet Load(BinaryReader reader)
        {
            long length = reader.ReadInt64();
            Texture2D tex = null;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(reader.ReadBytes((int)length), 0, (int)length);
                ms.Position = 0;
                tex = Texture2D.FromStream(device, ms);
            }

            int tileWidth = reader.ReadInt32();
            int tileHeight = reader.ReadInt32();
            return new TileSheet(tex, tileWidth, tileHeight);
        }

        public static new TileSheet LoadError()
        {
            return new TileSheet(defaultTex, defaultTex.Width, defaultTex.Height);
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(TileWidth);
            writer.Write(TileHeight);
        }


        public void DrawTile(SpriteBatch spriteBatch, Vector2 pos, int x, int y)
        {
            DrawTile(spriteBatch, pos, x, y, Color.White);
        }

        public void DrawTile(SpriteBatch spriteBatch, Vector2 pos, int x, int y, Color color)
        {
            DrawTile(spriteBatch, pos, x, y, color, SpriteEffects.None);
        }

        public void DrawTile(SpriteBatch spriteBatch, Vector2 pos, int x, int y, Color color, float rotation)
        {
            DrawTile(spriteBatch, pos, x, y, color, 1f, rotation);
        }

        public void DrawTile(SpriteBatch spriteBatch, Vector2 pos, int x, int y, Color color, float rotation, SpriteEffects spriteEffects)
        {
            DrawTile(spriteBatch, pos, x, y, color, Vector2.One, rotation, spriteEffects);
        }

        public void DrawTile(SpriteBatch spriteBatch, Vector2 pos, int x, int y, Color color, float scale, float rotation)
        {
            DrawTile(spriteBatch, pos, x, y, color, new Vector2(scale), rotation);
        }

        public void DrawTile(SpriteBatch spriteBatch, Vector2 pos, int x, int y, Color color, Vector2 scale, float rotation)
        {
            DrawTile(spriteBatch, pos, x, y, color, scale, rotation, SpriteEffects.None);
        }

        public void DrawTile(SpriteBatch spriteBatch, Vector2 pos, int x, int y, Color color, Vector2 scale, float rotation, SpriteEffects spriteEffects)
        {
            if (x < TotalX && y < TotalY)
                Draw(spriteBatch, pos, new Rectangle(TileWidth * x, TileHeight * y, TileWidth, TileHeight), color, scale, rotation, spriteEffects);
            else
                DrawDefault(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, TileWidth, TileHeight));
        }

        public void DrawTile(SpriteBatch spriteBatch, Vector2 pos, int x, int y, Vector2 origin, Color color, Vector2 scale, float rotation)
        {
            if (x < TotalX && y < TotalY)
                Draw(spriteBatch, pos, new Rectangle(TileWidth * x, TileHeight * y, TileWidth, TileHeight), origin, color, scale, rotation);
            else
                DrawDefault(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, TileWidth, TileHeight));
        }

        public void DrawTile(SpriteBatch spriteBatch, Vector2 pos, int x, int y, Color color, SpriteEffects effects)
        {
            if (x < TotalX && y < TotalY)
                Draw(spriteBatch, pos, new Rectangle(TileWidth * x, TileHeight * y, TileWidth, TileHeight), color, new Vector2(1), effects);
            else
                DrawDefault(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, TileWidth, TileHeight));
        }

        public void DrawTile(SpriteBatch spriteBatch, Rectangle destRect, int x, int y, Color color)
        {
            if (x < TotalX && y < TotalY)
                Draw(spriteBatch, destRect, new Rectangle(TileWidth * x, TileHeight * y, TileWidth, TileHeight), color);
            else
                DrawDefault(spriteBatch, destRect);
        }

        public void SetTileTexture(Texture2D tex, int tileWidth, int tileHeight)
        {
            base.SetTexture(tex);
            TotalX = Width / tileWidth;
            TotalY = Height / tileHeight;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

    }
}
