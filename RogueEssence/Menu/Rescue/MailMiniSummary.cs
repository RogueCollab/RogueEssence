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
            Reward = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + 2));
            Elements.Add(Reward);
            LastSeen = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE + 2));
            Elements.Add(LastSeen);
            Goal = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth + 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + 2));
            Elements.Add(Goal);
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
                Name.SetText(mail.TeamName);
                Reward.SetText(Text.FormatKey("MENU_SOS_REWARD", mail.OfferedItem.Value > -1 ? mail.OfferedItem.GetDungeonName() : "---"));
                LastSeen.SetText(Text.FormatKey("MENU_SOS_DATE", mail.DateDefeated));
                Goal.SetText(Text.FormatKey("MENU_SOS_GOAL", mail.GoalText.ToLocal().Replace('\n', ' ')));
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
                Reward.SetText(Text.FormatKey("MENU_SOS_REWARD", mail.OfferedItem.Value > -1 ? mail.OfferedItem.GetDungeonName() : "---"));
                LastSeen.SetText(Text.FormatKey("MENU_SOS_DATE", mail.DateDefeated));
                Goal.SetText(Text.FormatKey("MENU_SOS_GOAL", mail.GoalText.ToLocal().Replace('\n', ' ')));
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
            Reward.SetText(Text.FormatKey("MENU_SOS_REWARD", "---"));
            LastSeen.SetText(Text.FormatKey("MENU_SOS_DATE", "---"));
            Goal.SetText(Text.FormatKey("MENU_SOS_GOAL", "---"));
            Portraits = new SpeakerPortrait[0];
        }
    }
}
