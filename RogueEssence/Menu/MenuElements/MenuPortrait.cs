using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class MenuPortrait : IMenuElement
    {
        public Loc Loc;
        public MonsterID Speaker;
        public EmoteStyle SpeakerEmotion;

        public MenuPortrait(Loc loc, MonsterID speaker) : this(loc, speaker, new EmoteStyle())
        { }

        public MenuPortrait(Loc loc, MonsterID speaker, EmoteStyle emote)
        {
            Loc = loc;
            Speaker = speaker;
            SpeakerEmotion = emote;
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            PortraitSheet portrait = GraphicsManager.GetPortrait(Speaker.ToCharID());

            Loc drawLoc = Loc + offset;

            portrait.DrawPortrait(spriteBatch, new Vector2(drawLoc.X, drawLoc.Y), SpeakerEmotion);

        }
    }
}
