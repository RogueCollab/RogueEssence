using System;
using System.Collections.Generic;
using System.IO;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using RectPacker;

namespace RogueEssence.Content
{
    public struct GlyphData
    {
        public int RectIdx;
        public bool Colorless;

        public GlyphData(int rectIdx, bool colorless)
        {
            RectIdx = rectIdx;
            Colorless = colorless;
        }
    }

    public class FontSheet : SpriteSheet
    {
        const string NON_STARTERS = "";//lines cannot start with
        const string NON_ENDERS = "";//lines cannot end with

        //Spacing variables
        public int SpaceWidth { get; private set; }
        public int CharHeight { get; private set; }
        public int LineSpace { get; private set; }
        public int CharSpace { get; private set; }

        private Dictionary<int, GlyphData> charMap;

        public FontSheet(Texture2D tex, Rectangle[] rects, int space, int charHeight, int charSpace, int charLine, Dictionary<int, GlyphData> charMap)
            :base(tex, rects)
        {

            SpaceWidth = space;
            CharHeight = charHeight;
            CharSpace = charSpace;
            LineSpace = charLine;

            this.charMap = charMap;
        }


        //frompath (import) has two varieties: they will take
        // - take a folder containing all elements, each with a char number
        // - take a simple png
        //fromstream (load) will take the png and all the rectangles, and the char maps from stream
        //save will save as .font


        public static new FontSheet Import(string path)
        {
            //get all extra stat from an XML
            if (File.Exists(path + "FontData.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path + "FontData.xml");
                int spaceWidth = Convert.ToInt32(doc.SelectSingleNode("FontData/SpaceWidth").InnerText);
                int charHeight = Convert.ToInt32(doc.SelectSingleNode("FontData/CharHeight").InnerText);
                int charSpace = Convert.ToInt32(doc.SelectSingleNode("FontData/CharSpace").InnerText);
                int lineSpace = Convert.ToInt32(doc.SelectSingleNode("FontData/LineSpace").InnerText);

                HashSet<int> colorlessGlyphs = new HashSet<int>();
                XmlNode glyphNodes = doc.SelectSingleNode("FontData/Colorless");
                foreach (XmlNode glyphNode in glyphNodes.SelectNodes("Glyph"))
                {
                    int glyphIdx = Convert.ToInt32(glyphNode.InnerText, 16);
                    colorlessGlyphs.Add(glyphIdx);
                }

                string[] pngs = Directory.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly);
                List<ImageInfo> sheets = new List<ImageInfo>();
                foreach (string dir in pngs)
                {
                    int png = Convert.ToInt32(Path.GetFileNameWithoutExtension(dir), 16);
                    Texture2D newSheet = null;
                    using (FileStream fileStream = new FileStream(dir, FileMode.Open, FileAccess.Read, FileShare.Read))
                        newSheet = ImportTex(fileStream);

                    sheets.Add(new ImageInfo(png, newSheet));
                }
                if (sheets.Count == 0)
                    return null;

                Canvas canvas = new Canvas();
                canvas.SetCanvasDimensions(Canvas.INFINITE_SIZE, Canvas.INFINITE_SIZE);
                OptimalMapper mapper = new OptimalMapper(canvas, 0.975f, 10);
                Atlas atlas = mapper.Mapping(sheets);

                Rectangle[] rects = new Rectangle[sheets.Count];
                Dictionary<int, GlyphData> chars = new Dictionary<int, GlyphData>();
                Texture2D tex = new Texture2D(device, atlas.Width, atlas.Height);
                for (int ii = 0; ii < atlas.MappedImages.Count; ii++)
                {
                    MappedImageInfo info = atlas.MappedImages[ii];
                    BaseSheet.Blit(info.ImageInfo.Texture, tex, 0, 0, info.ImageInfo.Width, info.ImageInfo.Height, info.X, info.Y);
                    rects[ii] = new Rectangle(info.X, info.Y, info.ImageInfo.Width, info.ImageInfo.Height);
                    chars[info.ImageInfo.ID] = new GlyphData(ii, colorlessGlyphs.Contains(info.ImageInfo.ID));
                    info.ImageInfo.Texture.Dispose();
                }

                return new FontSheet(tex, rects, spaceWidth, charHeight, charSpace, lineSpace, chars);
            }
            else
                throw new Exception("Error finding XML file in " + path + ".");
        }

        public static new FontSheet Load(BinaryReader reader)
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
            Dictionary<int, GlyphData> chars = new Dictionary<int, GlyphData>();
            for (int ii = 0; ii < rectCount; ii++)
                rects[ii] = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

            int space = reader.ReadInt32();
            int charHeight = reader.ReadInt32();
            int charSpace = reader.ReadInt32();
            int charLine = reader.ReadInt32();

