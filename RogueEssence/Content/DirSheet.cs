using System;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml;
using System.Collections.Generic;

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


        public int TotalFrames { get; protected set; }
        public RotateType Dirs { get; protected set; }

        //public DirSheet(int width, int height, int tileWidth, int tileHeight, RotateType dirs)
        //    : base(width, height, tileWidth, tileHeight)
        //{
        //    Dirs = dirs;
        //}

        protected DirSheet(Texture2D tex, int tileWidth, int tileHeight, int totalFrames, RotateType dirs)
            : base(tex, tileWidth, tileHeight)
        {
            Dirs = dirs;
            TotalFrames = totalFrames;
        }

        //frompath (import) has two varieties: they will take
        // - a raw png with a prefix (or suffix??)
        // - a raw png with a number of frames as suffix
        //fromstream (load) will take the png and the two dimension arguments, and the dir structure from stream
        //save will save as .dir

        public static new DirSheet Import(string path)
        {
            if (Directory.Exists(path))//assume directory structure
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(path, "DirData.xml"));
                RotateType totalDirs = Enum.Parse<RotateType>(doc.SelectSingleNode("DirData/DirType").InnerText);

                List<(Color[] tex, int width, int height)> frames = new List<(Color[], int, int)>();
                while (true)
                {
                    string pngFile = Path.Combine(path, frames.Count.ToString() + ".png");
                    if (!File.Exists(pngFile))
                        break;

                    using (FileStream fileStream = new FileStream(pngFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (Texture2D tex = ImportTex(fileStream))
                            frames.Add((BaseSheet.GetData(tex), tex.Width, tex.Height));
                    }

                    //dimensions consistency check
                    if (frames.Count > 1)
                    {
                        if (frames[0].width != frames[frames.Count - 1].width && frames[0].height != frames[frames.Count - 1].height)
                            throw new Exception(String.Format("Frame {0} does not match previous frame's dimensions!", frames.Count-1));
                    }
                }

                int totalPixels = frames.Count * frames[0].width * frames[0].height;
                int tilesX = (int)Math.Ceiling(Math.Sqrt(totalPixels) / frames[0].width);
                int tilesY = MathUtils.DivUp(frames.Count, tilesX);

                int tileWidth = frames[0].width;
                int tileHeight = frames[0].height;
                int dirHeight = tileHeight / getDirDiv(totalDirs);
                int maxWidth = tilesX * tileWidth;
                int maxHeight = tilesY * tileHeight;

                Color[] texColors = new Color[maxWidth * maxHeight];

                for (int ii = 0; ii < frames.Count; ii++)
                {
                    int xx = ii % tilesX;
                    int yy = ii / tilesX;
                    BaseSheet.Blit(frames[ii].tex, texColors, new Point(tileWidth, tileHeight), new Point(maxWidth, maxHeight), new Point(tileWidth * xx, tileHeight * yy), SpriteEffects.None);
                }

                Texture2D full = new Texture2D(device, maxWidth, maxHeight);
                full.SetData<Color>(0, null, texColors, 0, texColors.Length);
                return new DirSheet(full, tileWidth, dirHeight, frames.Count, totalDirs);
            }
            else //assume png file
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                string[] components = fileName.Split('.');
                string typeString = components[components.Length - 1];
                int framesW = 0;
                if (Int32.TryParse(typeString, out framesW))
                {
                    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        Texture2D tex = ImportTex(fileStream);
                        return new DirSheet(tex, tex.Width / framesW, tex.Height, framesW, RotateType.None);
                    }
                }
                string[] dims = typeString.Split("x");
                int framesH = 0;
                if (dims.Length == 2 && Int32.TryParse(dims[0], out framesW) && Int32.TryParse(dims[1], out framesH))
                {
                    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        Texture2D tex = ImportTex(fileStream);
                        return new DirSheet(tex, tex.Width / framesW, tex.Height / framesH, framesW * framesH, RotateType.None);
                    }
                }


                RotateType dirs = Enum.Parse<RotateType>(typeString);
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Texture2D tex = ImportTex(fileStream);
                    int tileWidth = tex.Height / getDirDiv(dirs);
                    return new DirSheet(tex, tileWidth, tileWidth, tex.Width / tileWidth, dirs);
                }
            }
            throw new Exception("Unsupported file extension.");
        }

        public static int getDirDiv(RotateType dirs)
        {
            int div;
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
            return div;
        }

        public static string GetExportString(DirSheet sheet, string basename)
        {
            string suffix;
            int dirs = getDirDiv(sheet.Dirs);
            if (sheet.TotalY > dirs)//uses the second tier or more
            {
                if (sheet.Dirs == RotateType.None && sheet.TotalFrames % sheet.TotalX == 0)
                    suffix = string.Format("{0}x{1}", sheet.TotalX.ToString(), sheet.TotalY.ToString());
                else
                    suffix = "";
            }
            else
            {
                switch (sheet.Dirs)
                {
                    case RotateType.None:
                        {
                            if (sheet.TileWidth == sheet.TileHeight)//all square frames on one tier
                                suffix = sheet.Dirs.ToString();
                            else//rectangular frames on one tier
                                suffix = sheet.TotalFrames.ToString();
                        }
                        break;
                    default:
                        suffix = sheet.Dirs.ToString();
                        break;
                }
            }
            if (suffix == "")
                return basename;
            return basename + "." + suffix;
        }

        public static void Export(DirSheet sheet, string filepath, bool singleFrames)
        {
            if (singleFrames)
            {
                throw new NotImplementedException("Exporting folders not yet done.");
            }
            else
            {
                using (Stream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
                    ExportTex(stream, sheet.baseTexture);
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
            int totalFrames = reader.ReadInt32();
            return new DirSheet(tex, tileWidth, tileHeight, totalFrames, dirs);
            
        }

        public static new DirSheet LoadError()
        {
            return new DirSheet(defaultTex, defaultTex.Width, defaultTex.Height, 1, RotateType.None);
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write((int)Dirs);
            writer.Write(TotalFrames);
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
            DrawDir(spriteBatch, pos, frame, dir, color, SpriteFlip.None);
        }

        public void DrawDir(SpriteBatch spriteBatch, Vector2 pos, int frame, Dir8 dir, Color color, SpriteFlip spriteFlip)
        {
            if (frame >= TotalFrames)
            {
                DrawDefault(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, TileWidth, TileHeight));
                return;
            }
            SpriteEffects flip = (SpriteEffects)((int)spriteFlip);
            switch (Dirs)
            {
                case RotateType.None:
                    DrawTile(spriteBatch, pos, frame % TotalX, frame / TotalX, color, flip);
                    break;
                case RotateType.Dir1:
                    DrawTile(spriteBatch, pos + new Vector2(TileWidth / 2, TileHeight / 2), frame % TotalX, frame / TotalX, color, (float)((int)dir * Math.PI / 4), flip);
                    break;
                case RotateType.Dir2:
                    DrawTile(spriteBatch, pos + new Vector2(TileWidth / 2, TileHeight / 2), frame % TotalX, frame / TotalX * 2 + (int)dir % 2, color, (float)(((int)dir / 2) * Math.PI / 2), flip);
                    break;
                case RotateType.Dir5:
                    {
                        int index = (int)dir;
                        if (dir > Dir8.Up)
                        {
                            //flip the sprite for the reverse angles
                            index = 8 - index;
                            flip ^= SpriteEffects.FlipHorizontally;
                        }
                        DrawTile(spriteBatch, pos, frame % TotalX, frame / TotalX * 5 + index, color, flip);
                        break;
                    }
                case RotateType.Dir8:
                    DrawTile(spriteBatch, pos, frame % TotalX, frame / TotalX * 8 + (int)dir, color, flip);
                    break;
                case RotateType.Flip:
                    {
                        if (dir >= Dir8.Up)
                            flip ^= SpriteEffects.FlipHorizontally;
                        DrawTile(spriteBatch, pos, frame % TotalX, frame / TotalX, color, flip);
                        break;
                    }
            }
        }
    }

}
