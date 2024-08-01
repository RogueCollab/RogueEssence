using RogueElements;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueEssence.Data;
using System;

namespace RogueEssence.Menu
{
    public abstract class RescueCardMenu : InteractableMenu
    {
        public MenuText Title;
        public MenuDivider Div;
        SpeakerPortrait[] Portraits;
        MenuText Name;
        MenuText LastSeen;
        MenuText Goal;
        MenuText Reward;

        public RescueCardMenu()
        {

            Bounds = new Rect(8, 8, 192, GraphicsManager.MenuBG.TileHeight * 2 + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * 7);

            Title = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            Portraits = new SpeakerPortrait[0];

            
            Name = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + TitledStripMenu.TITLE_OFFSET));
            LastSeen = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4 + TitledStripMenu.TITLE_OFFSET));
            Goal = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5 + TitledStripMenu.TITLE_OFFSET));
            Reward = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 6 + TitledStripMenu.TITLE_OFFSET));
        }

        public void SetSOS(SOSMail sos)
        {
            Title.SetText(Text.FormatKey("MENU_SOS_TITLE"));
            Portraits = new SpeakerPortrait[sos.TeamProfile.Length];
            for (int ii = 0; ii < sos.TeamProfile.Length; ii++)
                Portraits[ii] = new SpeakerPortrait(sos.TeamProfile[ii], new EmoteStyle(sos.RescuedBy == null ? GraphicsManager.SOSEmotion : 0, true),
                    new Loc(Bounds.Width / 2 - (GraphicsManager.PortraitSize * sos.TeamProfile.Length + (sos.TeamProfile.Length - 1) * 2) / 2 + ii * (GraphicsManager.PortraitSize + 2),
                    GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET), false);
            Name.SetText(Text.FormatKey("MENU_SOS_CLIENT", sos.TeamName));
            LastSeen.SetText(Text.FormatKey("MENU_SOS_DATE", sos.DateDefeated));
            Goal.SetText(Text.FormatKey("MENU_SOS_GOAL", sos.GoalText.ToLocal().Replace('\n', ' ')));
            Reward.SetText(!String.IsNullOrEmpty(sos.OfferedItem.Value) ? Text.FormatKey("MENU_SOS_REWARD", sos.OfferedItem.GetDungeonName()) : "");

        }

        public void SetAOK(AOKMail aok)
        {
            Title.SetText(Text.FormatKey("MENU_AOK_TITLE"));
            Portraits = new SpeakerPortrait[aok.RescuingProfile.Length];
            for (int ii = 0; ii < aok.RescuingProfile.Length; ii++)
                Portraits[ii] = new SpeakerPortrait(aok.RescuingProfile[ii], new EmoteStyle(GraphicsManager.AOKEmotion, true),
                    new Loc(Bounds.Width / 2 - (GraphicsManager.PortraitSize * aok.RescuingProfile.Length + (aok.RescuingProfile.Length - 1) * 2) / 2 + ii * (GraphicsManager.PortraitSize + 2),
                    GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET), false);
            Name.SetText(Text.FormatKey("MENU_AOK_TEAM", aok.RescuingTeam));
            LastSeen.SetText(Text.FormatKey("MENU_SOS_DATE", aok.DateRescued));
            Goal.SetText(Text.FormatKey("MENU_SOS_GOAL", aok.GoalText.ToLocal().Replace('\n', ' ')));
            Reward.SetText(!String.IsNullOrEmpty(aok.OfferedItem.Value) ? Text.FormatKey("MENU_SOS_REWARD", aok.OfferedItem.GetDungeonName()) : "");

        }

        protected override IEnumerable<IMenuElement> GetDrawElements()
        {
            yield return Title;
            yield return Div;

            foreach (SpeakerPortrait portrait in Portraits)
                yield return portrait;

            yield return Name;
            yield return LastSeen;
            yield return Goal;
            yield return Reward;

        }

    }
}