            for (int ii = 0; ii < rectCount; ii++)
            {
                int charCode = reader.ReadInt32();
                int index = reader.ReadInt32();
                bool colorless = reader.ReadBoolean();
                chars.Add(charCode, new GlyphData(index, colorless));
            }
            return new FontSheet(tex, rects, space, charHeight, charSpace, charLine, chars);
            
        }

        public static new FontSheet LoadError()
        {
            Rectangle[] rects = new Rectangle[0];
            Dictionary<int, GlyphData> chars = new Dictionary<int, GlyphData>();
            return new FontSheet(defaultTex, rects, 8, 8, 1, 1, chars);
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(SpaceWidth);
            writer.Write(CharHeight);
            writer.Write(CharSpace);
            writer.Write(LineSpace);

            foreach(int key in charMap.Keys)
            {
                writer.Write(key);
                writer.Write(charMap[key].RectIdx);
                writer.Write(charMap[key].Colorless);
            }
        }

        public void DrawText(SpriteBatch spriteBatch, int x, int y, string text, Rectangle? area, DirV vOrigin, DirH hOrigin)
        {
            DrawText(spriteBatch, x, y, text, area, vOrigin, hOrigin, Color.White);
        }

        public void DrawText(SpriteBatch spriteBatch, int x, int y, string text, Rectangle? area, DirV vOrigin, DirH hOrigin, Color color)
        {
            DrawText(spriteBatch, x, y, text, area, vOrigin, hOrigin, color, 0, text.Length);
        }

        public void DrawText(SpriteBatch spriteBatch, int x, int y, string text, Rectangle? area, DirV vOrigin, DirH hOrigin, Color color, int start, int length)
        {
            if (String.IsNullOrWhiteSpace(text))
                return;

            int lineHeight = CharHeight + LineSpace;

            //Draw positions
            int dX = x;
            int dY = y;

            //If the text needs to be aligned
            if (area == null)
                area = new Rectangle(x, y, 0, 0);

            //Set origin (vertical)
            switch (vOrigin)
            {
                case DirV.Up:
                    dY = area.Value.Top;
                    break;
                case DirV.Down:
                    dY = area.Value.Bottom - StringHeight(text, LineSpace);
                    break;
                default:
                    dY = (area.Value.Top + area.Value.Bottom - StringHeight(text, LineSpace)) / 2;
                    break;
            }
            //Set origin (horizontal)
            switch (hOrigin)
            {
                case DirH.Left:
                    dX = area.Value.Left;
                    break;
                case DirH.Right:
                    dX = area.Value.Right - SubstringWidth(text);
                    break;
                default:
                    dX = (area.Value.Left + area.Value.Right - SubstringWidth(text)) / 2;
                    break;
            }
            int startDX = dX;

            //Go through string
            for (int ii = 0; ii < start + length; ii++)
            {
                //Space
                if (text[ii] == ' ' || text[ii] == '　')
                    dX += SpaceWidth;
                else if (text[ii] == '\u2060' || text[ii] == '\u202F')
                    dX++;
                //Newline
                else if (text[ii] == '\n')
                {
                    //Handle horizontal alignment
                    int targetX = x;
                    switch (hOrigin)
                    {
                        case DirH.Left:
                            targetX = area.Value.Left;
                            break;
                        case DirH.Right:
                            targetX = area.Value.Right - SubstringWidth(text[ii + 1].ToString());
                            break;
                        default:
                            targetX = (area.Value.Left + area.Value.Right - SubstringWidth(text.Substring(ii + 1).ToString())) / 2;
                            break;
                    }
                    dY += lineHeight;
                    dX = targetX;
                }
                //Character
                else
                {
                    int texture_char = charMap[0].RectIdx;//default null
                    Color curColor = color;

                    if (dX > startDX)
                        dX += CharSpace;

                    //Get corresponding texture
                    if (charMap.ContainsKey(text[ii]))
                    {
                        texture_char = charMap[text[ii]].RectIdx;
                        if (charMap[text[ii]].Colorless)
                            curColor = Color.White;
                    }

                    Rectangle char_rect = spriteRects[texture_char];
                    if (ii >= start)
                        DrawSprite(spriteBatch, new Vector2(dX, dY), texture_char, curColor);

                    //Move over
                    dX += char_rect.Width;
                }
            }

        }


        Rectangle getStringArea(string text, int lineSpace)
        {
            //Initialize area
            int subWidth = 0;
            Rectangle area = new Rectangle(0, 0, (int)subWidth, (int)CharHeight);
            //Go through string
            for (int ii = 0; ii < text.Length; ++ii)
            {
                //Space
                if (text[ii] == ' ' || text[ii] == '　')
                    subWidth += SpaceWidth;
                else if (text[ii] == '\u2060' || text[ii] == '\u202F')
                    subWidth++;
                //Newline
                else if (text[ii] == '\n')
                {
                    //Add another line
                    area.Height += CharHeight + LineSpace;
                    //Check for max width
                    if (subWidth > area.Width)
                    {
                        area.Width = (int)subWidth;
                        subWidth = 0;
                    }
                } //Character
                else
                {
                    if (subWidth > 0)
                        subWidth += CharSpace;
                    int texture_char = charMap[0].RectIdx;
                    if (charMap.ContainsKey(text[ii])) //Get texture
                        texture_char = charMap[text[ii]].RectIdx;

                    Rectangle char_rect = spriteRects[texture_char];
                    subWidth += char_rect.Width;
                }
            } //Check for max width
            if (subWidth > area.Width)
                area.Width = (int)subWidth;
            return area;
        }

