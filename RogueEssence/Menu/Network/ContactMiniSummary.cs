using RogueElements;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class ContactMiniSummary : SummaryMenu
    {
        MenuText Name;
        MenuText Stats;
        MenuText LastSeen;
        MenuText ID;
        SpeakerPortrait[] Portraits;

        public ContactMiniSummary(Rect bounds)
            : base(bounds)
        {
            Name = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + 2));
            Elements.Add(Name);
            Stats = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE + 2));
            Elements.Add(Stats);
            LastSeen = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + 2));
            Elements.Add(LastSeen);
            ID = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + 2));
            Elements.Add(ID);
            Portraits = new SpeakerPortrait[0];
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Name;
            yield return Stats;

            yield return LastSeen;
            yield return ID;

            foreach (SpeakerPortrait portrait in Portraits)
                yield return portrait;
        }

        public void SetContact(string name, string stats, string lastSeen, string id, ProfilePic[] portraits)
        {
            Name.SetText(name);
            Stats.SetText(stats);
            LastSeen.SetText(lastSeen);
            ID.SetText(id);
            Portraits = new SpeakerPortrait[portraits.Length];
            for (int ii = 0; ii < portraits.Length; ii++)
                Portraits[ii] = new SpeakerPortrait(portraits[ii].ID, new EmoteStyle(portraits[ii].Emote, true),
                    new Loc(Bounds.End.X - GraphicsManager.MenuBG.TileWidth + (GraphicsManager.PortraitSize + 2) * (ii - portraits.Length),
                    Bounds.Y + GraphicsManager.MenuBG.TileHeight), false);
            
        }
    }
}
