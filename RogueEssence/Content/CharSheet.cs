using System;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Collections.Generic;
using System.Xml;

namespace RogueEssence.Content
{

    public class CharFrameType
    {
        public string Name;
        public bool IsDash;

        public CharFrameType(string name, bool isDash)
        {
            Name = name;
            IsDash = isDash;
        }
    }

    public class CharAnimFrame
    {
        //a single frame would have:
        //TileTexture, Duration, offset, shadow offset, potentially other offsets
        //a full animation would denote keyframes
        //frame width/height are still used
        public Loc Frame;
        public int EndTime;
        public bool Flip;
        public Loc Offset;
        public Loc ShadowOffset;

        public CharAnimFrame() { }
        public CharAnimFrame(CharAnimFrame other)
        {
            Frame = other.Frame;
            EndTime = other.EndTime;
            Flip = other.Flip;
            Offset = other.Offset;
            ShadowOffset = other.ShadowOffset;
        }
    }

    public class CharAnimSequence
    {
        public int RushPoint;
        public int HitPoint;
        public int ReturnPoint;

        public List<CharAnimFrame> Frames;

        public CharAnimSequence()
        {
            Frames = new List<CharAnimFrame>();
        }
    }

    public class CharAnimGroup
    {

        public CharAnimSequence[] Sequences;

        public CharAnimGroup()
        {
            Sequences = new CharAnimSequence[DirExt.DIR8_COUNT];
            for (int ii = 0; ii < DirExt.DIR8_COUNT; ii++)
                Sequences[ii] = new CharAnimSequence();
        }
    }



    public class CharSheet : TileSheet
    {
        //class made specifically for characters, with their own actions and animations
        private Dictionary<int, CharAnimGroup> animData;
        public int ShadowSize { get; private set; }
        //public CharSheet(int width, int height, int tileWidth, int tileHeight)
        //    : base(width, height, tileWidth, tileHeight)
        //{
        //    animData = new Dictionary<int,CharAnimGroup>();
        //}

        protected CharSheet(Texture2D tex, int tileWidth, int tileHeight, int shadowSize, Dictionary<int, CharAnimGroup> animData)
            : base(tex, tileWidth, tileHeight)
        {
            this.animData = animData;
            ShadowSize = shadowSize;
        }

        //frompath (import) will take a folder containing all elements (xml and all animations, or xml and a sheet)
        //fromstream (load) will take the png, the tile height/width, and the animation data
        //save will save as .chara


        private static void mapDuplicates(List<(Texture2D img, Rectangle rect)> imgs, List<(Texture2D img, Rectangle rect)> final_imgs, CharAnimFrame[] img_map)
        {
            for (int xx = 0; xx < imgs.Count; xx++)
            {
                bool dupe = false;
                for (int yy = 0; yy < final_imgs.Count; yy++)
                {
                    CharAnimFrame imgs_equal = imgsEqual(final_imgs[yy].img, imgs[xx].img, final_imgs[yy].rect, imgs[xx].rect);
                    if (imgs_equal != null)
                    {
                        imgs_equal.Frame = new Loc(yy, 0);
                        img_map[xx] = imgs_equal;
                        dupe = true;
                        break;
                    }
                }


                if (!dupe)
                {
                    CharAnimFrame frame = new CharAnimFrame();
                    frame.Frame = new Loc(final_imgs.Count, 0);
                    img_map[xx] = frame;
                    final_imgs.Add(imgs[xx]);
                }
            }
        }

