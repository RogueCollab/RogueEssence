using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{

    [Serializable]
    public class AOKMail : BaseRescueMail
    {
        public override string Extension { get { return DataManager.AOK_EXTENSION; } }

        public string RescuingTeam;
        public string RescuingID;
        public string DateRescued;
        public string[] RescuingNames;
        public MonsterID[] RescuingProfile;
        public int[] RescuingPersonalities;
        public int FinalStatement;
        //TODO: just make this a byte array so that it doesn't undergo json serialization bloat
        public ReplayData RescueReplay;

        public AOKMail()
        { }

        public AOKMail(SOSMail sos, GameProgress progress, string dateTime, ReplayData replay)
        {
            TeamName = sos.TeamName;
            TeamID = sos.TeamID;
            Seed = sos.Seed;
            TurnsTaken = sos.TurnsTaken;
            RescueSeed = sos.RescueSeed;
            DateDefeated = sos.DateDefeated;
            Goal = sos.Goal;
            DefeatedVersion = sos.DefeatedVersion;
            OfferedItem = sos.OfferedItem;
            GoalText = sos.GoalText;
            TeamProfile = sos.TeamProfile;

            RescuingTeam = progress.ActiveTeam.Name;
            RescuingID = progress.UUID;
            DateRescued = dateTime;
            List<string> teamNames = new List<string>();
            List<MonsterID> teamProfile = new List<MonsterID>();
            List<int> teamPersonalities = new List<int>();
            foreach (Character chara in progress.ActiveTeam.Players)
            {
                teamNames.Add(chara.BaseName);
                teamProfile.Add(chara.BaseForm);
                teamPersonalities.Add(chara.Discriminator);
            }
            RescuingNames = teamNames.ToArray();
            RescuingProfile = teamProfile.ToArray();
            RescuingPersonalities = teamPersonalities.ToArray();

            RescueReplay = replay;
        }
    }
}
