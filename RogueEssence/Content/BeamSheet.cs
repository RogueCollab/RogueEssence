using System;
using System.Collections.Generic;
using RogueElements;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Content
{
    public class BeamSheet : SpriteSheet, IEffectAnim
    {
        private enum BeamFrame
        {
            Head,
            Body,
            Tail
        }

        public int TotalFrames { get; private set; }

        public BeamSheet(Texture2D tex, Rectangle[] rects, int totalFrames)
            :base(tex, rects)
        {
            TotalFrames = totalFrames;
        }

        //frompath (import) will take a folder containing all elements
        //fromstream (load) will take the png, and the rectangles (head.body/tail are precalculated)
        //save will save as .beam

        public static new BeamSheet Import(string path)
        {
            if (File.Exists(path + "BeamData.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path + "BeamData.xml");
                int totalFrames = Convert.ToInt32(doc.SelectSingleNode("BeamData/TotalFrames").InnerText);

                List<Texture2D> sheets = new List<Texture2D>();
                Rectangle[] rects = new Rectangle[3 * totalFrames];
                int maxWidth = 0;
                int maxHeight = 0;

                for (int ii = 0; ii < 3; ii++)
                {
                    using (FileStream fileStream = new FileStream(path + ((BeamFrame)ii).ToString() + ".png", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        Texture2D newSheet = ImportTex(fileStream);

                        for (int jj = 0; jj < totalFrames; jj++)
                            rects[ii * totalFrames + jj] = new Rectangle(newSheet.Width / totalFrames * jj, maxHeight, newSheet.Width / totalFrames, newSheet.Height);

                        maxWidth = Math.Max(maxWidth, newSheet.Width);
                        maxHeight += newSheet.Height;
                        sheets.Add(newSheet);
                    }
                }
                
                Texture2D tex = new Texture2D(device, maxWidth, maxHeight);

                int curHeight = 0;
                for (int ii = 0; ii < sheets.Count; ii++)
                {
                    BaseSheet.Blit(sheets[ii], tex, 0, 0, sheets[ii].Width, sheets[ii].Height, 0, curHeight);
                    curHeight += sheets[ii].Height;
                    sheets[ii].Dispose();
                }

                return new BeamSheet(tex, rects, totalFrames);
            }
            else
                throw new Exception("Error finding XML file in " + path + ".");
        }

        public static new BeamSheet Load(BinaryReader reader)
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
            int frameCount = reader.ReadInt32();
            return new BeamSheet(tex, rects, frameCount);
            
        }

        public static new BeamSheet LoadError()
        {
            Rectangle[] rects = new Rectangle[3];
            for (int ii = 0; ii < rects.Length; ii++)
                rects[ii] = new Rectangle(0, 0, defaultTex.Width, defaultTex.Height);
            return new BeamSheet(defaultTex, rects, 1);
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(TotalFrames);
        }

        private Rectangle getBeamFrame(BeamFrame component, int frame)
        {
            int index = (int)component * TotalFrames + frame;
            return spriteRects[index];
        }

        public void DrawBeam(SpriteBatch spriteBatch, Vector2 pos, int frame, Dir8 dir, int offset, int length, Color color)
        {
            Loc dirLoc = dir.GetLoc();

            Loc diff = dirLoc * (length + offset);
            Rectangle body = getBeamFrame(BeamFrame.Body, frame);
            Draw(spriteBatch, new Vector2(pos.X + diff.X, pos.Y + diff.Y), body, new Vector2(body.Width / 2, body.Height),
                color, new Vector2(1, dir.IsDiagonal() ? (float)(length * 1.4142136 + 1) : length), (float)((int)dir * Math.PI / 4));

            diff = dirLoc * offset;
            Rectangle tail = getBeamFrame(BeamFrame.Tail, frame);
            Draw(spriteBatch, new Vector2(pos.X + diff.X, pos.Y + diff.Y), tail, color, new Vector2(1), (float)((int)dir * Math.PI / 4));

            diff = dirLoc * (length + offset - 1);
            Rectangle head = getBeamFrame(BeamFrame.Head, frame);
            Draw(spriteBatch, new Vector2(pos.X + diff.X, pos.Y + diff.Y), head, color, new Vector2(1), (float)((int)dir * Math.PI / 4));
        }

        public void DrawColumn(SpriteBatch spriteBatch, Vector2 pos, int frame, Color color)
        {
            Rectangle head = getBeamFrame(BeamFrame.Head, frame);
            Draw(spriteBatch, pos - new Vector2(head.Width / 2, head.Height / 2), head, color);

            Rectangle body = getBeamFrame(BeamFrame.Body, frame);
            while (pos.Y > 0)
            {
                Draw(spriteBatch, pos - new Vector2(body.Width / 2, body.Height), body, color);
                pos.Y -= body.Height;
            }
        }
    }
}
