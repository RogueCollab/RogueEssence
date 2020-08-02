using System;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace RogueEssence.Content
{
    public interface IEffectAnim : IDisposable
    {
        int TotalFrames { get; }
    }

    public class DirSheet : TileSheet, IEffectAnim
    {

        public enum RotateType
        {
            None,
            Dir1,//rotate the sprite to all angles
            Dir2,//rotate the sprite, using diagonal or horizontal angles
            Dir5,//5 directions; flip around
            Dir8,//8 directions full
            Flip,//2 directions; flip around
        }


        public int TotalFrames { get { return TotalX; } }
        public RotateType Dirs { get; protected set; }

        //public DirSheet(int width, int height, int tileWidth, int tileHeight, RotateType dirs)
        //    : base(width, height, tileWidth, tileHeight)
        //{
        //    Dirs = dirs;
        //}

        protected DirSheet(Texture2D tex, int tileWidth, int tileHeight, RotateType dirs)
            : base(tex, tileWidth, tileHeight)
        {
            Dirs = dirs;
        }

        //frompath (import) has two varieties: they will take
        // - a raw png with a prefix (or suffix??)
        // - a raw png with a number of frames as suffix
        //fromstream (load) will take the png and the two dimension arguments, and the dir structure from stream
        //save will save as .dir

        public static new DirSheet Import(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string[] components = fileName.Split('.');
            int frames = 0;
            if (Int32.TryParse(components[components.Length - 1], out frames))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Texture2D tex = Texture2D.FromStream(device, fileStream);
                    return new DirSheet(tex, tex.Width / frames, tex.Height, RotateType.None);
                }
            }
            else
            {
                RotateType dirs = (RotateType)Enum.Parse(typeof(RotateType), components[components.Length - 1]);
                int div = 0;
                switch (dirs)
                {
                    case RotateType.Dir2:
                        div = 2;
                        break;
                    case RotateType.Dir5:
                        div = 5;
                        break;
                    case RotateType.Dir8:
                        div = 8;
                        break;
                    default:
                        div = 1;
                        break;
                }
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Texture2D tex = Texture2D.FromStream(device, fileStream);
                    int tileWidth = tex.Height / div;
                    return new DirSheet(tex, tileWidth, tileWidth, dirs);
                }
            }
        }

        public static new DirSheet Load(BinaryReader reader)
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
            RotateType dirs = (RotateType)reader.ReadInt32();
            return new DirSheet(tex, tileWidth, tileHeight, dirs);
            
        }

        public static new DirSheet LoadError()
        {
            return new DirSheet(defaultTex, defaultTex.Width, defaultTex.Height, RotateType.None);
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write((int)Dirs);
        }


        public void DrawDir(SpriteBatch spriteBatch, Vector2 pos, int frame)
        {
            DrawDir(spriteBatch, pos, frame, Dir8.Down);
        }

        public void DrawDir(SpriteBatch spriteBatch, Vector2 pos, int frame, Dir8 dir)
        {
            DrawDir(spriteBatch, pos, frame, dir, Color.White);
        }

        public void DrawDir(SpriteBatch spriteBatch, Vector2 pos, int frame, Dir8 dir, Color color)
        {
            if (frame >= TotalFrames)
            {
                DrawDefault(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, TileWidth, TileHeight));
                return;
            }
            switch (Dirs)
            {
                case RotateType.None:
                    DrawTile(spriteBatch, pos, frame, 0, color);
                    break;
                case RotateType.Dir1:
                    DrawTile(spriteBatch, pos + new Vector2(TileWidth / 2, TileHeight / 2), frame, 0, color, (float)((int)dir * Math.PI / 4));
                    break;
                case RotateType.Dir2:
                    DrawTile(spriteBatch, pos + new Vector2(TileWidth / 2, TileHeight / 2), frame, (int)dir % 2, color, (float)(((int)dir / 2) * Math.PI / 2));
                    break;
                case RotateType.Dir5:
                    {
                        int index = (int)dir;
                        SpriteEffects flip = SpriteEffects.None;
                        if (dir > Dir8.Up)
                        {
                            //flip the sprite for the reverse angles
                            index = 8 - index;
                            flip = SpriteEffects.FlipHorizontally;
                        }
                        DrawTile(spriteBatch, pos, frame, index, color, flip);
                        break;
                    }
                case RotateType.Dir8:
                    DrawTile(spriteBatch, pos, frame, (int)dir, color);
                    break;
                case RotateType.Flip:
                    DrawTile(spriteBatch, pos, frame, 0, color, (dir >= Dir8.Up) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                    break;
            }
        }
    }

}