        public int StringHeight(string thisString, int lineSpace)
        {
            if (String.IsNullOrEmpty(thisString))
                return 0;
            int height = CharHeight;
            //Go through string
            for (int ii = 0; ii < thisString.Length; ii++)
            {
                //Space
                if (thisString[ii] == '\n')
                    height += CharHeight + lineSpace;
            }
            return height;
        }

        public int SubstringWidth(string substring)
        {
            if (String.IsNullOrEmpty(substring))
                return 0;
            int subWidth = 0;
            //Go through string
            for (int ii = 0; ii < substring.Length && substring[ii] != '\n'; ii++)
            {
                //Space
                if (substring[ii] == ' ' || substring[ii] == '　')
                    subWidth += SpaceWidth;
                else if (substring[ii] == '\u2060' || substring[ii] == '\u202F')
                    subWidth++;
                //Character
                else
                {
                    if (subWidth > 0)
                        subWidth += CharSpace;
                    int texture_char = charMap[0].RectIdx;
                    if (charMap.ContainsKey(substring[ii])) //Get texture
                        texture_char = charMap[substring[ii]].RectIdx;

                    Rectangle char_rect = spriteRects[texture_char];
                    subWidth += char_rect.Width;
                }
            }
            return subWidth;
        }

        public string[] BreakIntoLines(string substring, int width)
        {
            return BreakIntoLines(substring, width, substring.Length);
        }
        public string[] BreakIntoLines(string substring, int width, int charIndex)
        {
            if (String.IsNullOrEmpty(substring))
                return null;
            int substr_width = 0;
            int width_since_breakable = 0;
            int line_start = 0;
            int last_breakable = 0;
            //Go through string
            List<string> lines = new List<string>();
            for (int ii = 0; ii < substring.Length; ii++)
            {
                //newline
                if (substring[ii] == '\n')
                {
                    lines.Add(substring.Substring(line_start, Math.Min(ii, charIndex) - line_start));
                    line_start = ii + 1;
                    if (line_start >= charIndex)
                        return lines.ToArray();
                    substr_width = 0;
                    last_breakable = ii+1;
                    width_since_breakable = 0;
                }//spaces
                else if (substring[ii] == ' ' || substring[ii] == '　')
                {
                    if (substr_width > 0)
                        substr_width += SpaceWidth;
                    else
                        line_start = ii + 1;

                    width_since_breakable = 0;
                    last_breakable = ii+1;
                }//word joining spaces
                else if (substring[ii] == '\u2060' || substring[ii] == '\u202F')
                {
                    if (substr_width > 0)
                        substr_width++;

                    width_since_breakable++;
                }
                //Character
                else
                {
                    if (charMap.ContainsKey(substring[ii]))
                    {
                        if (substr_width > 0)
                            substr_width += CharSpace;
                        //Get ASCII
                        int texture_char = charMap[substring[ii]].RectIdx;
                        Rectangle char_rect = spriteRects[texture_char];
                        substr_width += char_rect.Width;
                        if (substring[ii] == '-' || substring[ii] == '、' || substring[ii] == '。')
                        {
                            width_since_breakable = 0;
                            last_breakable = ii+1;
                        }
                        else
                        {
                            //CJK
                            bool isCJK = false;
                            if (substring[ii] >= 0x4E00 && substring[ii] < 0x9FFF)
                                isCJK = true;

                            if (isCJK)
                            {
                                last_breakable = ii;
                                width_since_breakable = char_rect.Width;
                            }
                            else
                            {
                                if (width_since_breakable > 0)
                                    width_since_breakable += CharSpace;
                                width_since_breakable += char_rect.Width;
                            }
                        }
                    }
                }

                if (substr_width >= width && last_breakable > line_start)
                {
                    lines.Add(substring.Substring(line_start, Math.Min(last_breakable, charIndex) - line_start));
                    line_start = last_breakable;
                    if (line_start >= charIndex)
                        return lines.ToArray();
                    substr_width = width_since_breakable;
                }
            }
            lines.Add(substring.Substring(line_start, Math.Min(substring.Length, charIndex) - line_start));
            return lines.ToArray();
        }

        public bool CanRenderChar(char glyph)
        {
            if (glyph == ' ' || glyph == '　')
                return true;
            else if (glyph == '\u2060' || glyph == '\u202F')
                return true;
            else if (glyph == '\n')
                return true;
            else
                return charMap.ContainsKey(glyph);
        }
    }
}
