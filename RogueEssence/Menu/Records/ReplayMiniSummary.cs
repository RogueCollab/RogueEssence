using RogueElements;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueEssence.Data;
using System.IO;

namespace RogueEssence.Menu
{
    public class ReplayMiniSummary : SummaryMenu
    {
        MenuText Name;
        MenuText Date;
        MenuText Location;
        MenuText Version;
        MenuText Filename;

        public ReplayMiniSummary(Rect bounds)
            : base(bounds)
        {
            Name = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + 2));
            Elements.Add(Name);

            Version = new MenuText("", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth - 2, GraphicsManager.MenuBG.TileHeight), DirH.Right);
            Elements.Add(Version);

            Location = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE + 2));
            Elements.Add(Location);

            Date = new MenuText("", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth - 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE), DirH.Right);
            Elements.Add(Date);

            Filename = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + 2));
            Elements.Add(Filename);
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Name;
            yield return Version;

            yield return Location;
            yield return Date;

            yield return Filename;

        }

        public void SetReplay(RecordHeaderData header)
        {
            Name.SetText(header.Name);
            
            Location.SetText(header.LocationString);
            Date.SetText(header.DateTimeString.Split('_')[0]);
            Filename.SetText(Path.GetFileName(header.Path));
            
        }
    }
}
