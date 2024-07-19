using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class MenuPortrait : BaseMenuElement
    {
        public Loc Loc;
        public MonsterID Speaker;
        public EmoteStyle SpeakerEmotion;

        public MenuPortrait(Loc loc, MonsterID speaker) : this("", loc, speaker, new EmoteStyle())
        { }

        public MenuPortrait(string label, Loc loc, MonsterID speaker) : this(label, loc, speaker, new EmoteStyle())
        { }

        public MenuPortrait(Loc loc, MonsterID speaker, EmoteStyle emote) : this("", loc, speaker, emote)
        { }

        public MenuPortrait(string label, Loc loc, MonsterID speaker, EmoteStyle emote)
        {
            Label = label;
            Loc = loc;
            Speaker = speaker;
            SpeakerEmotion = emote;
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            PortraitSheet portrait = GraphicsManager.GetPortrait(Speaker.ToCharID());

            Loc drawLoc = Loc + offset;

            portrait.DrawPortrait(spriteBatch, new Vector2(drawLoc.X, drawLoc.Y), SpeakerEmotion);

        }
    }
}
