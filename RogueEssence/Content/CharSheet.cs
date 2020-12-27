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

    public enum ActionPointType
    {
        Shadow,
        Center,
        Head,
        LeftHand,
        RightHand
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
        public int RushFrame;
        public int HitFrame;
        public int ReturnFrame;

        public List<CharAnimFrame> Frames;

        public CharAnimSequence()
        {
            RushFrame = -1;
            HitFrame = -1;
            ReturnFrame = -1;
            Frames = new List<CharAnimFrame>();
        }
    }

    public class CharAnimGroup
    {
        public int CopyOf;
        public CharAnimSequence[] Sequences;

        public CharAnimGroup()
        {
            CopyOf = -1;
            Sequences = new CharAnimSequence[DirExt.DIR8_COUNT];
            for (int ii = 0; ii < DirExt.DIR8_COUNT; ii++)
                Sequences[ii] = new CharAnimSequence();
        }
    }


    public class OffsetData
    {
        public Loc Center;
        public Loc Head;
        public Loc LeftHand;
        public Loc RightHand;

        public OffsetData()
        {

        }

        public void AddLoc(Loc loc)
        {
            Center += loc;
            Head += loc;
            LeftHand += loc;
            RightHand += loc;
        }
    }


    public class CharSheet : TileSheet
    {
        //class made specifically for characters, with their own actions and animations
        private Dictionary<int, CharAnimGroup> animData;
        public int ShadowSize { get; private set; }
        //offsets are relative to the 0,0 of each image
        private List<OffsetData> offsetData;
        //public CharSheet(int width, int height, int tileWidth, int tileHeight)
        //    : base(width, height, tileWidth, tileHeight)
        //{
        //    animData = new Dictionary<int,CharAnimGroup>();
        //}

        protected CharSheet(Texture2D tex, int tileWidth, int tileHeight, int shadowSize, Dictionary<int, CharAnimGroup> animData, List<OffsetData> offsetData)
            : base(tex, tileWidth, tileHeight)
        {
            this.animData = animData;
            this.offsetData = offsetData;
            ShadowSize = shadowSize;
        }

        //frompath (import) will take a folder containing all elements (xml and all animations, or xml and a sheet)
        //fromstream (load) will take the png, the tile height/width, and the animation data
        //save will save as .chara


        private static void mapDuplicates(List<(Texture2D img, Rectangle rect, OffsetData)> imgs, List<(Texture2D img, Rectangle rect, OffsetData)> final_imgs, CharAnimFrame[] img_map)
        {
            for (int xx = 0; xx < imgs.Count; xx++)
            {
                bool dupe = false;
                for (int yy = 0; yy < final_imgs.Count; yy++)
                {
                    CharAnimFrame imgs_equal = imgsEqual(final_imgs[yy].img, imgs[xx].img,
                        new Rectangle(0, 0, final_imgs[yy].img.Width, final_imgs[yy].img.Height),
                        new Rectangle(0, 0, imgs[xx].img.Width, imgs[xx].img.Height));
                    if (imgs_equal != null)
                    {
                        //TODO: check offsets for consistency
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

        /// <summary>
        /// Assume 0,0 to be the center of their respective images. Images can be of different sizes.
        /// </summary>
        /// <param name="img1"></param>
        /// <param name="img2"></param>
        /// <param name="rect1"></param>
        /// <param name="rect2"></param>
        /// <returns></returns>
        private static CharAnimFrame imgsEqual(Texture2D img1, Texture2D img2, Rectangle rect1, Rectangle rect2)
        {

            if (rect1.Width != rect2.Width || rect1.Height != rect2.Height)
                return null;

            int diffX = (img1.Width - img2.Width) / 2;
            int diffY = (img1.Height - img2.Height) / 2;

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

                frame.Offset = new Loc(diffX + rect2.X - rect1.X, diffY + rect2.Y - rect1.Y);
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
                frame.Offset = new Loc(diffX + rev_rect.X - rect1.X, diffY + rev_rect.Y - rect1.Y);
                frame.Flip = true;
                return frame;
            }
            return null;
        }

        private static Loc?[] getOffsetFromRGB(Texture2D img, Rectangle rect, bool black, bool r, bool g, bool b, bool white)
        {
            Color[] data = new Color[rect.Width * rect.Height];
            img.GetData<Color>(0, rect, data, 0, data.Length);

            Loc?[] results = new Loc?[5];
            //TODO: check against repeats and failures to find a value
            for (int ii = 0; ii < data.Length; ii++)
            {
                if (black && data[ii] == Color.Black)
                    results[0] = new Loc(ii % rect.Width, ii / rect.Width);
                if (r && data[ii].R == 255)
                    results[1] = new Loc(ii % rect.Width, ii / rect.Width);
                if (g && data[ii].G == 255)
                    results[2] = new Loc(ii % rect.Width, ii / rect.Width);
                if (b && data[ii].B == 255)
                    results[3] = new Loc(ii % rect.Width, ii / rect.Width);
                if (white && data[ii] == Color.White)
                    results[4] = new Loc(ii % rect.Width, ii / rect.Width);
            }
            return results;
        }

        /// <summary>
        /// Returns the rectangle bound of all nontransparent pixels within the specified bound.
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="bounds"></param>
        /// <returns>Rectangle bounds relative to input bounds.</returns>
        private static Rectangle getCoveredRect(Texture2D tex, Rectangle bounds)
        {
            int top = bounds.Height;
            int left = bounds.Width;
            int bottom = 0;
            int right = 0;
            Color[] color = new Color[bounds.Width * bounds.Height];
            tex.GetData<Color>(0, bounds, color, 0, color.Length);
            for (int ii = 0; ii < bounds.Width * bounds.Height; ii++)
            {
                if (color[ii].A > 0)
                {
                    Loc loc = new Loc(ii % bounds.Width, ii / bounds.Width);
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
            if (File.Exists(path + "AnimData.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path + "AnimData.xml");
                int shadowSize = Convert.ToInt32(doc.SelectSingleNode("AnimData/ShadowSize").InnerText);

                Dictionary<int, (Loc, CharAnimSequence, int)> sequenceList = new Dictionary<int, (Loc, CharAnimSequence, int)>();
                XmlNode sequenceNode = doc.SelectSingleNode("AnimData/Anims");
                XmlNodeList sequences = sequenceNode.SelectNodes("Anim");
                foreach (XmlNode sequence in sequences)
                {
                    CharAnimSequence frameList = new CharAnimSequence();
                    int copyOf = -1;
                    XmlNode nameNode = sequence.SelectSingleNode("Name");
                    int animIndex = GraphicsManager.Actions.FindIndex((e) => { return (String.Compare(e.Name, nameNode.InnerText, true) == 0); });
                    Loc animSize = Loc.Zero;
                    XmlNode copyOfNode = sequence.SelectSingleNode("CopyOf");
                    if (copyOfNode != null)
                        copyOf = GraphicsManager.Actions.FindIndex((e) => { return (String.Compare(e.Name, copyOfNode.InnerText, true) == 0); });
                    else
                    {
                        int tileWidth = Convert.ToInt32(sequence.SelectSingleNode("FrameWidth").InnerText);
                        int tileHeight = Convert.ToInt32(sequence.SelectSingleNode("FrameHeight").InnerText);
                        animSize = new Loc(tileWidth, tileHeight);

                        XmlNode rushFrame = sequence.SelectSingleNode("RushFrame");
                        if (rushFrame != null)
                            frameList.RushFrame = Convert.ToInt32(rushFrame.InnerText);
                        XmlNode hitFrame = sequence.SelectSingleNode("HitFrame");
                        if (hitFrame != null)
                            frameList.HitFrame = Convert.ToInt32(hitFrame.InnerText);
                        XmlNode returnFrame = sequence.SelectSingleNode("ReturnFrame");
                        if (returnFrame != null)
                            frameList.HitFrame = Convert.ToInt32(returnFrame.InnerText);

                        XmlNodeList durations = sequence.SelectNodes("Durations/Duration");
                        int totalDuration = 0;
                        foreach (XmlNode duration in durations)
                        {
                            CharAnimFrame sequenceFrame = new CharAnimFrame();
                            totalDuration += Convert.ToInt32(duration.InnerText);
                            sequenceFrame.EndTime = totalDuration;
                            frameList.Frames.Add(sequenceFrame);
                        }
                    }
                    sequenceList[animIndex] = (animSize, frameList, copyOf);
                }

                int maxWidth = 0;
                int maxHeight = 0;

                Dictionary<int, CharAnimGroup> animData = new Dictionary<int, CharAnimGroup>();
                //load all available tilesets
                List<(Texture2D, Rectangle, OffsetData)> frames = new List<(Texture2D, Rectangle, OffsetData)>();
                List<(int frameType, int dir, int frame)> frameToSequence = new List<(int, int, int)>();
                //get all frames
                //TODO: check against animations present in png files but missing from xml
                foreach (int kk in sequenceList.Keys)
                {
                    CharFrameType frameType = GraphicsManager.Actions[kk];

                    Loc tileSize = sequenceList[kk].Item1;
                    int tileWidth = tileSize.X;
                    int tileHeight = tileSize.Y;
                    CharAnimSequence preSequence = sequenceList[kk].Item2;
                    int copyOf = sequenceList[kk].Item3;


                    CharAnimGroup sequence = new CharAnimGroup();
                    if (copyOf > -1)
                    {
                        sequence.CopyOf = copyOf;
                        animData[kk] = sequence;
                        continue;
                    }

                    Texture2D animSheet = null;
                    using (FileStream fileStream = new FileStream(path + frameType.Name + ".png", FileMode.Open, FileAccess.Read, FileShare.Read))
                        animSheet = ImportTex(fileStream);
                    Texture2D shadowSheet = null;
                    using (FileStream fileStream = new FileStream(path + frameType.Name + "-Shadow.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                        shadowSheet = ImportTex(fileStream);
                    Texture2D offsetSheet = null;
                    using (FileStream fileStream = new FileStream(path + frameType.Name + "-Offsets.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                        offsetSheet = ImportTex(fileStream);

                    //TODO: check against inconsistent sizing
                    //check against inconsistent duration count

                    //automatically calculate frame durations and use preset offsets
                    for (int ii = 0; ii < DirExt.DIR8_COUNT; ii++)
                    {
                        //convert from clockwise PMD style to counterclockwise PMDO style
                        int sheetIndex = (DirExt.DIR8_COUNT - ii) % DirExt.DIR8_COUNT;
                        sheetIndex = Math.Min(sheetIndex, animSheet.Height / tileHeight - 1);
                        for (int jj = 0; jj < (animSheet.Width / tileWidth); jj++)
                        {
                            Rectangle tileRect = new Rectangle(jj * tileWidth, sheetIndex * tileHeight, tileWidth, tileHeight);
                            Rectangle rect = getCoveredRect(animSheet, tileRect);
                            if (rect.Width <= 0)
                                rect = new Rectangle(tileWidth / 2, tileHeight / 2, 1, 1);
                            Texture2D frameTex = new Texture2D(device, rect.Width, rect.Height);

                            BaseSheet.Blit(animSheet, frameTex, jj * tileWidth + rect.X, sheetIndex * tileHeight + rect.Y, rect.Width, rect.Height, 0, 0);

                            maxWidth = Math.Max(maxWidth, rect.Width);
                            maxHeight = Math.Max(maxHeight, rect.Height);
                            Loc boundsCenter = new Loc(rect.Center.X, rect.Center.Y);

                            Loc?[] shadowOffset = getOffsetFromRGB(shadowSheet, tileRect, false, false, false, false, true);
                            Loc?[] frameOffset = getOffsetFromRGB(offsetSheet, tileRect, true, true, true, true, false);
                            OffsetData offsets = new OffsetData();
                            if (frameOffset[2].HasValue)
                            {
                                offsets.Center = frameOffset[2].Value;
                                if (frameOffset[0].HasValue)
                                    offsets.Head = frameOffset[0].Value;
                                else
                                    offsets.Head = frameOffset[2].Value;
                                offsets.LeftHand = frameOffset[1].Value;
                                offsets.RightHand = frameOffset[3].Value;
                            }
                            offsets.AddLoc(-new Loc(rect.X, rect.Y));

                            frames.Add((frameTex, rect, offsets));

                            CharAnimFrame frame = new CharAnimFrame();
                            frame.Offset = boundsCenter - tileSize / 2;
                            frame.EndTime = preSequence.Frames[jj].EndTime;
                            if (shadowOffset[4].HasValue)
                                frame.ShadowOffset = shadowOffset[4].Value - tileSize / 2;
                            sequence.Sequences[ii].Frames.Add(frame);
                            frameToSequence.Add((kk, ii, jj));
                        }
                    }
                    animData[kk] = sequence;

                    animSheet.Dispose();
                    shadowSheet.Dispose();
                    offsetSheet.Dispose();
                }
                if (frames.Count == 0)
                    return null;

                CharAnimFrame[] frameMap = new CharAnimFrame[frames.Count];
                List<(Texture2D, Rectangle, OffsetData)> finalFrames = new List<(Texture2D, Rectangle, OffsetData)>();
                mapDuplicates(frames, finalFrames, frameMap);

                if (maxWidth % 2 == 1)
                    maxWidth++;
                if (maxHeight % 2 == 1)
                    maxHeight++;

                int maxSize = (int)Math.Ceiling(Math.Sqrt(finalFrames.Count));
                Texture2D tex = new Texture2D(device, maxSize * maxWidth, maxSize * maxHeight);

                List<OffsetData> offsetData = new List<OffsetData>();
                for (int ii = 0; ii < finalFrames.Count; ii++)
                {
                    int diffX = maxWidth / 2 - finalFrames[ii].Item1.Width / 2;
                    int diffY = maxHeight / 2 - finalFrames[ii].Item1.Height / 2;
                    BaseSheet.Blit(finalFrames[ii].Item1, tex, 0, 0, finalFrames[ii].Item1.Width, finalFrames[ii].Item1.Height,
                        maxWidth * (ii % maxSize) + diffX, maxHeight * (ii / maxSize) + diffY);
                    OffsetData endData = finalFrames[ii].Item3;
                    endData.AddLoc(new Loc(diffX, diffY));
                    offsetData.Add(endData);
                }

                for (int ii = 0; ii < frames.Count; ii++)
                {
                    CharAnimFrame animFrame = animData[frameToSequence[ii].frameType].Sequences[frameToSequence[ii].dir].Frames[frameToSequence[ii].frame];
                    CharAnimFrame mapFrame = frameMap[ii];
                    animFrame.Flip = mapFrame.Flip;
                    int finalIndex = mapFrame.Frame.X;
                    animFrame.Frame = new Loc(finalIndex % maxSize, finalIndex / maxSize);

                    frames[ii].Item1.Dispose();
                }

                return new CharSheet(tex, maxWidth, maxHeight, shadowSize, animData, offsetData);
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
                int tilesX = tex.Width / tileWidth;
                int tilesY = tex.Height / tileHeight;

                List<OffsetData> offsetData = new List<OffsetData>();
                for (int ii = 0; ii < tilesX * tilesY; ii++)
                    offsetData.Add(new OffsetData());

                //read anim data first
                List<CharAnimSequence> sequenceList = new List<CharAnimSequence>();

                XmlNode sequenceNode = doc.SelectSingleNode("AnimData/AnimSequenceTable");
                XmlNodeList sequences = sequenceNode.SelectNodes("AnimSequence");
                foreach (XmlNode sequence in sequences)
                {
                    CharAnimSequence frameList = new CharAnimSequence();
                    frameList.RushFrame = Convert.ToInt32(sequence.SelectSingleNode("RushPoint").InnerText);
                    frameList.HitFrame = Convert.ToInt32(sequence.SelectSingleNode("HitPoint").InnerText);
                    frameList.ReturnFrame = Convert.ToInt32(sequence.SelectSingleNode("ReturnPoint").InnerText);

                    XmlNodeList frames = sequence.SelectNodes("AnimFrame");
                    int totalDuration = 0;
                    foreach (XmlNode frame in frames)
                    {
                        CharAnimFrame sequenceFrame = new CharAnimFrame();
                        int frameIndex = Convert.ToInt32(frame.SelectSingleNode("MetaFrameGroupIndex").InnerText);
                        sequenceFrame.Frame = new Loc(frameIndex % tilesX, frameIndex / tilesX);
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
                        group.Sequences[dirIndex].RushFrame = sequenceList[indexNum].RushFrame;
                        group.Sequences[dirIndex].HitFrame = sequenceList[indexNum].HitFrame;
                        group.Sequences[dirIndex].ReturnFrame = sequenceList[indexNum].ReturnFrame;
                        foreach (CharAnimFrame frame in sequenceList[indexNum].Frames)
                            group.Sequences[dirIndex].Frames.Add(new CharAnimFrame(frame));
                        dirIndex++;
                    }
                    if (dirIndex > 0)
                        animData.Add(groupIndex, group);
                    groupIndex++;
                }
                return new CharSheet(tex, tileWidth, tileHeight, shadowSize, animData, offsetData);
            }
            else
                throw new InvalidOperationException("Error finding FrameData.xml, AnimData.xml, or Animations.xml in " + path + ".");

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

                    sequence.AppendInnerTextChild(doc, "RushPoint", frameList.RushFrame.ToString());
                    sequence.AppendInnerTextChild(doc, "HitPoint", frameList.HitFrame.ToString());
                    sequence.AppendInnerTextChild(doc, "ReturnPoint", frameList.ReturnFrame.ToString());

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

            doc.Save(baseDirectory + "AnimData.xml");

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


            int offsetCount = reader.ReadInt32();
            List<OffsetData> offsetData = new List<OffsetData>();
            for (int ii = 0; ii < offsetCount; ii++)
            {
                OffsetData offset = new OffsetData();
                offset.Center = new Loc(reader.ReadInt32(), reader.ReadInt32());
                offset.Head = new Loc(reader.ReadInt32(), reader.ReadInt32());
                offset.LeftHand = new Loc(reader.ReadInt32(), reader.ReadInt32());
                offset.RightHand = new Loc(reader.ReadInt32(), reader.ReadInt32());
                offsetData.Add(offset);
            }

            Dictionary<int, CharAnimGroup> animData = new Dictionary<int, CharAnimGroup>();

            int keyCount = reader.ReadInt32();
            for (int ii = 0; ii < keyCount; ii++)
            {
                int frameType = reader.ReadInt32();

                CharAnimGroup group = new CharAnimGroup();
                group.CopyOf = reader.ReadInt32();
                if (group.CopyOf > -1)
                {
                    animData[frameType] = group;
                    continue;
                }

                for (int jj = 0; jj < DirExt.DIR8_COUNT; jj++)
                {
                    group.Sequences[jj].RushFrame = reader.ReadInt32();
                    group.Sequences[jj].HitFrame = reader.ReadInt32();
                    group.Sequences[jj].ReturnFrame = reader.ReadInt32();
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

            return new CharSheet(tex, tileWidth, tileHeight, shadowSize, animData, offsetData);

        }

        public static new CharSheet LoadError()
        {
            return new CharSheet(defaultTex, defaultTex.Width, defaultTex.Height, 0, new Dictionary<int, CharAnimGroup>(), new List<OffsetData>());
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);

            writer.Write(ShadowSize);
            writer.Write(offsetData.Count);
            for (int ii = 0; ii < offsetData.Count; ii++)
            {
                writer.Write(offsetData[ii].Center.X);
                writer.Write(offsetData[ii].Center.Y);
                writer.Write(offsetData[ii].Head.X);
                writer.Write(offsetData[ii].Head.Y);
                writer.Write(offsetData[ii].LeftHand.X);
                writer.Write(offsetData[ii].LeftHand.Y);
                writer.Write(offsetData[ii].RightHand.X);
                writer.Write(offsetData[ii].RightHand.Y);
            }

            writer.Write(animData.Keys.Count);
            foreach (int frameType in animData.Keys)
            {
                writer.Write(frameType);
                CharAnimGroup sequence = animData[frameType];

                writer.Write(sequence.CopyOf);
                if (sequence.CopyOf > -1)
                    continue;

                for (int ii = 0; ii < DirExt.DIR8_COUNT; ii++)
                {
                    writer.Write(sequence.Sequences[ii].RushFrame);
                    writer.Write(sequence.Sequences[ii].HitFrame);
                    writer.Write(sequence.Sequences[ii].ReturnFrame);
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
            CharAnimGroup group = getReferencedAnim(type);
            if (group != null)
            {
                List<CharAnimFrame> frames = group.Sequences[(int)dir].Frames;
                CharAnimFrame frame = frames[frameMethod(frames)];
                DrawTile(spriteBatch, pos + frame.Offset.ToVector2(), frame.Frame.X, frame.Frame.Y, color, frame.Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
            else
                DrawDefault(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, TileWidth, TileHeight));
        }
        public Loc GetActionPoint(int type, Dir8 dir, ActionPointType pointType, DetermineFrame frameMethod)
        {
            CharAnimGroup group = getReferencedAnim(type);
            if (group != null)
            {
                List<CharAnimFrame> frames = group.Sequences[(int)dir].Frames;
                CharAnimFrame frame = frames[frameMethod(frames)];
                if (pointType == ActionPointType.Shadow)
                    return frame.ShadowOffset;

                OffsetData offset = offsetData[frame.Frame.Y * TotalX + frame.Frame.X];
                Loc chosenLoc = Loc.Zero;
                switch (pointType)
                {
                    case ActionPointType.Center:
                        chosenLoc = offset.Center;
                        break;
                    case ActionPointType.Head:
                        chosenLoc = offset.Head;
                        break;
                    case ActionPointType.LeftHand:
                        chosenLoc = offset.LeftHand;
                        break;
                    case ActionPointType.RightHand:
                        chosenLoc = offset.RightHand;
                        break;
                }
                if (frame.Flip)
                    chosenLoc.X = TileWidth - chosenLoc.X;
                chosenLoc -= new Loc(TileWidth, TileHeight) / 2;
                return frame.Offset + chosenLoc;
            }
            return Loc.Zero;
        }

        public void DrawCharFrame(SpriteBatch spriteBatch, int type, Dir8 dir, Vector2 pos, int frameNum, Color color)
        {
            CharAnimGroup group = getReferencedAnim(type);
            if (group != null)
            {
                CharAnimFrame frame = group.Sequences[(int)dir].Frames[frameNum];
                DrawTile(spriteBatch, pos + frame.Offset.ToVector2(), frame.Frame.X, frame.Frame.Y, color, frame.Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
            else
                DrawDefault(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, TileWidth, TileHeight));
        }

        public int GetCurrentFrame(int type, Dir8 dir, DetermineFrame frameMethod)
        {
            CharAnimGroup group = getReferencedAnim(type);
            if (group != null)
                return frameMethod(group.Sequences[(int)dir].Frames);
            else
                return 0;
        }

        public bool HasOwnAnim(int type)
        {
            CharAnimGroup group;
            if (!animData.TryGetValue(type, out group))
                return false;
            return group.CopyOf == -1;
        }

        public bool IsAnimCopied(int type)
        {
            foreach (int otherType in animData.Keys)
            {
                CharAnimGroup group = animData[otherType];
                if (group.CopyOf == type)
                    return true;
            }
            return false;
        }

        private CharAnimGroup getReferencedAnim(int type)
        {
            CharAnimGroup group;
            if (!animData.TryGetValue(type, out group))
                return null;

            while (group.CopyOf > -1)
                group = animData[group.CopyOf];
            return group;
        }

        public int GetTotalTime(int type, Dir8 dir)
        {
            CharAnimGroup group = getReferencedAnim(type);
            if (group != null)
            {
                CharAnimSequence frameList = group.Sequences[(int)dir];
                if (frameList.Frames.Count > 0)
                    return frameList.Frames[frameList.Frames.Count - 1].EndTime;
            }
            return 0;
        }


        public int GetReturnTime(int type, Dir8 dir)
        {
            CharAnimGroup group = getReferencedAnim(type);
            if (group != null)
            {
                CharAnimSequence frameList = group.Sequences[(int)dir];
                if (frameList.ReturnFrame > -1)
                    return frameList.Frames[frameList.ReturnFrame].EndTime;
                else
                    return frameList.Frames[frameList.Frames.Count - 1].EndTime;
            }
            return 0;
        }


        public int GetHitTime(int type, Dir8 dir)
        {
            CharAnimGroup group = getReferencedAnim(type);
            if (group != null)
            {
                CharAnimSequence frameList = group.Sequences[(int)dir];
                if (frameList.HitFrame > -1)
                    return frameList.Frames[frameList.HitFrame].EndTime;
                else
                    return frameList.Frames[frameList.Frames.Count - 1].EndTime;
            }
            return 0;
        }


        public int GetRushTime(int type, Dir8 dir)
        {
            CharAnimGroup group = getReferencedAnim(type);
            if (group != null)
            {
                CharAnimSequence frameList = group.Sequences[(int)dir];
                if (frameList.RushFrame > -1)
                    return frameList.Frames[frameList.RushFrame].EndTime;
                else
                    return 0;
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
