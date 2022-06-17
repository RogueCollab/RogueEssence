using RogueElements;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class MailMiniSummary : SummaryMenu
    {
        MenuText Name;
        MenuText Reward;
        MenuText LastSeen;
        MenuText Goal;
        SpeakerPortrait[] Portraits;

        public MailMiniSummary(Rect bounds)
            : base(bounds)
        {
            Name = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + 2));
            Elements.Add(Name);
            Goal = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE + 2));
            Elements.Add(Goal);
            LastSeen = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + 2));
            Elements.Add(LastSeen);
            Reward = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + 2));
            Elements.Add(Reward);
            Portraits = new SpeakerPortrait[0];
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Name;
            yield return Reward;

            yield return LastSeen;
            yield return Goal;

            foreach (SpeakerPortrait portrait in Portraits)
                yield return portrait;
        }

        public void SetSOS(SOSMail mail)
        {
            if (mail != null)
            {
                List<ModVersion> curVersions = PathMod.GetModVersion();
                List<ModDiff> versionDiff = PathMod.DiffModVersions(mail.DefeatedVersion, curVersions);
                Name.SetText(mail.TeamName);
                Goal.SetText(Text.FormatKey("MENU_SOS_GOAL", mail.GoalText.ToLocal().Replace('\n', ' ')));
                if (versionDiff.Count > 0)
                {
                    LastSeen.SetText("[" + Text.FormatKey("MENU_MAIL_INCOMPATIBLE") + "]");
                    Reward.SetText("");
                }
                else
                {
                    LastSeen.SetText(Text.FormatKey("MENU_SOS_DATE", mail.DateDefeated));
                    Reward.SetText(Text.FormatKey("MENU_SOS_REWARD", mail.OfferedItem.Value > -1 ? mail.OfferedItem.GetDungeonName() : "---"));
                }
                Portraits = new SpeakerPortrait[mail.TeamProfile.Length];
                for (int ii = 0; ii < mail.TeamProfile.Length; ii++)
                    Portraits[ii] = new SpeakerPortrait(mail.TeamProfile[ii], new EmoteStyle(GraphicsManager.SOSEmotion, true),
                        new Loc(GraphicsManager.MenuBG.TileWidth + (GraphicsManager.PortraitSize + 2) * (ii - mail.TeamProfile.Length),
                        GraphicsManager.MenuBG.TileHeight), false);
            }
            else
                setError();
        }

        public void SetAOK(AOKMail mail)
        {
            if (mail != null)
            {
                Name.SetText(mail.TeamName);
                Goal.SetText(Text.FormatKey("MENU_SOS_GOAL", mail.GoalText.ToLocal().Replace('\n', ' ')));
                LastSeen.SetText(Text.FormatKey("MENU_SOS_DATE", mail.DateDefeated));
                Reward.SetText(Text.FormatKey("MENU_SOS_REWARD", mail.OfferedItem.Value > -1 ? mail.OfferedItem.GetDungeonName() : "---"));
                Portraits = new SpeakerPortrait[mail.TeamProfile.Length];
                for (int ii = 0; ii < mail.TeamProfile.Length; ii++)
                    Portraits[ii] = new SpeakerPortrait(mail.TeamProfile[ii], new EmoteStyle(0, true),
                        new Loc(GraphicsManager.MenuBG.TileWidth + (GraphicsManager.PortraitSize + 2) * (ii - mail.TeamProfile.Length),
                        GraphicsManager.MenuBG.TileHeight), false);
            }
            else
                setError();
        }

        private void setError()
        {
            Name.SetText("[" + Text.FormatKey("MENU_MAIL_ERROR") + "]");
            Goal.SetText(Text.FormatKey("MENU_SOS_GOAL", "---"));
            LastSeen.SetText(Text.FormatKey("MENU_SOS_DATE", "---"));
            Reward.SetText(Text.FormatKey("MENU_SOS_REWARD", "---"));
            Portraits = new SpeakerPortrait[0];
        }
    }
}
