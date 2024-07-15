using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class SpeakerPortrait : IMenuElement
    {
        public string Label { get; set; }
        public Loc Loc;
        public MonsterID Speaker;
        public EmoteStyle SpeakerEmotion;
        public bool Bordered;

        public bool HasLabel()
        {
            return !string.IsNullOrEmpty(Label);
        }
        public bool LabelContains(string substr)
        {
            return HasLabel() && Label.Contains(substr);
        }

        public SpeakerPortrait(MonsterID speaker, EmoteStyle emotion, Loc loc, bool bordered)
        {
            Speaker = speaker;
            SpeakerEmotion = emotion;
            Bordered = bordered;
            Loc = loc;
        }

        public static Loc DefaultLoc => new Loc(DialogueBox.SIDE_BUFFER,
            GraphicsManager.ScreenHeight - (16 + DialogueBox.TEXT_HEIGHT * DialogueBox.MAX_LINES + DialogueBox.VERT_PAD * 2) - 56);

        public SpeakerPortrait(SpeakerPortrait other, int emote)
        {
            Speaker = other.Speaker;
            SpeakerEmotion = other.SpeakerEmotion;
            SpeakerEmotion.Emote = emote;
            SpeakerEmotion.Reverse = other.SpeakerEmotion.Reverse;
            Bordered = other.Bordered;
            Loc = other.Loc;
        }
        
        //kind of like a menu, but not quite (uses borders)
        //draws the portrait

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            PortraitSheet portrait = GraphicsManager.GetPortrait(Speaker.ToCharID());

            Loc drawLoc = Loc + offset;

            if (!Bordered)
                portrait.DrawPortrait(spriteBatch, new Vector2(drawLoc.X, drawLoc.Y), SpeakerEmotion);
            else
            {
                int addX = 3 * MenuBase.BorderStyle;
                int addY = 3 * MenuBase.BorderFlash;

                TileSheet sheet = GraphicsManager.PicBorder;
                //pic
                portrait.DrawPortrait(spriteBatch, new Vector2(drawLoc.X + sheet.TileWidth, drawLoc.Y + sheet.TileHeight), SpeakerEmotion);

                //top-left
                sheet.DrawTile(spriteBatch, new Vector2(drawLoc.X, drawLoc.Y), addX, addY);
                //top-right
                sheet.DrawTile(spriteBatch, new Vector2(drawLoc.X + sheet.TileWidth + GraphicsManager.PortraitSize, drawLoc.Y), addX + 2, addY);
                //bottom-right
                sheet.DrawTile(spriteBatch, new Vector2(drawLoc.X + sheet.TileWidth + GraphicsManager.PortraitSize, drawLoc.Y + sheet.TileHeight + GraphicsManager.PortraitSize), addX + 2, addY + 2);
                //bottom-left
                sheet.DrawTile(spriteBatch, new Vector2(drawLoc.X, drawLoc.Y + sheet.TileHeight + GraphicsManager.PortraitSize), addX, addY + 2);

                //top
                sheet.DrawTile(spriteBatch, new Rectangle(drawLoc.X + sheet.TileWidth, drawLoc.Y, GraphicsManager.PortraitSize, sheet.TileHeight), addX + 1, addY, Color.White);

                //right
                sheet.DrawTile(spriteBatch, new Rectangle(drawLoc.X + sheet.TileWidth + GraphicsManager.PortraitSize, drawLoc.Y + sheet.TileHeight, sheet.TileWidth, GraphicsManager.PortraitSize), addX + 2, addY + 1, Color.White);

                //bottom
                sheet.DrawTile(spriteBatch, new Rectangle(drawLoc.X + sheet.TileWidth, drawLoc.Y + sheet.TileHeight + GraphicsManager.PortraitSize, GraphicsManager.PortraitSize, sheet.TileHeight), addX + 1, addY + 2, Color.White);

                //left
                sheet.DrawTile(spriteBatch, new Rectangle(drawLoc.X, drawLoc.Y + sheet.TileHeight, sheet.TileWidth, GraphicsManager.PortraitSize), addX, addY + 1, Color.White);
            }
        }
    }
}
