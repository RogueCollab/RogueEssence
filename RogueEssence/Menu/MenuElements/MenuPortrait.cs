using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class MenuPortrait : IMenuElement
    {
        public string Label { get; set; }
        public Loc Loc;
        public MonsterID Speaker;
        public EmoteStyle SpeakerEmotion;

        public bool HasLabel()
        {
            return !string.IsNullOrEmpty(Label);
        }
        public bool LabelContains(string substr)
        {
            return HasLabel() && Label.Contains(substr);
        }

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

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            PortraitSheet portrait = GraphicsManager.GetPortrait(Speaker.ToCharID());

            Loc drawLoc = Loc + offset;

            portrait.DrawPortrait(spriteBatch, new Vector2(drawLoc.X, drawLoc.Y), SpeakerEmotion);

        }
    }
}
