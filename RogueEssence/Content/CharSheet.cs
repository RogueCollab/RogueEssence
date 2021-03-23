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
        public List<int> Fallbacks;

        public CharFrameType(string name, bool isDash)
        {
            Name = name;
            IsDash = isDash;
            Fallbacks = new List<int>();
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
        public List<CharAnimFrame> Frames;

        public CharAnimSequence()
        {
            Frames = new List<CharAnimFrame>();
        }
    }

    public class CharAnimGroup
    {
        public int InternalIndex;
        public int CopyOf;
        public List<CharAnimSequence> Sequences;

        public int RushFrame;
        public int HitFrame;
        public int ReturnFrame;

        public CharAnimGroup()
        {
            InternalIndex = -1;
            CopyOf = -1;
            Sequences = new List<CharAnimSequence>();
            RushFrame = -1;
            HitFrame = -1;
            ReturnFrame = -1;
        }

        public CharAnimSequence SeqAtDir(Dir8 dir)
        {
            int sequenceIndex = Math.Min((int)dir, Sequences.Count - 1);
            return Sequences[sequenceIndex];
        }
    }


    public class OffsetData
    {
        public Loc CenterFlip { get { return flip(Center); } }
        public Loc HeadFlip { get { return flip(Head); } }
        public Loc LeftHandFlip { get { return flip(LeftHand); } }
        public Loc RightHandFlip { get { return flip(RightHand); } }

        public Loc Center { get; private set; }
        public Loc Head { get; private set; }
        public Loc LeftHand { get; private set; }
        public Loc RightHand { get; private set; }

        public OffsetData()
        { }
        public OffsetData(Loc center, Loc head, Loc leftHand, Loc rightHand)
        {
            this.Center = center;
            this.Head = head;
            this.LeftHand = leftHand;
            this.RightHand = rightHand;
        }

        private Loc flip(Loc loc)
        {
            return new Loc(-loc.X - 1, loc.Y);
        }

        public void AddLoc(Loc loc)
        {
            Center += loc;
            Head += loc;
            LeftHand += loc;
            RightHand += loc;
        }


        public Rectangle GetCoveredRect()
        {
            int top = Math.Min(Math.Min(Center.Y, Head.Y), Math.Min(LeftHand.Y, RightHand.Y));
            int left = Math.Min(Math.Min(Center.X, Head.X), Math.Min(LeftHand.X, RightHand.X));
            int bottom = Math.Max(Math.Max(Center.Y, Head.Y), Math.Max(LeftHand.Y, RightHand.Y)) + 1;
            int right = Math.Max(Math.Max(Center.X, Head.X), Math.Max(LeftHand.X, RightHand.X)) + 1;

            return new Rectangle(left, top, right - left, bottom - top);
        }
    }


    public class CharSheet : TileSheet
    {
        //class made specifically for characters, with their own actions and animations
        public Dictionary<int, CharAnimGroup> AnimData;
        public int ShadowSize;
        //offsets are relative to the center of each image
        public List<OffsetData> OffsetData;


        protected CharSheet(Texture2D tex, int tileWidth, int tileHeight, int shadowSize, Dictionary<int, CharAnimGroup> animData, List<OffsetData> offsetData)
            : base(tex, tileWidth, tileHeight)
        {
            this.AnimData = animData;
            this.OffsetData = offsetData;
            this.ShadowSize = shadowSize;
        }

        //frompath (import) will take a folder containing all elements (xml and all animations, or xml and a sheet)
        //fromstream (load) will take the png, the tile height/width, and the animation data
        //save will save as .chara


        private static void mapDuplicates(List<(Color[] img, Rectangle rect, OffsetData offsets)> imgs, List<(Color[] img, Rectangle rect, OffsetData offsets)> final_imgs, CharAnimFrame[] img_map, bool offsetCheck)
        {
            for (int xx = 0; xx < imgs.Count; xx++)
            {
                bool dupe = false;
                for (int yy = 0; yy < final_imgs.Count; yy++)
                {
                    Point szFinal = new Point(final_imgs[yy].rect.Width, final_imgs[yy].rect.Height);
                    Point sz = new Point(imgs[xx].rect.Width, imgs[xx].rect.Height);
                    bool imgs_equal = imgsEqual(final_imgs[yy].img, imgs[xx].img, szFinal, sz, false) && (!offsetCheck || offsetsEqual(final_imgs[yy].offsets, imgs[xx].offsets, false, sz.X % 2 == 1));
                    if (imgs_equal)
                    {
                        CharAnimFrame map_frame = new CharAnimFrame();
                        map_frame.Frame = new Loc(yy, 0);
                        img_map[xx] = map_frame;
                        dupe = true;
                        break;
                    }
                    bool flip_equal = imgsEqual(final_imgs[yy].img, imgs[xx].img, szFinal, sz, true) && (!offsetCheck || offsetsEqual(final_imgs[yy].offsets, imgs[xx].offsets, true, sz.X % 2 == 1));
                    if (flip_equal)
                    {
                        CharAnimFrame map_frame = new CharAnimFrame();
                        map_frame.Frame = new Loc(yy, 0);
                        map_frame.Flip = true;
                        img_map[xx] = map_frame;
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
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <param name="sz1"></param>
        /// <param name="sz2"></param>
        /// <param name="flip"></param>
        /// <returns></returns>
        private static bool imgsEqual(Color[] data1, Color[] data2, Point sz1, Point sz2, bool flip)
        {
            if (sz1 != sz2)
                return false;

            for (int ii = 0; ii < data1.Length; ii++)
            {
                int result_ii = ii;
                if (flip)
                {
                    int yPoint = ii / sz1.X;
                    int xPoint = ii % sz1.X;
                    result_ii = (yPoint + 1) * sz1.X - 1 - xPoint;
                }
                if (data1[ii] != data2[result_ii])
                    return false;
            }
            return true;
        }

        private static bool offsetsEqual(OffsetData offset1, OffsetData offset2, bool flip, bool oddWidth)
        {
            Loc center = offset2.Center;
            Loc head = offset2.Head;
            Loc leftHand = offset2.LeftHand;
            Loc rightHand = offset2.RightHand;
            if (flip)
            {
                center = offset2.CenterFlip + (oddWidth ? Loc.UnitX : Loc.Zero);
                head = offset2.HeadFlip + (oddWidth ? Loc.UnitX : Loc.Zero);
                leftHand = offset2.LeftHandFlip + (oddWidth ? Loc.UnitX : Loc.Zero);
                rightHand = offset2.RightHandFlip + (oddWidth ? Loc.UnitX : Loc.Zero);
            }

            if (offset1.Center != center)
                return false;
            if (offset1.Head != head)
                return false;
            if (offset1.LeftHand != leftHand)
                return false;
            if (offset1.RightHand != rightHand)
                return false;

            return true;
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

        public static new CharSheet Import(string path)
        {
            if (File.Exists(path + "Animations.xml"))
            {
                Texture2D tex = null;
                using (FileStream fileStream = new FileStream(path + "sheet.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                    tex = ImportTex(fileStream);

                XmlDocument doc = new XmlDocument();
                doc.Load(path + "Animations.xml");

                int tileWidth = Convert.ToInt32(doc.SelectSingleNode("AnimData/FrameWidth").InnerText);
                int tileHeight = Convert.ToInt32(doc.SelectSingleNode("AnimData/FrameHeight").InnerText);
                if (tileWidth % 2 == 1 || tileHeight % 2 == 1)
                    throw new InvalidDataException("Tile width and tile height must be even numbers!");
                int shadowSize = Convert.ToInt32(doc.SelectSingleNode("AnimData/ShadowSize").InnerText);
                int tilesX = tex.Width / tileWidth;
                int tilesY = tex.Height / tileHeight;

                Loc center = new Loc(tileWidth, tileHeight) / 2;
                List<OffsetData> offsetData = new List<OffsetData>();
                for (int ii = 0; ii < tilesX * tilesY; ii++)
                    offsetData.Add(new OffsetData(Loc.Zero, Loc.Zero, Loc.Zero, Loc.Zero));

                //read anim data first
                List<(CharAnimSequence sequence, int rushFrame, int hitFrame, int returnFrame)> sequenceList = new List<(CharAnimSequence, int, int, int)>();

                XmlNode sequenceNode = doc.SelectSingleNode("AnimData/AnimSequenceTable");
                XmlNodeList sequences = sequenceNode.SelectNodes("AnimSequence");
                foreach (XmlNode sequence in sequences)
                {
                    CharAnimSequence frameList = new CharAnimSequence();
                    int rushTime = Convert.ToInt32(sequence.SelectSingleNode("RushPoint").InnerText);
                    int hitTime = Convert.ToInt32(sequence.SelectSingleNode("HitPoint").InnerText);
                    int returnTime = Convert.ToInt32(sequence.SelectSingleNode("ReturnPoint").InnerText);
                    int rushFrame = -1;
                    int hitFrame = -1;
                    int returnFrame = -1;

                    XmlNodeList frames = sequence.SelectNodes("AnimFrame");
                    int totalDuration = 0;
                    foreach (XmlNode frame in frames)
                    {
                        CharAnimFrame sequenceFrame = new CharAnimFrame();
                        int frameIndex = Convert.ToInt32(frame.SelectSingleNode("MetaFrameGroupIndex").InnerText);
                        sequenceFrame.Frame = new Loc(frameIndex % tilesX, frameIndex / tilesX);
                        if (totalDuration < rushTime)
                            rushFrame = frameList.Frames.Count;
                        if (totalDuration < hitTime)
                            hitFrame = frameList.Frames.Count;
                        if (totalDuration < returnTime)
                            returnFrame = frameList.Frames.Count;
                        totalDuration += Convert.ToInt32(frame.SelectSingleNode("Duration").InnerText);
                        sequenceFrame.EndTime = totalDuration;
                        sequenceFrame.Flip = Convert.ToBoolean(Convert.ToInt32(frame.SelectSingleNode("HFlip").InnerText));
                        XmlNode offset = frame.SelectSingleNode("Sprite");
                        sequenceFrame.Offset = new Loc(Convert.ToInt32(offset.SelectSingleNode("XOffset").InnerText), Convert.ToInt32(offset.SelectSingleNode("YOffset").InnerText));
                        XmlNode shadow = frame.SelectSingleNode("Shadow");
                        sequenceFrame.ShadowOffset = new Loc(Convert.ToInt32(shadow.SelectSingleNode("XOffset").InnerText), Convert.ToInt32(shadow.SelectSingleNode("YOffset").InnerText));
                        frameList.Frames.Add(sequenceFrame);
                    }
                    sequenceList.Add((frameList, rushFrame, hitFrame, returnFrame));
                }

                Dictionary<int, CharAnimGroup> animData = new Dictionary<int, CharAnimGroup>();

                XmlNode groupNode = doc.SelectSingleNode("AnimData/AnimGroupTable");
                int groupIndex = 0;
                foreach (XmlNode animGroupNode in groupNode.SelectNodes("AnimGroup"))
                {
                    XmlNodeList sequenceIndices = animGroupNode.SelectNodes("AnimSequenceIndex");
                    int dirIndex = 0;
                    CharAnimGroup group = new CharAnimGroup();
                    foreach (XmlNode sequenceIndex in sequenceIndices)
                    {
                        int indexNum = Convert.ToInt32(sequenceIndex.InnerText);
                        CharAnimSequence sequence = new CharAnimSequence();
                        group.RushFrame = sequenceList[indexNum].rushFrame;
                        group.HitFrame = sequenceList[indexNum].hitFrame;
                        group.ReturnFrame = sequenceList[indexNum].returnFrame;
                        foreach (CharAnimFrame frame in sequenceList[indexNum].sequence.Frames)
                            sequence.Frames.Add(new CharAnimFrame(frame));
                        group.Sequences.Add(sequence);
                        dirIndex++;
                    }
                    if (dirIndex > 0)
                        animData.Add(groupIndex, group);
                    groupIndex++;
                }
                return new CharSheet(tex, tileWidth, tileHeight, shadowSize, animData, offsetData);
            }
            else if (File.Exists(path + "FrameData.xml"))
            {
                Texture2D tex = null;
                using (FileStream fileStream = new FileStream(path + "Anim.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                    tex = ImportTex(fileStream);

                XmlDocument doc = new XmlDocument();
                doc.Load(path + "FrameData.xml");

                int tileWidth = Convert.ToInt32(doc.SelectSingleNode("AnimData/FrameWidth").InnerText);
                int tileHeight = Convert.ToInt32(doc.SelectSingleNode("AnimData/FrameHeight").InnerText);
                if (tileWidth % 2 == 1 || tileHeight % 2 == 1)
                    throw new InvalidDataException("Tile width and tile height must be even numbers!");
                int shadowSize = Convert.ToInt32(doc.SelectSingleNode("AnimData/ShadowSize").InnerText);
                int tilesX = tex.Width / tileWidth;
                int tilesY = tex.Height / tileHeight;


                int maxFrames = 0;
                Dictionary<int, CharAnimGroup> animData = new Dictionary<int, CharAnimGroup>();
                XmlNode animsNode = doc.SelectSingleNode("AnimData/Anims");
                foreach (XmlNode animGroupNode in animsNode.SelectNodes("Anim"))
                {
                    CharAnimGroup animGroup = new CharAnimGroup();
                    XmlNode nameNode = animGroupNode.SelectSingleNode("Name");
                    int animIndex = GraphicsManager.Actions.FindIndex((e) => { return (String.Compare(e.Name, nameNode.InnerText, true) == 0); });
                    if (animIndex == -1)
                        throw new InvalidDataException(String.Format("Could not find index for anim named '{0}'!", nameNode.InnerText));

                    XmlNode internalIndexNode = animGroupNode.SelectSingleNode("Index");
                    if (internalIndexNode != null)
                        animGroup.InternalIndex = Convert.ToInt32(internalIndexNode.InnerText);

                    XmlNode copyOfNode = animGroupNode.SelectSingleNode("CopyOf");
                    if (copyOfNode != null)
                        animGroup.CopyOf = GraphicsManager.Actions.FindIndex((e) => { return (String.Compare(e.Name, copyOfNode.InnerText, true) == 0); });
                    else
                    {
                        XmlNode rushFrame = animGroupNode.SelectSingleNode("RushFrame");
                        if (rushFrame != null)
                            animGroup.RushFrame = Convert.ToInt32(rushFrame.InnerText);
                        XmlNode hitFrame = animGroupNode.SelectSingleNode("HitFrame");
                        if (hitFrame != null)
                            animGroup.HitFrame = Convert.ToInt32(hitFrame.InnerText);
                        XmlNode returnFrame = animGroupNode.SelectSingleNode("ReturnFrame");
                        if (returnFrame != null)
                            animGroup.ReturnFrame = Convert.ToInt32(returnFrame.InnerText);

                        XmlNode sequencesNode = animGroupNode.SelectSingleNode("Sequences");
                        foreach (XmlNode sequenceNode in sequencesNode.SelectNodes("AnimSequence"))
                        {
                            CharAnimSequence sequence = new CharAnimSequence();

                            XmlNodeList frames = sequenceNode.SelectNodes("AnimFrame");
                            int totalDuration = 0;
                            foreach (XmlNode frame in frames)
                            {
                                CharAnimFrame sequenceFrame = new CharAnimFrame();
                                int frameIndex = Convert.ToInt32(frame.SelectSingleNode("FrameIndex").InnerText);
                                maxFrames = Math.Max(frameIndex, maxFrames);
                                sequenceFrame.Frame = new Loc(frameIndex % tilesX, frameIndex / tilesX);
                                totalDuration += Convert.ToInt32(frame.SelectSingleNode("Duration").InnerText);
                                sequenceFrame.EndTime = totalDuration;
                                sequenceFrame.Flip = Convert.ToBoolean(Convert.ToInt32(frame.SelectSingleNode("HFlip").InnerText));
                                XmlNode offset = frame.SelectSingleNode("Sprite");
                                sequenceFrame.Offset = new Loc(Convert.ToInt32(offset.SelectSingleNode("XOffset").InnerText), Convert.ToInt32(offset.SelectSingleNode("YOffset").InnerText));
                                XmlNode shadow = frame.SelectSingleNode("Shadow");
                                sequenceFrame.ShadowOffset = new Loc(Convert.ToInt32(shadow.SelectSingleNode("XOffset").InnerText), Convert.ToInt32(shadow.SelectSingleNode("YOffset").InnerText));
                                sequence.Frames.Add(sequenceFrame);
                            }
                            animGroup.Sequences.Add(sequence);
                        }
                    }
                    animData[animIndex] = animGroup;
                }



                Texture2D offsetTex = null;
                using (FileStream fileStream = new FileStream(path + "Offsets.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                    offsetTex = ImportTex(fileStream);
                Loc tileCenter = new Loc(tileWidth, tileHeight) / 2;
                List<OffsetData> offsetData = new List<OffsetData>();
                for (int ii = 0; ii < tilesX * tilesY; ii++)
                {
                    if (ii > maxFrames)
                        break;
                    Loc tileLoc = new Loc(ii % tilesX, ii / tilesX);
                    Rectangle tileRect = new Rectangle(tileLoc.X * tileWidth, tileLoc.Y * tileHeight, tileWidth, tileHeight);
                    Loc?[] frameOffset = getOffsetFromRGB(offsetTex, tileRect, true, true, true, true, false);
                    OffsetData offsets = new OffsetData(tileCenter, tileCenter, tileCenter, tileCenter);
                    if (frameOffset[2].HasValue)
                    {
                        Loc center = frameOffset[2].Value;
                        Loc head = center;
                        if (frameOffset[0].HasValue)
                            head = frameOffset[0].Value;
                        Loc leftHand = frameOffset[1].Value;
                        Loc rightHand = frameOffset[3].Value;
                        offsets = new OffsetData(center, head, leftHand, rightHand);
                    }
                    //center the offsets to the center of the frametex
                    offsets.AddLoc(-tileCenter);

                    offsetData.Add(offsets);
                }
                offsetTex.Dispose();

                return new CharSheet(tex, tileWidth, tileHeight, shadowSize, animData, offsetData);
            }
            else if (File.Exists(path + "AnimData.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path + "AnimData.xml");
                int shadowSize = Convert.ToInt32(doc.SelectSingleNode("AnimData/ShadowSize").InnerText);

                Dictionary<int, (Loc size, CharAnimGroup group, List<int> endTimes)> sequenceList = new Dictionary<int, (Loc, CharAnimGroup, List<int>)>();
                XmlNode animsNode = doc.SelectSingleNode("AnimData/Anims");
                foreach (XmlNode animGroupNode in animsNode.SelectNodes("Anim"))
                {
                    CharAnimGroup animGroup = new CharAnimGroup();
                    List<int> endList = new List<int>();
                    XmlNode nameNode = animGroupNode.SelectSingleNode("Name");
                    int animIndex = GraphicsManager.Actions.FindIndex((e) => { return (String.Compare(e.Name, nameNode.InnerText, true) == 0); });
                    if (animIndex == -1)
                        throw new InvalidDataException(String.Format("Could not find index for anim named '{0}'!", nameNode.InnerText));

                    XmlNode internalIndexNode = animGroupNode.SelectSingleNode("Index");
                    if (internalIndexNode != null)
                        animGroup.InternalIndex = Convert.ToInt32(internalIndexNode.InnerText);
                    Loc animSize = Loc.Zero;
                    XmlNode copyOfNode = animGroupNode.SelectSingleNode("CopyOf");
                    if (copyOfNode != null)
                        animGroup.CopyOf = GraphicsManager.Actions.FindIndex((e) => { return (String.Compare(e.Name, copyOfNode.InnerText, true) == 0); });
                    else
                    {
                        int tileWidth = Convert.ToInt32(animGroupNode.SelectSingleNode("FrameWidth").InnerText);
                        int tileHeight = Convert.ToInt32(animGroupNode.SelectSingleNode("FrameHeight").InnerText);
                        animSize = new Loc(tileWidth, tileHeight);

                        XmlNode rushFrame = animGroupNode.SelectSingleNode("RushFrame");
                        if (rushFrame != null)
                            animGroup.RushFrame = Convert.ToInt32(rushFrame.InnerText);
                        XmlNode hitFrame = animGroupNode.SelectSingleNode("HitFrame");
                        if (hitFrame != null)
                            animGroup.HitFrame = Convert.ToInt32(hitFrame.InnerText);
                        XmlNode returnFrame = animGroupNode.SelectSingleNode("ReturnFrame");
                        if (returnFrame != null)
                            animGroup.ReturnFrame = Convert.ToInt32(returnFrame.InnerText);

                        XmlNodeList durations = animGroupNode.SelectNodes("Durations/Duration");
                        int totalDuration = 0;
                        foreach (XmlNode duration in durations)
                        {
                            CharAnimFrame sequenceFrame = new CharAnimFrame();
                            totalDuration += Convert.ToInt32(duration.InnerText);
                            endList.Add(totalDuration);
                        }
                    }
                    sequenceList[animIndex] = (animSize, animGroup, endList);
                }

                int maxWidth = 0;
                int maxHeight = 0;

                Dictionary<int, CharAnimGroup> animData = new Dictionary<int, CharAnimGroup>();
                //load all available tilesets
                List<(Color[] img, Rectangle rect, OffsetData offsets)> frames = new List<(Color[], Rectangle, OffsetData)>();
                List<(int frameType, int dir, int frame)> frameToSequence = new List<(int, int, int)>();
                //get all frames
                //TODO: check against animations present in png files but missing from xml
                foreach (int kk in sequenceList.Keys)
                {
                    CharFrameType frameType = GraphicsManager.Actions[kk];

                    Loc tileSize = sequenceList[kk].size;
                    int tileWidth = tileSize.X;
                    int tileHeight = tileSize.Y;
                    List<int> endList = sequenceList[kk].endTimes;

                    CharAnimGroup animGroup = sequenceList[kk].group;
                    if (animGroup.CopyOf > -1)
                    {
                        animData[kk] = animGroup;
                        continue;
                    }

                    Texture2D animSheet = null;
                    using (FileStream fileStream = new FileStream(path + frameType.Name + "-Anim.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                        animSheet = ImportTex(fileStream);
                    Texture2D shadowSheet = null;
                    using (FileStream fileStream = new FileStream(path + frameType.Name + "-Shadow.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                        shadowSheet = ImportTex(fileStream);
                    Texture2D offsetSheet = null;
                    using (FileStream fileStream = new FileStream(path + frameType.Name + "-Offsets.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                        offsetSheet = ImportTex(fileStream);


                    //check against inconsistent sizing
                    if (animSheet.Width != shadowSheet.Width || animSheet.Width != offsetSheet.Width ||
                        animSheet.Height != shadowSheet.Height || animSheet.Height != offsetSheet.Height)
                        throw new InvalidDataException(String.Format("Anim/Offset/Shadow images of {0} are not the same size!", frameType.Name));

                    //check against bad tile size
                    if (animSheet.Width % tileWidth != 0 || animSheet.Height % tileHeight != 0)
                        throw new InvalidDataException(String.Format("Anim sheet {0} is {1}x{2} and is not divisible by a tile size of {3}x{4}!", frameType.Name, animSheet.Width, animSheet.Height, tileWidth, tileHeight));

                    int totalX = animSheet.Width / tileWidth;
                    int totalY = animSheet.Height / tileHeight;
                    //check against inconsistent duration count
                    if (endList.Count != totalX)
                        throw new InvalidDataException(String.Format("Duration list of {0} is not the same length as the number of frames in the anim!", frameType.Name));

                    //check against bad sequence amounts
                    if (totalY != 1 && totalY != 8)
                        throw new InvalidDataException(String.Format("Anim for {0} must be one-directional or 8-directional!", frameType.Name));

                    //automatically calculate frame durations and use preset offsets
                    for (int ii = 0; ii < DirExt.DIR8_COUNT; ii++)
                    {
                        //convert from clockwise PMD style to counterclockwise PMDO style
                        int sheetIndex = (DirExt.DIR8_COUNT - ii) % DirExt.DIR8_COUNT;
                        if (sheetIndex >= totalY)
                            continue;
                        CharAnimSequence sequence = new CharAnimSequence();
                        for (int jj = 0; jj < (animSheet.Width / tileWidth); jj++)
                        {
                            Rectangle tileRect = new Rectangle(jj * tileWidth, sheetIndex * tileHeight, tileWidth, tileHeight);
                            Rectangle imgCoveredRect = GetCoveredRect(animSheet, tileRect);
                            if (imgCoveredRect.Width <= 0)
                                imgCoveredRect = new Rectangle(tileWidth / 2, tileHeight / 2, 1, 1);

                            //first blit, then edit the rect sizes
                            Color[] frameTex = BaseSheet.GetData(animSheet, jj * tileWidth + imgCoveredRect.X, sheetIndex * tileHeight + imgCoveredRect.Y, imgCoveredRect.Width, imgCoveredRect.Height);
                            //the final tile size must be able to contain this image
                            maxWidth = Math.Max(maxWidth, imgCoveredRect.Width);
                            maxHeight = Math.Max(maxHeight, imgCoveredRect.Height);

                            Loc boundsCenter = new Loc(imgCoveredRect.Center.X, imgCoveredRect.Center.Y);

                            Loc?[] frameOffset = getOffsetFromRGB(offsetSheet, tileRect, true, true, true, true, false);
                            OffsetData offsets = new OffsetData(boundsCenter, boundsCenter, boundsCenter, boundsCenter);
                            if (frameOffset[2].HasValue)
                            {
                                Loc center = frameOffset[2].Value;
                                Loc head = center;
                                if (frameOffset[0].HasValue)
                                    head = frameOffset[0].Value;
                                Loc leftHand = frameOffset[1].Value;
                                Loc rightHand = frameOffset[3].Value;
                                offsets = new OffsetData(center, head, leftHand, rightHand);
                            }
                            //center the offsets to the center of the frametex
                            offsets.AddLoc(-boundsCenter);

                            //get the farthest that the offsets can cover, relative to the image
                            Rectangle offsetCoveredRect = offsets.GetCoveredRect();
                            Rectangle centeredOffsetRect = centerBounds(offsetCoveredRect);

                            //the final tile size must be able to contain the offsets
                            maxWidth = Math.Max(maxWidth, centeredOffsetRect.Width);
                            maxHeight = Math.Max(maxHeight, centeredOffsetRect.Height);


                            frames.Add((frameTex, imgCoveredRect, offsets));

                            CharAnimFrame frame = new CharAnimFrame();
                            frame.Offset = boundsCenter - tileSize / 2;
                            frame.EndTime = endList[jj];

                            Loc?[] shadowOffset = getOffsetFromRGB(shadowSheet, tileRect, false, false, false, false, true);
                            if (shadowOffset[4].HasValue)
                                frame.ShadowOffset = shadowOffset[4].Value - tileSize / 2;
                            
                            sequence.Frames.Add(frame);
                            frameToSequence.Add((kk, ii, jj));
                        }
                        animGroup.Sequences.Add(sequence);
                    }
                    animData[kk] = animGroup;

                    animSheet.Dispose();
                    shadowSheet.Dispose();
                    offsetSheet.Dispose();
                }
                if (frames.Count == 0)
                    return null;

                //make frame sizes even
                maxWidth = roundUpToMult(maxWidth, 2);
                maxHeight = roundUpToMult(maxHeight, 2);

                CharAnimFrame[] frameMap = new CharAnimFrame[frames.Count];
                List<(Color[] img, Rectangle rect, OffsetData offsets)> finalFrames = new List<(Color[], Rectangle, OffsetData)>();
                mapDuplicates(frames, finalFrames, frameMap, true);

                int maxSize = (int)Math.Ceiling(Math.Sqrt(finalFrames.Count));
                Point texSize = new Point(maxSize * maxWidth, maxSize * maxHeight);
                Color[] texColors = new Color[texSize.X * texSize.Y];

                List <OffsetData> offsetData = new List<OffsetData>();
                for (int ii = 0; ii < finalFrames.Count; ii++)
                {
                    int diffX = maxWidth / 2 - finalFrames[ii].rect.Width / 2;
                    int diffY = maxHeight / 2 - finalFrames[ii].rect.Height / 2;
                    BaseSheet.Blit(finalFrames[ii].img, texColors, new Point(finalFrames[ii].rect.Width, finalFrames[ii].rect.Height), texSize,
                        new Point(maxWidth * (ii % maxSize) + diffX, maxHeight * (ii / maxSize) + diffY), SpriteEffects.None);
                    OffsetData endData = finalFrames[ii].offsets;

                    offsetData.Add(endData);
                }

                for (int ii = 0; ii < frames.Count; ii++)
                {
                    CharAnimFrame animFrame = animData[frameToSequence[ii].frameType].Sequences[frameToSequence[ii].dir].Frames[frameToSequence[ii].frame];
                    CharAnimFrame mapFrame = frameMap[ii];
                    animFrame.Flip = mapFrame.Flip;
                    int finalIndex = mapFrame.Frame.X;
                    //if an image with an odd number of pixels is flipped, it must be moved over slightly
                    if (animFrame.Flip && frames[ii].rect.Width % 2 == 1)
                        animFrame.Offset = animFrame.Offset + Loc.UnitX;
                    animFrame.Frame = new Loc(finalIndex % maxSize, finalIndex / maxSize);
                }

                // automatically add default animation
                {
                    CharAnimGroup anim = new CharAnimGroup();
                    CharAnimGroup parentGroup = animData[GraphicsManager.IdleAction];
                    while(parentGroup.CopyOf > -1)
                        parentGroup = animData[parentGroup.CopyOf];
                    foreach (CharAnimSequence sequence in parentGroup.Sequences)
                    {
                        CharAnimSequence newSequence = new CharAnimSequence();
                        CharAnimFrame frame = new CharAnimFrame(sequence.Frames[0]);
                        frame.EndTime = 1;
                        newSequence.Frames.Add(frame);
                        anim.Sequences.Add(newSequence);
                    }
                    animData[0] = anim;
                }

                Texture2D tex = new Texture2D(device, maxSize * maxWidth, maxSize * maxHeight);
                tex.SetData<Color>(0, null, texColors, 0, texColors.Length);

                return new CharSheet(tex, maxWidth, maxHeight, shadowSize, animData, offsetData);
            }
            else
                throw new InvalidOperationException("Error finding AnimData.xml, FrameData.xml or Animations.xml in " + path + ".");

        }

        public void CollapseOffsets()
        {
            int maxWidth = 0;
            int maxHeight = 0;

            Dictionary<int, CharAnimGroup> animData = new Dictionary<int, CharAnimGroup>();
            //load all available tilesets
            List<(Color[] img, Rectangle rect, OffsetData offsets)> frames = new List<(Color[], Rectangle, OffsetData)>();
            //get all frames
            for (int kk = 0; kk < OffsetData.Count; kk++)
            {
                int xx = kk % TotalX;
                int yy = kk / TotalX;
                Rectangle tileRect = new Rectangle(xx * TileWidth, yy * TileHeight, TileWidth, TileHeight);
                Rectangle imgCoveredRect = GetCoveredRect(baseTexture, tileRect);
                if (imgCoveredRect.Width <= 0)
                    imgCoveredRect = new Rectangle(TileWidth / 2, TileHeight / 2, 1, 1);

                //first blit, then edit the rect sizes
                Color[] frameTex = BaseSheet.GetData(baseTexture, xx * TileWidth + imgCoveredRect.X, yy * TileHeight + imgCoveredRect.Y, imgCoveredRect.Width, imgCoveredRect.Height);
                //the final tile size must be able to contain this image
                maxWidth = Math.Max(maxWidth, imgCoveredRect.Width);
                maxHeight = Math.Max(maxHeight, imgCoveredRect.Height);

                OffsetData offsets = OffsetData[kk];

                //get the farthest that the offsets can cover, relative to the image
                Rectangle offsetCoveredRect = offsets.GetCoveredRect();
                Rectangle centeredOffsetRect = centerBounds(offsetCoveredRect);

                //the final tile size must be able to contain the offsets
                maxWidth = Math.Max(maxWidth, centeredOffsetRect.Width);
                maxHeight = Math.Max(maxHeight, centeredOffsetRect.Height);

                frames.Add((frameTex, imgCoveredRect, offsets));
            }

            //make frame sizes even
            maxWidth = roundUpToMult(maxWidth, 2);
            maxHeight = roundUpToMult(maxHeight, 2);

            CharAnimFrame[] frameMap = new CharAnimFrame[frames.Count];
            List<(Color[] img, Rectangle rect, OffsetData offsets)> finalFrames = new List<(Color[], Rectangle, OffsetData)>();
            mapDuplicates(frames, finalFrames, frameMap, false);

            int maxSize = (int)Math.Ceiling(Math.Sqrt(finalFrames.Count));
            Point texSize = new Point(maxSize * maxWidth, maxSize * maxHeight);
            Color[] texColors = new Color[texSize.X * texSize.Y];

            List<OffsetData> offsetData = new List<OffsetData>();
            for (int ii = 0; ii < finalFrames.Count; ii++)
            {
                int diffX = maxWidth / 2 - finalFrames[ii].rect.Width / 2;
                int diffY = maxHeight / 2 - finalFrames[ii].rect.Height / 2;
                BaseSheet.Blit(finalFrames[ii].img, texColors, new Point(finalFrames[ii].rect.Width, finalFrames[ii].rect.Height), texSize,
                    new Point(maxWidth * (ii % maxSize) + diffX, maxHeight * (ii / maxSize) + diffY), SpriteEffects.None);
                OffsetData endData = finalFrames[ii].offsets;

                offsetData.Add(endData);
            }

            foreach(int key in AnimData.Keys)
            {
                CharAnimGroup group = AnimData[key];
                foreach (CharAnimSequence seq in group.Sequences)
                {
                    foreach (CharAnimFrame frame in seq.Frames)
                    {
                        Loc frameFrom = frame.Frame;
                        int fromIndex = frameFrom.Y * TotalX + frameFrom.X;
                        CharAnimFrame mapFrame = frameMap[fromIndex];
                        frame.Flip ^= mapFrame.Flip;
                        int finalIndex = mapFrame.Frame.X;
                        if (frames[fromIndex].rect.Width % 2 == 1 && mapFrame.Flip)
                        {
                            if (frame.Flip)
                                frame.Offset = frame.Offset + Loc.UnitX;
                            else
                                frame.Offset = frame.Offset - Loc.UnitX;
                        }
                        frame.Frame = new Loc(finalIndex % maxSize, finalIndex / maxSize);
                    }
                }
            }
            Texture2D tex = new Texture2D(device, maxSize * maxWidth, maxSize * maxHeight);
            tex.SetData<Color>(0, null, texColors, 0, texColors.Length);

            SetTileTexture(tex, maxWidth, maxHeight);
            OffsetData = offsetData;
        }

        private static Rectangle addToBounds(Rectangle frame, Loc add)
        {
            return new Rectangle(frame.X + add.X, frame.Y + add.Y, frame.Width, frame.Height);
        }
        private static Rectangle combineExtents(Rectangle frame1, Rectangle frame2)
        {
            int startX = Math.Min(frame1.X, frame2.X);
            int startY = Math.Min(frame1.Y, frame2.Y);
            int endX = Math.Max(frame1.Right, frame2.Right);
            int endY = Math.Max(frame1.Bottom, frame2.Bottom);
            return new Rectangle(startX, startY, endX - startX, endY - startY);
        }

        private static Rectangle mirrorRect(Rectangle rect)
        {
            return new Rectangle(-rect.Right, rect.Y, rect.Width, rect.Height);
        }

        private static Rectangle centerBounds(Rectangle rect)
        {
            int minX = Math.Min(rect.X, -rect.Right);
            int minY = Math.Min(rect.Y, -rect.Bottom);

            int maxX = Math.Max(-rect.X, rect.Right);
            int maxY = Math.Max(-rect.Y, rect.Bottom);
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        private static int roundUpToMult(int inInt, int inMult)
        {
            int subInt = inInt - 1;
            int div = subInt / inMult;
            return (div + 1) * inMult;
        }

        private static Rectangle roundUpBox(Rectangle minBox)
        {
            int newWidth = roundUpToMult(minBox.Width, 8);
            int newHeight = roundUpToMult(minBox.Height, 8);
            int startX = minBox.X + (minBox.Width - newWidth) / 2;
            int startY = minBox.Y + (minBox.Height - newHeight) / 2;
            return new Rectangle(startX, startY, newWidth, newHeight);
        }

        private static void setOffsetsToRGB(Color[] colors, int imgWidth, Loc loc, Color newCol)
        {
            Color oldCol = colors[loc.Y * imgWidth + loc.X];
            colors[loc.Y * imgWidth + loc.X] = new Color(Math.Max(oldCol.R, newCol.R), Math.Max(oldCol.G, newCol.G), Math.Max(oldCol.B, newCol.B), 255);
        }

        public static void Export(CharSheet sheet, string baseDirectory, bool singleFrames)
        {
            if (singleFrames)
            {
                using (Stream stream = new FileStream(baseDirectory + "Anim.png", FileMode.Create, FileAccess.Write, FileShare.None))
                    ExportTex(stream, sheet.baseTexture);

                Point imgSize = new Point(sheet.baseTexture.Width, sheet.baseTexture.Height);
                Color[] particleColors = new Color[imgSize.X * imgSize.Y];

                Loc tileSize = new Loc(sheet.TileWidth, sheet.TileHeight);
                for (int ii = 0; ii < sheet.OffsetData.Count; ii++)
                {
                    OffsetData offsets = sheet.OffsetData[ii];
                    Loc tilePos = new Loc(ii % sheet.TotalX, ii / sheet.TotalX) * tileSize;
                    tilePos = tilePos + tileSize / 2;
                    setOffsetsToRGB(particleColors, imgSize.X, tilePos + offsets.Center, new Color(0, 255, 0, 255));
                    setOffsetsToRGB(particleColors, imgSize.X, tilePos + offsets.Head, new Color(0, 0, 0, 255));
                    setOffsetsToRGB(particleColors, imgSize.X, tilePos + offsets.LeftHand, new Color(255, 0, 0, 255));
                    setOffsetsToRGB(particleColors, imgSize.X, tilePos + offsets.RightHand, new Color(0, 0, 255, 255));
                }

                exportColors(baseDirectory + "Offsets.png", particleColors, imgSize);

                XmlDocument doc = new XmlDocument();
                XmlNode configNode = doc.CreateXmlDeclaration("1.0", null, null);
                doc.AppendChild(configNode);


                XmlNode docNode = doc.CreateElement("AnimData");
                docNode.AppendInnerTextChild(doc, "FrameWidth", sheet.TileWidth.ToString());
                docNode.AppendInnerTextChild(doc, "FrameHeight", sheet.TileHeight.ToString());
                docNode.AppendInnerTextChild(doc, "ShadowSize", sheet.ShadowSize.ToString());


                XmlNode animsNode = doc.CreateElement("Anims");

                foreach (int key in sheet.AnimData.Keys)
                {
                    if (key == 0)
                        continue;

                    CharAnimGroup group = sheet.AnimData[key];
                    XmlNode animGroupNode = doc.CreateElement("Anim");

                    animGroupNode.AppendInnerTextChild(doc, "Name", GraphicsManager.Actions[key].Name);
                    if (group.InternalIndex > -1)
                        animGroupNode.AppendInnerTextChild(doc, "Index", group.InternalIndex.ToString());
                    if (group.CopyOf > -1)
                        animGroupNode.AppendInnerTextChild(doc, "CopyOf", GraphicsManager.Actions[group.CopyOf].Name);
                    else
                    {
                        if (group.RushFrame > -1)
                            animGroupNode.AppendInnerTextChild(doc, "RushFrame", group.RushFrame.ToString());
                        if (group.HitFrame > -1)
                            animGroupNode.AppendInnerTextChild(doc, "HitFrame", group.HitFrame.ToString());
                        if (group.ReturnFrame > -1)
                            animGroupNode.AppendInnerTextChild(doc, "ReturnFrame", group.ReturnFrame.ToString());

                        XmlNode sequencesNode = doc.CreateElement("Sequences");
                        foreach (CharAnimSequence animSequence in group.Sequences)
                        {
                            XmlNode sequenceNode = doc.CreateElement("AnimSequence");

                            int duration = 0;
                            foreach (CharAnimFrame sequenceFrame in animSequence.Frames)
                            {
                                XmlNode frame = doc.CreateElement("AnimFrame");

                                int frameIndex = sequenceFrame.Frame.Y * sheet.TotalX + sequenceFrame.Frame.X;
                                frame.AppendInnerTextChild(doc, "FrameIndex", frameIndex.ToString());

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

                                sequenceNode.AppendChild(frame);
                            }

                            sequencesNode.AppendChild(sequenceNode);
                        }

                        animGroupNode.AppendChild(sequencesNode);
                    }
                    animsNode.AppendChild(animGroupNode);
                }
                docNode.AppendChild(animsNode);

                doc.AppendChild(docNode);

                doc.Save(baseDirectory + "FrameData.xml");
            }
            else
            {
                // calculate the max bounds of each animation
                // this is relative to the center of the frame
                List<(Rectangle crop_rect, Rectangle frame_rect, Color[] colors)> frames_data = new List<(Rectangle, Rectangle, Color[])>();
                // use offsets because they conveniently give the actual number of unique frames
                for (int ii = 0; ii < sheet.OffsetData.Count; ii++)
                {
                    Loc rectStart = new Loc(ii % sheet.TotalX, ii / sheet.TotalX);
                    Rectangle crop_rect = GetCoveredRect(sheet.baseTexture, new Rectangle(rectStart.X * sheet.TileWidth, rectStart.Y * sheet.TileHeight, sheet.TileWidth, sheet.TileHeight));
                    Rectangle source_rect = new Rectangle(rectStart.X * sheet.TileWidth + crop_rect.X, rectStart.Y * sheet.TileHeight + crop_rect.Y,
                        crop_rect.Width, crop_rect.Height);
                    Color[] frame_colors = BaseSheet.GetData(sheet, source_rect.X, source_rect.Y, source_rect.Width, source_rect.Height);

                    Rectangle frame_rect = addToBounds(crop_rect, new Loc(-sheet.TileWidth / 2, -sheet.TileHeight / 2));
                    frame_rect = combineExtents(frame_rect, sheet.OffsetData[ii].GetCoveredRect());
                    frames_data.Add((crop_rect, frame_rect, frame_colors));
                }

                Dictionary<int, Rectangle> groupBounds = new Dictionary<int, Rectangle>();
                Rectangle shadow_rect = new Rectangle(-GraphicsManager.MarkerShadow.Width / 2, -GraphicsManager.MarkerShadow.Height / 2, GraphicsManager.MarkerShadow.Width, GraphicsManager.MarkerShadow.Height);
                Rectangle shadow_rect_tight = GraphicsManager.MarkerShadow.GetCoveredRect(new Rectangle(0, 0, GraphicsManager.MarkerShadow.Width, GraphicsManager.MarkerShadow.Height));
                Color[] markerColors = BaseSheet.GetData(GraphicsManager.MarkerShadow, shadow_rect_tight.X, shadow_rect_tight.Y, shadow_rect_tight.Width, shadow_rect_tight.Height);

                // get max bounds for all animations
                foreach (int key in sheet.AnimData.Keys)
                {
                    if (key == 0)
                        continue;
                    if (sheet.AnimData[key].CopyOf > -1)
                        continue;

                    Rectangle maxBounds = new Rectangle(10000, 10000, -20000, -20000);
                    foreach (CharAnimSequence sequence in sheet.AnimData[key].Sequences)
                    {
                        foreach (CharAnimFrame frame in sequence.Frames)
                        {
                            Rectangle frame_rect = frames_data[frame.Frame.X + frame.Frame.Y * sheet.TotalX].frame_rect;
                            if (frame.Flip)
                                frame_rect = mirrorRect(frame_rect);
                            frame_rect = addToBounds(frame_rect, frame.Offset);
                            maxBounds = combineExtents(maxBounds, frame_rect);
                            Rectangle shadowBounds = addToBounds(shadow_rect_tight, new Loc(shadow_rect.X, shadow_rect.Y));
                            shadowBounds = addToBounds(shadowBounds, frame.ShadowOffset);
                            maxBounds = combineExtents(maxBounds, shadowBounds);
                        }
                        // round up to nearest x8
                        maxBounds = centerBounds(maxBounds);
                        maxBounds = roundUpBox(maxBounds);
                        groupBounds[key] = maxBounds;
                    }
                }


                foreach (int key in sheet.AnimData.Keys)
                {
                    if (key == 0)
                        continue;

                    if (sheet.AnimData[key].CopyOf > -1)
                        continue;

                    Rectangle maxBounds = groupBounds[key];
                    int framesPerAnim = sheet.AnimData[key].Sequences[0].Frames.Count;

                    Point imgSize = new Point(maxBounds.Width * framesPerAnim, maxBounds.Height * sheet.AnimData[key].Sequences.Count);
                    Color[] animColors = new Color[imgSize.X * imgSize.Y];
                    Color[] particleColors = new Color[imgSize.X * imgSize.Y];
                    Color[] shadowColors = new Color[imgSize.X * imgSize.Y];

                    for (int ii = 0; ii < DirExt.DIR8_COUNT; ii++)
                    {
                        int sheetIndex = (DirExt.DIR8_COUNT - ii) % DirExt.DIR8_COUNT;
                        if (sheetIndex >= sheet.AnimData[key].Sequences.Count)
                            continue;
                        CharAnimSequence sequence = sheet.AnimData[key].Sequences[ii];
                        for (int jj = 0; jj < sequence.Frames.Count; jj++)
                        {
                            CharAnimFrame frame = sequence.Frames[jj];
                            //get the absolute position of the rectangle to blit from
                            Rectangle crop_rect = frames_data[frame.Frame.X + frame.Frame.Y * sheet.TotalX].crop_rect;
                            Rectangle source_rect = new Rectangle(frame.Frame.X * sheet.TileWidth + crop_rect.X, frame.Frame.Y * sheet.TileHeight + crop_rect.Y,
                                crop_rect.Width, crop_rect.Height);
                            Color[] frameColors = frames_data[frame.Frame.X + frame.Frame.Y * sheet.TotalX].colors;

                            Loc tilePos = new Loc(jj * maxBounds.Width, sheetIndex * maxBounds.Height);
                            //add half of the maxbounds to get the center
                            Loc centerPos = tilePos + new Loc(maxBounds.Width, maxBounds.Height) / 2;
                            //add the anim frame's offset to get center of actual sprite
                            Loc framePos = centerPos + frame.Offset;
                            //subtract half the atlas tile size for the draw position based on the atlas
                            Loc atlasPastePos = framePos - new Loc(sheet.TileWidth, sheet.TileHeight) / 2;
                            //add frame rect start to the source value to get the exact draw position on the anim sheet
                            Rectangle crop_dest_rect = crop_rect;
                            if (frame.Flip)
                            {
                                crop_dest_rect = mirrorRect(crop_dest_rect);
                                crop_dest_rect.X += sheet.TileWidth;
                            }
                            Loc pastePos = atlasPastePos + new Loc(crop_dest_rect.X, crop_dest_rect.Y);

                            BaseSheet.Blit(frameColors, animColors, new Point(source_rect.Width, source_rect.Height), imgSize,
                                new Point(pastePos.X, pastePos.Y), frame.Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

                            OffsetData offsets = sheet.OffsetData[frame.Frame.X + frame.Frame.Y * sheet.TotalX];
                            setOffsetsToRGB(particleColors, imgSize.X, framePos + (frame.Flip ? offsets.CenterFlip : offsets.Center), new Color(0, 255, 0, 255));
                            setOffsetsToRGB(particleColors, imgSize.X, framePos + (frame.Flip ? offsets.HeadFlip : offsets.Head), new Color(0, 0, 0, 255));
                            setOffsetsToRGB(particleColors, imgSize.X, framePos + (frame.Flip ? offsets.LeftHandFlip : offsets.LeftHand), new Color(255, 0, 0, 255));
                            setOffsetsToRGB(particleColors, imgSize.X, framePos + (frame.Flip ? offsets.RightHandFlip : offsets.RightHand), new Color(0, 0, 255, 255));

                            Loc shadowPos = centerPos + frame.ShadowOffset;
                            shadowPos = shadowPos + new Loc(shadow_rect.X, shadow_rect.Y);
                            shadowPos = shadowPos + new Loc(shadow_rect_tight.X, shadow_rect_tight.Y);

                            BaseSheet.Blit(markerColors, shadowColors, new Point(shadow_rect_tight.Width, shadow_rect_tight.Height), imgSize,
                                new Point(shadowPos.X, shadowPos.Y), SpriteEffects.None);
                        }
                    }
                    string name = GraphicsManager.Actions[key].Name;

                    exportColors(baseDirectory + name + "-Anim.png", animColors, imgSize);
                    exportColors(baseDirectory + name + "-Offsets.png", particleColors, imgSize);
                    exportColors(baseDirectory + name + "-Shadow.png", shadowColors, imgSize);

                }

                XmlDocument doc = new XmlDocument();
                XmlNode configNode = doc.CreateXmlDeclaration("1.0", null, null);
                doc.AppendChild(configNode);

                XmlNode docNode = doc.CreateElement("AnimData");
                docNode.AppendInnerTextChild(doc, "ShadowSize", sheet.ShadowSize.ToString());

                XmlNode animsNode = doc.CreateElement("Anims");

                foreach (int key in sheet.AnimData.Keys)
                {
                    if (key == 0)
                        continue;

                    CharAnimGroup group = sheet.AnimData[key];
                    XmlNode animGroupNode = doc.CreateElement("Anim");

                    animGroupNode.AppendInnerTextChild(doc, "Name", GraphicsManager.Actions[key].Name);
                    if (group.InternalIndex > -1)
                        animGroupNode.AppendInnerTextChild(doc, "Index", group.InternalIndex.ToString());
                    if (group.CopyOf > -1)
                        animGroupNode.AppendInnerTextChild(doc, "CopyOf", GraphicsManager.Actions[group.CopyOf].Name);
                    else
                    {
                        animGroupNode.AppendInnerTextChild(doc, "FrameWidth", groupBounds[key].Width.ToString());
                        animGroupNode.AppendInnerTextChild(doc, "FrameHeight", groupBounds[key].Height.ToString());

                        if (group.RushFrame > -1)
                            animGroupNode.AppendInnerTextChild(doc, "RushFrame", group.RushFrame.ToString());
                        if (group.HitFrame > -1)
                            animGroupNode.AppendInnerTextChild(doc, "HitFrame", group.HitFrame.ToString());
                        if (group.ReturnFrame > -1)
                            animGroupNode.AppendInnerTextChild(doc, "ReturnFrame", group.ReturnFrame.ToString());

                        XmlNode durations = doc.CreateElement("Durations");
                        int curTime = 0;
                        foreach (CharAnimFrame frame in group.Sequences[0].Frames)
                        {
                            int dur = frame.EndTime - curTime;
                            durations.AppendInnerTextChild(doc, "Duration", dur.ToString());
                            curTime = frame.EndTime;
                        }
                        animGroupNode.AppendChild(durations);
                    }
                    animsNode.AppendChild(animGroupNode);
                }
                docNode.AppendChild(animsNode);

                doc.AppendChild(docNode);

                doc.Save(baseDirectory + "AnimData.xml");
            }
        }

        private static void exportColors(string fileName, Color[] colors, Point imgSize)
        {
            Texture2D animImg = new Texture2D(device, imgSize.X, imgSize.Y);
            animImg.SetData<Color>(0, null, colors, 0, colors.Length);
            using (Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                ExportTex(stream, animImg);
            animImg.Dispose();
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
                Loc center = new Loc(reader.ReadInt32(), reader.ReadInt32());
                Loc head = new Loc(reader.ReadInt32(), reader.ReadInt32());
                Loc leftHand = new Loc(reader.ReadInt32(), reader.ReadInt32());
                Loc rightHand = new Loc(reader.ReadInt32(), reader.ReadInt32());
                OffsetData offset = new OffsetData(center, head, leftHand, rightHand);
                offsetData.Add(offset);
            }

            Dictionary<int, CharAnimGroup> animData = new Dictionary<int, CharAnimGroup>();

            int keyCount = reader.ReadInt32();
            for (int ii = 0; ii < keyCount; ii++)
            {
                int frameType = reader.ReadInt32();

                CharAnimGroup group = new CharAnimGroup();
                group.InternalIndex = reader.ReadInt32();
                group.CopyOf = reader.ReadInt32();
                if (group.CopyOf > -1)
                {
                    animData[frameType] = group;
                    continue;
                }
                group.RushFrame = reader.ReadInt32();
                group.HitFrame = reader.ReadInt32();
                group.ReturnFrame = reader.ReadInt32();

                int sequenceCount = reader.ReadInt32();
                for (int jj = 0; jj < sequenceCount; jj++)
                {
                    CharAnimSequence sequence = new CharAnimSequence();
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
                        sequence.Frames.Add(frame);
                    }
                    group.Sequences.Add(sequence);
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
            writer.Write(OffsetData.Count);
            for (int ii = 0; ii < OffsetData.Count; ii++)
            {
                writer.Write(OffsetData[ii].Center.X);
                writer.Write(OffsetData[ii].Center.Y);
                writer.Write(OffsetData[ii].Head.X);
                writer.Write(OffsetData[ii].Head.Y);
                writer.Write(OffsetData[ii].LeftHand.X);
                writer.Write(OffsetData[ii].LeftHand.Y);
                writer.Write(OffsetData[ii].RightHand.X);
                writer.Write(OffsetData[ii].RightHand.Y);
            }

            writer.Write(AnimData.Keys.Count);
            foreach (int frameType in AnimData.Keys)
            {
                writer.Write(frameType);
                CharAnimGroup animGroup = AnimData[frameType];

                writer.Write(animGroup.InternalIndex);
                writer.Write(animGroup.CopyOf);
                if (animGroup.CopyOf > -1)
                    continue;

                writer.Write(animGroup.RushFrame);
                writer.Write(animGroup.HitFrame);
                writer.Write(animGroup.ReturnFrame);

                writer.Write(animGroup.Sequences.Count);
                for (int ii = 0; ii < animGroup.Sequences.Count; ii++)
                {
                    writer.Write(animGroup.Sequences[ii].Frames.Count);
                    int prevTime = 0;
                    for (int jj = 0; jj < animGroup.Sequences[ii].Frames.Count; jj++)
                    {
                        CharAnimFrame frame = animGroup.Sequences[ii].Frames[jj];
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
                CharAnimSequence seq = group.SeqAtDir(dir);
                int frameNum = frameMethod(seq.Frames);
                CharAnimFrame frame = seq.Frames[frameNum];
                int trueRush = group.RushFrame > -1 ? group.RushFrame : 0;
                DrawTile(spriteBatch, pos + AdjustOffset(type, group.RushFrame, frameNum, seq.Frames[trueRush].Offset, frame.Offset).ToVector2(), frame.Frame.X, frame.Frame.Y, color, frame.Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
            else
                DrawDefault(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, TileWidth, TileHeight));
        }
        public Loc GetActionPoint(int type, Dir8 dir, ActionPointType pointType, DetermineFrame frameMethod)
        {
            CharAnimGroup group = getReferencedAnim(type);
            if (group != null)
            {
                List<CharAnimFrame> frames = group.SeqAtDir(dir).Frames;
                int frameNum = frameMethod(frames);
                CharAnimFrame frame = frames[frameNum];
                int trueRush = group.RushFrame > -1 ? group.RushFrame : 0;
                if (pointType == ActionPointType.Shadow)
                    return AdjustOffset(type, group.RushFrame, frameNum, frames[trueRush].ShadowOffset, frame.ShadowOffset);

                OffsetData offset = OffsetData[frame.Frame.Y * TotalX + frame.Frame.X];
                Loc chosenLoc = Loc.Zero;
                switch (pointType)
                {
                    case ActionPointType.Center:
                        chosenLoc = frame.Flip ? offset.CenterFlip : offset.Center;
                        break;
                    case ActionPointType.Head:
                        chosenLoc = frame.Flip ? offset.HeadFlip : offset.Head;
                        break;
                    case ActionPointType.LeftHand:
                        chosenLoc = frame.Flip ? offset.LeftHandFlip : offset.LeftHand;
                        break;
                    case ActionPointType.RightHand:
                        chosenLoc = frame.Flip ? offset.RightHandFlip : offset.RightHand;
                        break;
                }
                return AdjustOffset(type, group.RushFrame, frameNum, frames[trueRush].Offset, frame.Offset) + chosenLoc;
            }
            return Loc.Zero;
        }

        public void DrawCharFrame(SpriteBatch spriteBatch, int type, Dir8 dir, Vector2 pos, int frameNum, Color color)
        {
            CharAnimGroup group = getReferencedAnim(type);
            if (group != null)
            {
                CharAnimSequence seq = group.SeqAtDir(dir);
                CharAnimFrame frame = seq.Frames[frameNum];
                int trueRush = group.RushFrame > -1 ? group.RushFrame : 0;
                DrawTile(spriteBatch, pos + AdjustOffset(type, group.RushFrame, frameNum, seq.Frames[trueRush].Offset, frame.Offset).ToVector2(), frame.Frame.X, frame.Frame.Y, color, frame.Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
            else
                DrawDefault(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, TileWidth, TileHeight));
        }

        private Loc AdjustOffset(int type, int rushFrame, int frameNum, Loc rushOffset, Loc offset)
        {
            if (GraphicsManager.Actions[type].IsDash)
            {
                if (frameNum > rushFrame)
                {
                    Loc diff = offset - rushOffset;
                    return rushOffset + (diff / 2);
                }
            }
            
            return offset;
        }

        public int GetCurrentFrame(int type, Dir8 dir, DetermineFrame frameMethod)
        {
            CharAnimGroup group = getReferencedAnim(type);
            if (group != null)
                return frameMethod(group.SeqAtDir(dir).Frames);
            else
                return 0;
        }

        public bool HasOwnAnim(int type)
        {
            CharAnimGroup group;
            if (!AnimData.TryGetValue(type, out group))
                return false;
            return group.CopyOf == -1;
        }

        public bool IsAnimCopied(int type)
        {
            foreach (int otherType in AnimData.Keys)
            {
                CharAnimGroup group = AnimData[otherType];
                if (group.CopyOf == type)
                    return true;
            }
            return false;
        }

        private CharAnimGroup getReferencedAnim(int type)
        {
            int fallbackIndex = -1;
            CharFrameType actionData = GraphicsManager.Actions[type];
            CharAnimGroup group;
            while (!AnimData.TryGetValue(type, out group))
            {
                fallbackIndex++;
                if (fallbackIndex < actionData.Fallbacks.Count)
                    type = actionData.Fallbacks[fallbackIndex];
                else
                    return null;
            }

            while (group.CopyOf > -1)
                group = AnimData[group.CopyOf];
            return group;
        }

        public int GetTotalTime(int type, Dir8 dir)
        {
            CharAnimGroup group = getReferencedAnim(type);
            if (group != null)
            {
                CharAnimSequence frameList = group.SeqAtDir(dir);
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
                CharAnimSequence frameList = group.SeqAtDir(dir);
                if (group.ReturnFrame > -1)
                    return frameList.Frames[group.ReturnFrame].EndTime;
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
                CharAnimSequence frameList = group.SeqAtDir(dir);
                if (group.HitFrame > -1)
                    return frameList.Frames[group.HitFrame].EndTime;
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
                CharAnimSequence frameList = group.SeqAtDir(dir);
                if (group.RushFrame > -1)
                    return frameList.Frames[group.RushFrame].EndTime;
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
