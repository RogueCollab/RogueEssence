using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{

    [Serializable]
    public class SOSMail : BaseRescueMail
    {
        public override string Extension { get { return DataManager.SOS_EXTENSION; } }

        public string RescuedBy;
        public string[] RescuingNames;
        public MonsterID[] RescuingTeam;
        public int[] RescuingPersonalities;
        public int FinalStatement;

        public SOSMail()
        { }

        public SOSMail(GameProgress progress, ZoneLoc goal, LocalText goalText, string dateTime, List<ModVersion> version)
        {
            TeamName = progress.ActiveTeam.Name;
            TeamID = progress.UUID;
            DateDefeated = dateTime;
            DefeatedVersion = version;

            List<MonsterID> teamProfile = new List<MonsterID>();
            foreach (Character chara in progress.ActiveTeam.Players)
                teamProfile.Add(chara.BaseForm);
            TeamProfile = teamProfile.ToArray();

            Seed = progress.Rand.FirstSeed;
            TurnsTaken = progress.TotalTurns;
            Goal = goal;
            GoalText = goalText;
            OfferedItem = new MapItem();
        }
    }

}