        private static CharAnimFrame imgsEqual(Texture2D img1, Texture2D img2, Rectangle rect1, Rectangle rect2)
        {

            if (rect1.Width != rect2.Width || rect1.Height != rect2.Height)
                return null;


            Color[] data1 = new Color[rect1.Width * rect1.Height];
            img1.GetData<Color>(0, rect1, data1, 0, data1.Length);
            Color[] data2 = new Color[rect2.Width * rect2.Height];
            img2.GetData<Color>(0, rect2, data2, 0, data2.Length);

            //check against similarity
            bool equal = true;
            for (int ii = 0; ii < data1.Length; ii++)
            {
                if (data1[ii] != data2[ii])
                    equal = false;
                if (!equal)
                    break;
            }

            //check against flip similarity

            if (equal)
            {
                CharAnimFrame frame = new CharAnimFrame();
                frame.Offset = new Loc(rect2.X - rect1.X, rect2.Y - rect1.Y);
                return frame;
            }

            equal = true;
            for (int ii = 0; ii < data1.Length; ii++)
            {
                int yPoint = (ii / rect2.Width + 1) * rect2.Width;
                int reverse_ii = yPoint - 1 - (ii - yPoint + rect2.Width);
                if (data1[ii] != data2[reverse_ii])
                    equal = false;
                if (!equal)
                    break;
            }

            if (equal)
            {
                Rectangle rev_rect = new Rectangle(img1.Width - rect2.Right, rect2.Y, img1.Width - rect2.X, rect2.Height);
                CharAnimFrame frame = new CharAnimFrame();
                frame.Offset = new Loc(rev_rect.X - rect1.X, rev_rect.Y - rect1.Y);
                frame.Flip = true;
                return frame;
            }
            return null;
        }

        private static Rectangle getCoveredRect(Texture2D tex)
        {
            int top = tex.Height;
            int left = tex.Width;
            int bottom = 0;
            int right = 0;
            Color[] color = new Color[tex.Width * tex.Height];
            tex.GetData<Color>(0, new Rectangle(0, 0, tex.Width, tex.Height), color, 0, color.Length);
            for (int ii = 0; ii < tex.Width * tex.Height; ii++)
            {
                if (color[ii].A > 0)
                {
                    Loc loc = new Loc(ii % tex.Width, ii / tex.Width);
                    top = Math.Min(loc.Y, top);
                    left = Math.Min(loc.X, left);
                    bottom = Math.Max(loc.Y + 1, bottom);
                    right = Math.Max(loc.X + 1, right);
                }
            }
            return new Rectangle(left, top, right - left, bottom - top);
        }

