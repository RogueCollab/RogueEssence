using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using RectPacker;

namespace RogueEssence.Content
{
    public class SpriteSheet : BaseSheet
    {
        protected Rectangle[] spriteRects;

        //public SpriteSheet(int width, int height, params Rectangle[] rects)
        //    : base(width, height)
        //{
        //    //Initialize vertex buffer data
        //    spriteRects = new List<Rectangle>();
        //    spriteRects.AddRange(rects);
        //}

        protected SpriteSheet(Texture2D tex, params Rectangle[] rects)
            : base(tex)
        {
            spriteRects = rects;
        }

        //frompath (import) will take a folder containing all elements
        //fromstream (load) will take the png and all the rectangles from stream
        //save will save as .atlas

        public static new SpriteSheet Import(string path)
        {
            string[] pngs = Directory.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly);
            List<ImageInfo> sheets = new List<ImageInfo>();
            int index = 0;
            foreach (string dir in pngs)
            {
                Texture2D newSheet = null;
                using (FileStream fileStream = new FileStream(dir, FileMode.Open, FileAccess.Read, FileShare.Read))
                    newSheet = Texture2D.FromStream(device, fileStream);
                sheets.Add(new ImageInfo(index, newSheet));
                index++;
            }
            if (sheets.Count == 0)
                return null;

            Canvas canvas = new Canvas();
            canvas.SetCanvasDimensions(Canvas.INFINITE_SIZE, Canvas.INFINITE_SIZE);
            OptimalMapper mapper = new OptimalMapper(canvas);
            Atlas atlas = mapper.Mapping(sheets);

            Rectangle[] rects = new Rectangle[sheets.Count];
            Texture2D tex = new Texture2D(device, atlas.Width, atlas.Height);
            for (int ii = 0; ii < atlas.MappedImages.Count; ii++)
            {
                MappedImageInfo info = atlas.MappedImages[ii];
                BaseSheet.Blit(info.ImageInfo.Texture, tex, 0, 0, info.ImageInfo.Width, info.ImageInfo.Height, info.X, info.Y);
                rects[info.ImageInfo.ID] = new Rectangle(info.X, info.Y, info.ImageInfo.Width, info.ImageInfo.Height);
                info.ImageInfo.Texture.Dispose();
            }

            return new SpriteSheet(tex, rects);
        }

        public static new SpriteSheet Load(BinaryReader reader)
        {
            long length = reader.ReadInt64();
            Texture2D tex = null;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(reader.ReadBytes((int)length), 0, (int)length);
                ms.Position = 0;
                tex = Texture2D.FromStream(device, ms);
            }

            int rectCount = reader.ReadInt32();
            Rectangle[] rects = new Rectangle[rectCount];
            for (int ii = 0; ii < rectCount; ii++)
                rects[ii] = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
            return new SpriteSheet(tex, rects);
        }

        public static new SpriteSheet LoadError()
        {
            return new SpriteSheet(defaultTex, new Rectangle[0] { });
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(spriteRects.Length);
            for (int ii = 0; ii < spriteRects.Length; ii++)
            {
                writer.Write(spriteRects[ii].X);
                writer.Write(spriteRects[ii].Y);
                writer.Write(spriteRects[ii].Width);
                writer.Write(spriteRects[ii].Height);
            }
        }

        public void DrawSprite(SpriteBatch spriteBatch, Vector2 pos, int index, Color color)
        {
            if (index < spriteRects.Length)
                Draw(spriteBatch, pos, spriteRects[index], color);
            else
                DrawDefault(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, 32, 32));
        }

    }
}
