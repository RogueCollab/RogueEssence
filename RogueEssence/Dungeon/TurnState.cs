using System;
using System.Collections.Generic;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class TurnState
    {
        public TurnOrder CurrentOrder;
        public List<CharIndex> TurnToChar;

        public TurnState()
        {
            CurrentOrder = new TurnOrder(0, false, 0);
            TurnToChar = new List<CharIndex>();
        }

        public CharIndex GetCurrentTurnChar()
        {
            return TurnToChar[CurrentOrder.TurnIndex];
        }



        public void UpdateCharRemoval(int removedTeam, int charIndex)
        {
            //all characters on the team with an index higher than the removed char need to be decremented to reflect their new position
            //the removed character (if they're in this turn map, which may not be true) needs to be removed

            //remove the character and update the indices accordingly
            int removeIndex = TurnToChar.Count;
            for (int ii = TurnToChar.Count - 1; ii >= 0; ii--)
            {
                CharIndex turnChar = TurnToChar[ii];
                if (turnChar.Team == removedTeam)
                {
                    if (turnChar.Char > charIndex)
                        TurnToChar[ii] = new CharIndex(turnChar.Team, turnChar.Char - 1);
                    else if (turnChar.Char == charIndex)
                    {
                        TurnToChar.RemoveAt(ii);
                        removeIndex = ii;
                    }
                }
            }

            if (CurrentOrder.TurnIndex > removeIndex)
                CurrentOrder.TurnIndex--;
        }

        public void UpdateTeamRemoval(int removedTeam)
        {
            //all characters with a team index higher than the removed char need to be decremented to reflect their new position
            //keep in mind: team -1 is never removed
            for (int ii = 0; ii < TurnToChar.Count; ii++)
            {
                CharIndex turnChar = TurnToChar[ii];
                if (turnChar.Team > removedTeam)
                    TurnToChar[ii] = new CharIndex(turnChar.Team - 1, turnChar.Char);
            }
        }

        public bool IsEligibleToMove(Character character)
        {
            if (character.Dead)
                return false;

            if (CurrentOrder.SkipAll)
                return false;

            if (character.TurnUsed)
                return false;

            //switch statement on the turn tier
            switch (CurrentOrder.TurnTier)
            {
                case 0: //for 0, check to see if the character's turnwait is at or exceeds its -(movement speed) + 1
                    return (character.MovementSpeed >= 0 || character.TurnWait <= 0);
                case 1:
                case 5: //for 1 and 5, check to see if the character's movement speed is +3
                    return (character.MovementSpeed == 3);
                case 2:
                case 4: //for 2 and 4, check to see if the character's movement speed is +2
                    return (character.MovementSpeed == 2);
                case 3://for 3, check to see if the character's movement speed is +1 or +3
                    return (character.MovementSpeed == 1 || character.MovementSpeed == 3);
            }
            return false;
        }
        
        public void LoadTeamTurnMap(int teamIndex, Team team)
        {
            //team members start off with their turns organized in-order
            //with the exception being the leader
            loadTeamMemberTurnMap(teamIndex, team, team.GetLeaderIndex());
            for (int ii = 0; ii < team.Players.Count; ii++)
            {
                if (ii != team.GetLeaderIndex())
                    loadTeamMemberTurnMap(teamIndex, team, ii);
            }
        }

        private void loadTeamMemberTurnMap(int teamIndex, Team team, int charIndex)
        {
            Character character = team.Players[charIndex];
            if (!character.Dead)
            {
                if (CurrentOrder.TurnTier == 0)//decrement wait for all slow charas
                {
                    character.TurnWait--;
                    character.TurnUsed = false;//refresh turn-used immediately after
                }
            }

            if (IsEligibleToMove(character))
                TurnToChar.Add(new CharIndex(teamIndex, charIndex));
        }


        public void AdjustDemotion(int teamIndex, int newSlot)
        {
            for (int ii = 0; ii < TurnToChar.Count; ii++)
            {
                CharIndex turnChar = TurnToChar[ii];
                if (turnChar.Team == teamIndex)
                {
                    if (turnChar.Char == 0) //team member of char index 0 is now the last member of the team
                        TurnToChar[ii] = new CharIndex(turnChar.Team, newSlot);
                    else//also decrement everyone else in team
                        TurnToChar[ii] = new CharIndex(turnChar.Team, turnChar.Char - 1);
                    //Maybe we should update their turn position, since they're last.
                    //however, this code is only called for enemy death, so it's probably okay
                }
            }
        }

        public void AdjustPromotion(int teamIndex, int newSlot)
        {
            //this code is not necessarily safe when dealing with enemy teams,
            //but this only gets called when a player team messes with their team formation,
            //and when it's specifically the leader's turn.
            //so this code will ride on those assumptions

            //first put the current leader where it belongs (in order)
            CharIndex leaderChar = TurnToChar[0];
            for (int ii = TurnToChar.Count - 1; ii > 0; ii--)
            {
                CharIndex turnChar = TurnToChar[ii];
                if (turnChar.Team == teamIndex)
                {
                    if (turnChar.Char < leaderChar.Char)
                    {
                        TurnToChar.RemoveAt(0);
                        TurnToChar.Insert(ii, leaderChar);
                        break;
                    }
                }
            }
            //then, promote the new one
            for (int ii = 0; ii < TurnToChar.Count; ii++)
            {
                CharIndex turnChar = TurnToChar[ii];
                if (turnChar.Team == teamIndex)
                {
                    if (turnChar.Char == newSlot)
                    {
                        TurnToChar.RemoveAt(ii);
                        TurnToChar.Insert(0, turnChar);
                        break;
                    }
                }
            }
        }
    }
}