        public static new CharSheet Import(string path)
        {
            if (File.Exists(path + "FrameData.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path + "FrameData.xml");
                int tileWidth = Convert.ToInt32(doc.SelectSingleNode("FrameData/FrameWidth").InnerText);
                int tileHeight = Convert.ToInt32(doc.SelectSingleNode("FrameData/FrameHeight").InnerText);
                int shadowSize = Convert.ToInt32(doc.SelectSingleNode("FrameData/ShadowSize").InnerText);
                int framerate = Convert.ToInt32(doc.SelectSingleNode("FrameData/FrameRate").InnerText);

                Dictionary<int, CharAnimGroup> animData = new Dictionary<int, CharAnimGroup>();

                //load all available tilesets
                List<(Texture2D, Rectangle)> frames = new List<(Texture2D, Rectangle)>();
                List<(int frameType, int dir, int frame)> frameToSequence = new List<(int, int, int)>();
                //get all frames
                for (int kk = 0; kk < GraphicsManager.Actions.Count; kk++)
                {
                    CharFrameType frameType = GraphicsManager.Actions[kk];
                    if (File.Exists(path + frameType.Name.ToString() + ".png"))
                    {
                        Texture2D newSheet = null;
                        using (FileStream fileStream = new FileStream(path + frameType.Name.ToString() + ".png", FileMode.Open, FileAccess.Read, FileShare.Read))
                            newSheet = ImportTex(fileStream);

                        CharAnimGroup sequence = new CharAnimGroup();
                        //automatically calculate frame durations and use preset offsets
                        for (int ii = 0; ii < DirExt.DIR8_COUNT; ii++)
                        {
                            int index = ii;
                            bool flip = false;
                            if (ii >= (newSheet.Height / tileHeight))
                            {
                                if ((newSheet.Height / tileHeight) == 1)
                                    index = 0;
                                else
                                {
                                    index = DirExt.DIR8_COUNT - ii;
                                    flip = true;
                                }
                            }
                            int totalTime = 0;
                            for (int jj = 0; jj < (newSheet.Width / tileWidth); jj++)
                            {
                                Texture2D frameTex = new Texture2D(device, tileWidth, tileHeight);

                                BaseSheet.Blit(newSheet, frameTex, jj * tileWidth, index * tileHeight, tileWidth, tileHeight, 0, 0);
                                Rectangle rect = getCoveredRect(frameTex);
                                frames.Add((frameTex, rect));

                                CharAnimFrame frame = new CharAnimFrame();
                                frame.Flip = flip;
                                totalTime += framerate;
                                frame.EndTime = totalTime;
                                sequence.Sequences[ii].Frames.Add(frame);
                                frameToSequence.Add((kk, ii, jj));
                            }
                        }
                        animData[kk] = sequence;

                        newSheet.Dispose();
                    }
                }
                if (frames.Count == 0)
                    return null;

                CharAnimFrame[] frameMap = new CharAnimFrame[frames.Count];
                List<(Texture2D, Rectangle)> finalFrames = new List<(Texture2D, Rectangle)>();
                mapDuplicates(frames, finalFrames, frameMap);


                int maxSize = (int)Math.Ceiling(Math.Sqrt(finalFrames.Count));
                Texture2D tex = new Texture2D(device, maxSize * tileWidth, maxSize * tileHeight);

                for (int ii = 0; ii < finalFrames.Count; ii++)
                    BaseSheet.Blit(finalFrames[ii].Item1, tex, 0, 0, tileWidth, tileHeight, tileWidth * (ii % maxSize), tileHeight * (ii / maxSize));

                for (int ii = 0; ii < frames.Count; ii++)
                {
                    CharAnimFrame animFrame = animData[frameToSequence[ii].frameType].Sequences[frameToSequence[ii].dir].Frames[frameToSequence[ii].frame];
                    CharAnimFrame mapFrame = frameMap[ii];
                    animFrame.Flip = (mapFrame.Flip != animFrame.Flip);
                    animFrame.Offset = animFrame.Offset + mapFrame.Offset;
                    int finalIndex = mapFrame.Frame.X;
                    animFrame.Frame = new Loc(finalIndex % maxSize, finalIndex / maxSize);

                    frames[ii].Item1.Dispose();
                }

                return new CharSheet(tex, tileWidth, tileHeight, shadowSize, animData);
            }
            else if (File.Exists(path + "Animations.xml"))
            {
                Texture2D tex = null;
                using (FileStream fileStream = new FileStream(path + "sheet.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                    tex = ImportTex(fileStream);

                XmlDocument doc = new XmlDocument();
                doc.Load(path + "Animations.xml");

                int tileWidth = Convert.ToInt32(doc.SelectSingleNode("AnimData/FrameWidth").InnerText);
                int tileHeight = Convert.ToInt32(doc.SelectSingleNode("AnimData/FrameHeight").InnerText);
                int shadowSize = Convert.ToInt32(doc.SelectSingleNode("AnimData/ShadowSize").InnerText);

                //read anim data first
                List<CharAnimSequence> sequenceList = new List<CharAnimSequence>();

                XmlNode sequenceNode = doc.SelectSingleNode("AnimData/AnimSequenceTable");
                XmlNodeList sequences = sequenceNode.SelectNodes("AnimSequence");
                foreach (XmlNode sequence in sequences)
                {
                    CharAnimSequence frameList = new CharAnimSequence();
                    frameList.RushPoint = Convert.ToInt32(sequence.SelectSingleNode("RushPoint").InnerText);
                    frameList.HitPoint = Convert.ToInt32(sequence.SelectSingleNode("HitPoint").InnerText);
                    frameList.ReturnPoint = Convert.ToInt32(sequence.SelectSingleNode("ReturnPoint").InnerText);

                    XmlNodeList frames = sequence.SelectNodes("AnimFrame");
                    int totalDuration = 0;
                    foreach (XmlNode frame in frames)
                    {
                        CharAnimFrame sequenceFrame = new CharAnimFrame();
                        int frameIndex = Convert.ToInt32(frame.SelectSingleNode("MetaFrameGroupIndex").InnerText);
                        sequenceFrame.Frame = new Loc(frameIndex % (tex.Width / tileWidth), frameIndex / (tex.Width / tileWidth));
                        totalDuration += Convert.ToInt32(frame.SelectSingleNode("Duration").InnerText);
                        sequenceFrame.EndTime = totalDuration;
                        sequenceFrame.Flip = Convert.ToBoolean(Convert.ToInt32(frame.SelectSingleNode("HFlip").InnerText));
                        XmlNode offset = frame.SelectSingleNode("Sprite");
                        sequenceFrame.Offset = new Loc(Convert.ToInt32(offset.SelectSingleNode("XOffset").InnerText), Convert.ToInt32(offset.SelectSingleNode("YOffset").InnerText));
                        XmlNode shadow = frame.SelectSingleNode("Shadow");
                        sequenceFrame.ShadowOffset = new Loc(Convert.ToInt32(shadow.SelectSingleNode("XOffset").InnerText), Convert.ToInt32(shadow.SelectSingleNode("YOffset").InnerText));
                        frameList.Frames.Add(sequenceFrame);
                    }
                    sequenceList.Add(frameList);
                }

                Dictionary<int, CharAnimGroup> animData = new Dictionary<int, CharAnimGroup>();

                XmlNode groupNode = doc.SelectSingleNode("AnimData/AnimGroupTable");
                XmlNodeList animGroups = groupNode.SelectNodes("AnimGroup");
                int groupIndex = 0;
                foreach (XmlNode animGroup in animGroups)
                {
                    XmlNodeList sequenceIndices = animGroup.SelectNodes("AnimSequenceIndex");
                    int dirIndex = 0;
                    CharAnimGroup group = new CharAnimGroup();
                    foreach (XmlNode sequenceIndex in sequenceIndices)
                    {
                        int indexNum = Convert.ToInt32(sequenceIndex.InnerText);
                        group.Sequences[dirIndex] = new CharAnimSequence();
                        group.Sequences[dirIndex].RushPoint = sequenceList[indexNum].RushPoint;
                        group.Sequences[dirIndex].HitPoint = sequenceList[indexNum].HitPoint;
                        group.Sequences[dirIndex].ReturnPoint = sequenceList[indexNum].ReturnPoint;
                        foreach (CharAnimFrame frame in sequenceList[indexNum].Frames)
                            group.Sequences[dirIndex].Frames.Add(new CharAnimFrame(frame));
                        dirIndex++;
                    }
                    if (dirIndex > 0)
                        animData.Add(groupIndex, group);
                    groupIndex++;
                }

                return new CharSheet(tex, tileWidth, tileHeight, shadowSize, animData);
            }
            else
                throw new InvalidOperationException("Error finding FrameData.xml or Animations.xml in " + path + ".");

        }


        public static void Export(CharSheet sheet, string baseDirectory)
        {
            using (Stream stream = new FileStream(baseDirectory + "sheet.png", FileMode.Create, FileAccess.Write, FileShare.None))
                ExportTex(stream, sheet.baseTexture);


            XmlDocument doc = new XmlDocument();
            XmlNode configNode = doc.CreateXmlDeclaration("1.0", null, null);
            doc.AppendChild(configNode);



            XmlNode docNode = doc.CreateElement("AnimData");
            docNode.AppendInnerTextChild(doc, "FrameWidth", sheet.TileWidth.ToString());
            docNode.AppendInnerTextChild(doc, "FrameHeight", sheet.TileHeight.ToString());
            docNode.AppendInnerTextChild(doc, "ShadowSize", sheet.ShadowSize.ToString());

            List<CharAnimSequence> sequenceList = new List<CharAnimSequence>();
            {
                XmlNode groupNode = doc.CreateElement("AnimGroupTable");

                for (int ii = 0; ii < GraphicsManager.Actions.Count; ii++)
                {
                    XmlNode animGroup = doc.CreateElement("AnimGroup");

                    if (sheet.animData.ContainsKey(ii))
                    {
                        foreach (CharAnimSequence sequence in sheet.animData[ii].Sequences)
                        {
                            animGroup.AppendInnerTextChild(doc, "AnimSequenceIndex", sequenceList.Count.ToString());
                            sequenceList.Add(sequence);
                        }
                    }

                    groupNode.AppendChild(animGroup);
                }

                docNode.AppendChild(groupNode);
            }

            {
                XmlNode sequenceNode = doc.CreateElement("AnimSequenceTable");

                foreach (CharAnimSequence frameList in sequenceList)
                {
                    XmlNode sequence = doc.CreateElement("AnimSequence");

                    sequence.AppendInnerTextChild(doc, "RushPoint", frameList.RushPoint.ToString());
                    sequence.AppendInnerTextChild(doc, "HitPoint", frameList.HitPoint.ToString());
                    sequence.AppendInnerTextChild(doc, "ReturnPoint", frameList.ReturnPoint.ToString());

                    int duration = 0;
                    foreach (CharAnimFrame sequenceFrame in frameList.Frames)
                    {
                        XmlNode frame = doc.CreateElement("AnimFrame");

                        int frameIndex = sequenceFrame.Frame.Y * sheet.TotalX + sequenceFrame.Frame.X;
                        frame.AppendInnerTextChild(doc, "MetaFrameGroupIndex", frameIndex.ToString());

                        frame.AppendInnerTextChild(doc, "Duration", (sequenceFrame.EndTime - duration).ToString());
                        duration = sequenceFrame.EndTime;

                        frame.AppendInnerTextChild(doc, "HFlip", sequenceFrame.Flip ? "1" : "0");

                        XmlNode offset = doc.CreateElement("Sprite");
                        offset.AppendInnerTextChild(doc, "XOffset", sequenceFrame.Offset.X.ToString());
                        offset.AppendInnerTextChild(doc, "YOffset", sequenceFrame.Offset.Y.ToString());
                        frame.AppendChild(offset);

                        XmlNode shadow = doc.CreateElement("Shadow");
                        shadow.AppendInnerTextChild(doc, "XOffset", sequenceFrame.ShadowOffset.X.ToString());
                        shadow.AppendInnerTextChild(doc, "YOffset", sequenceFrame.ShadowOffset.Y.ToString());
                        frame.AppendChild(shadow);

                        sequence.AppendChild(frame);
                    }

                    sequenceNode.AppendChild(sequence);
                }

                docNode.AppendChild(sequenceNode);
            }

            doc.AppendChild(docNode);

            doc.Save(baseDirectory + "Animations.xml");

        }

        public static new CharSheet Load(BinaryReader reader)
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
            int shadowSize = reader.ReadInt32();

            Dictionary<int, CharAnimGroup> animData = new Dictionary<int, CharAnimGroup>();

            int keyCount = reader.ReadInt32();
            for (int ii = 0; ii < keyCount; ii++)
            {
                int frameType = reader.ReadInt32();

                CharAnimGroup group = new CharAnimGroup();
                for (int jj = 0; jj < DirExt.DIR8_COUNT; jj++)
                {
                    group.Sequences[jj].RushPoint = reader.ReadInt32();
                    group.Sequences[jj].HitPoint = reader.ReadInt32();
                    group.Sequences[jj].ReturnPoint = reader.ReadInt32();
                    int frameCount = reader.ReadInt32();
                    int totalTime = 0;
                    for (int kk = 0; kk < frameCount; kk++)
                    {
                        CharAnimFrame frame = new CharAnimFrame();
                        frame.Frame = new Loc(reader.ReadInt32(), reader.ReadInt32());
                        totalTime += reader.ReadInt32();
                        frame.EndTime = totalTime;
                        frame.Flip = reader.ReadBoolean();
                        frame.Offset = new Loc(reader.ReadInt32(), reader.ReadInt32());
                        frame.ShadowOffset = new Loc(reader.ReadInt32(), reader.ReadInt32());
                        group.Sequences[jj].Frames.Add(frame);
                    }
                }
                animData[frameType] = group;
            }

            return new CharSheet(tex, tileWidth, tileHeight, shadowSize, animData);

        }

        public static new CharSheet LoadError()
        {
            return new CharSheet(defaultTex, defaultTex.Width, defaultTex.Height, 0, new Dictionary<int, CharAnimGroup>());
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);

            writer.Write(ShadowSize);
            writer.Write(animData.Keys.Count);
            foreach (int frameType in animData.Keys)
            {
                writer.Write(frameType);
                CharAnimGroup sequence = animData[frameType];
                for (int ii = 0; ii < DirExt.DIR8_COUNT; ii++)
                {
                    writer.Write(sequence.Sequences[ii].RushPoint);
                    writer.Write(sequence.Sequences[ii].HitPoint);
                    writer.Write(sequence.Sequences[ii].ReturnPoint);
                    writer.Write(sequence.Sequences[ii].Frames.Count);
                    int prevTime = 0;
                    for (int jj = 0; jj < sequence.Sequences[ii].Frames.Count; jj++)
                    {
                        CharAnimFrame frame = sequence.Sequences[ii].Frames[jj];
                        writer.Write(frame.Frame.X);
                        writer.Write(frame.Frame.Y);
                        writer.Write(frame.EndTime - prevTime);
                        prevTime = frame.EndTime;
                        writer.Write(frame.Flip);
                        writer.Write(frame.Offset.X);
                        writer.Write(frame.Offset.Y);
                        writer.Write(frame.ShadowOffset.X);
                        writer.Write(frame.ShadowOffset.Y);
                    }
                }
            }
        }

        //need a way to determine frame the old fashioned way,
        //however, also need a way to determine frame for an animation playing at the true specified speed
        public void DrawChar(SpriteBatch spriteBatch, int type, Dir8 dir, Vector2 pos, DetermineFrame frameMethod, Color color)
        {
            if (animData.ContainsKey(type))
            {
                List<CharAnimFrame> frames = animData[type].Sequences[(int)dir].Frames;
                CharAnimFrame frame = frames[frameMethod(frames)];
                DrawTile(spriteBatch, pos + frame.Offset.ToVector2(), frame.Frame.X, frame.Frame.Y, color, frame.Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
            else
                DrawDefault(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, TileWidth, TileHeight));
        }
        public void DrawShadow(SpriteBatch spriteBatch, int type, Dir8 dir, Vector2 pos, Loc shadowType, DetermineFrame frameMethod)
        {
            if (animData.ContainsKey(type))
            {
                List<CharAnimFrame> frames = animData[type].Sequences[(int)dir].Frames;
                CharAnimFrame frame = frames[frameMethod(frames)];
                GraphicsManager.Shadows.DrawTile(spriteBatch,
                    pos + new Vector2(TileWidth / 2, TileHeight / 2 + 4) - new Vector2(GraphicsManager.Shadows.TileWidth / 2, GraphicsManager.Shadows.TileHeight / 2) + frame.ShadowOffset.ToVector2(),
                    shadowType.X, shadowType.Y);
            }
        }

        public void DrawCharFrame(SpriteBatch spriteBatch, int type, Dir8 dir, Vector2 pos, int frameNum, Color color)
        {
            if (animData.ContainsKey(type))
            {
                CharAnimFrame frame = animData[type].Sequences[(int)dir].Frames[frameNum];
                DrawTile(spriteBatch, pos + frame.Offset.ToVector2(), frame.Frame.X, frame.Frame.Y, color, frame.Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
            else
                DrawDefault(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, TileWidth, TileHeight));
        }

        public int GetCurrentFrame(int type, Dir8 dir, DetermineFrame frameMethod)
        {
            if (animData.ContainsKey(type))
                return frameMethod(animData[type].Sequences[(int)dir].Frames);
            else
                return 0;
        }

        public bool HasAnim(int type)
        {
            return animData.ContainsKey(type);
        }

        public int GetTotalTime(int type, Dir8 dir)
        {
            if (animData.ContainsKey(type))
            {
                CharAnimSequence frameList = animData[type].Sequences[(int)dir];
                if (frameList.Frames.Count > 0)
                    return frameList.Frames[frameList.Frames.Count - 1].EndTime;
            }
            return 0;
        }


        public int GetReturnTime(int type, Dir8 dir)
        {
            if (animData.ContainsKey(type))
            {
                CharAnimSequence frameList = animData[type].Sequences[(int)dir];
                return frameList.ReturnPoint;
            }
            return 0;
        }


        public int GetHitTime(int type, Dir8 dir)
        {
            if (animData.ContainsKey(type))
            {
                CharAnimSequence frameList = animData[type].Sequences[(int)dir];
                return frameList.HitPoint;
            }
            return 0;
        }


        public int GetRushTime(int type, Dir8 dir)
        {
            if (animData.ContainsKey(type))
            {
                CharAnimSequence frameList = animData[type].Sequences[(int)dir];
                return frameList.RushPoint;
            }
            return 0;
        }


        /// <summary>
        /// Computes the current frame based on fraction of a total time, using a specified total time.
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="time"></param>
        /// <param name="totalTime"></param>
        /// <param name="clamp"></param>
        /// <returns></returns>
        public static int FractionFrame(List<CharAnimFrame> frames, long time, long totalTime, bool clamp)
        {
            if (totalTime == 0)
                return 0;
            long adjTime = clamp ? Math.Min(time, totalTime) : time % totalTime;
            for (int ii = 0; ii < frames.Count; ii++)
            {
                if (adjTime < frames[ii].EndTime * totalTime / frames[frames.Count - 1].EndTime)
                    return ii;
            }
            return frames.Count - 1;
        }

        /// <summary>
        /// Computes the current frame based on time, using the frame durations of the animation.
        /// </summary>
        /// <param name="time">Time into the animation</param>
        /// <param name="clamp">Whether to clamp the frame if the time exceeds the full anim time.</param>
        /// <returns></returns>
        public static int TrueFrame(List<CharAnimFrame> frames, long time, bool clamp)
        {
            if (frames[frames.Count - 1].EndTime == 0)
                return 0;
            long adjTime = clamp ? Math.Min(time, FrameTick.FrameToTick(frames[frames.Count - 1].EndTime)) : time % FrameTick.FrameToTick(frames[frames.Count - 1].EndTime);
            for (int ii = 0; ii < frames.Count; ii++)
            {
                if (adjTime < FrameTick.FrameToTick(frames[ii].EndTime))
                    return ii;
            }
            return frames.Count - 1;
        }

        public static int SpecFrame(List<CharAnimFrame> frames, int frame, bool clamp)
        {
            int adjFrame = clamp ? Math.Min(frame, frames.Count) : frame % frames.Count;
            return adjFrame;
        }

        public delegate int DetermineFrame(List<CharAnimFrame> frames);

    }
}
