using System;
using System.Collections.Generic;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class TurnState
    {
        public TurnOrder CurrentOrder;

        /// <summary>
        /// The list of characters (represented by index) scheduled to take a turn within the current faction.
        /// </summary>
        public List<CharIndex> TurnToChar;

        public TurnState()
        {
            CurrentOrder = new TurnOrder(0, Faction.Player, 0, false);
            TurnToChar = new List<CharIndex>();
        }

        public void SkipRemainingTurns()
        {
            CurrentOrder.SkipAll = true;
        }

        public CharIndex GetCurrentTurnChar()
        {
            if (TurnToChar.Count == 0)
                return CharIndex.Invalid;
            return TurnToChar[CurrentOrder.TurnIndex];
        }



        public void UpdateCharRemoval(CharIndex charIndex)
        {
            //The TurnToChar list will always contain only one faction's worth of turns.
            //which faction can be automatically deduced from CurrentOrder.Faction
            if (charIndex.Faction != CurrentOrder.Faction)
                return;
            //all characters on the team with an index higher than the removed char need to be decremented to reflect their new position
            //the removed character (if they're in this turn map, which may not be true) needs to be removed

            //remove the character and update the indices accordingly
            int removeIndex = TurnToChar.Count;
            for (int ii = TurnToChar.Count - 1; ii >= 0; ii--)
            {
                CharIndex turnChar = TurnToChar[ii];
                if (turnChar.Team == charIndex.Team && turnChar.Guest == charIndex.Guest)
                {
                    if (turnChar.Char > charIndex.Char)
                        TurnToChar[ii] = new CharIndex(turnChar.Faction, turnChar.Team, turnChar.Guest, turnChar.Char - 1);
                    else if (turnChar.Char == charIndex.Char)
                    {
                        TurnToChar.RemoveAt(ii);
                        removeIndex = ii;
                    }
                }
            }

            if (CurrentOrder.TurnIndex > removeIndex)
                CurrentOrder.TurnIndex--;
        }

        public void UpdateTeamRemoval(Faction faction, int removedTeam)
        {
            //The TurnToChar list will always contain only one faction's worth of turns.
            //which faction can be automatically deduced from CurrentOrder.Faction
            if (faction != CurrentOrder.Faction)
                return;

            //all characters with a team index higher than the removed char need to be decremented to reflect their new position
            for (int ii = 0; ii < TurnToChar.Count; ii++)
            {
                CharIndex turnChar = TurnToChar[ii];
                if (turnChar.Team > removedTeam)
                    TurnToChar[ii] = new CharIndex(turnChar.Faction, turnChar.Team - 1, turnChar.Guest, turnChar.Char);
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
        
        public void LoadTeamTurnMap(Faction faction, int teamIndex, Team team)
        {
            //team members start off with their turns organized in-order
            //with the exception being the leader
            loadTeamMemberTurnMap(faction, teamIndex, false, team.LeaderIndex, team.Players);
            for (int ii = 0; ii < team.Players.Count; ii++)
            {
                if (ii != team.LeaderIndex)
                    loadTeamMemberTurnMap(faction, teamIndex, false, ii, team.Players);
            }
            for (int ii = 0; ii < team.Guests.Count; ii++)
                loadTeamMemberTurnMap(faction, teamIndex, true, ii, team.Guests);
        }

        private void loadTeamMemberTurnMap(Faction faction, int teamIndex, bool guest, int charIndex, EventedList<Character> playerList)
        {
            Character character = playerList[charIndex];
            if (!character.Dead)
            {
                if (CurrentOrder.TurnTier == 0)//decrement wait for all slow charas
                {
                    character.TurnWait--;
                    character.TiersUsed = 0;
                    character.TurnUsed = false;//refresh turn-used immediately after
                }
            }

            if (IsEligibleToMove(character))
                TurnToChar.Add(new CharIndex(faction, teamIndex, guest, charIndex));
        }

        public void AdjustSlotSwap(Faction faction, int teamIndex, bool guest, int oldSlot, int newSlot)
        {
            if (faction != CurrentOrder.Faction)
                return;

            for (int ii = 0; ii < TurnToChar.Count; ii++)
            {
                CharIndex turnChar = TurnToChar[ii];
                if (turnChar.Team == teamIndex && turnChar.Guest == guest)
                {
                    if (turnChar.Char == oldSlot)
                        TurnToChar[ii] = new CharIndex(turnChar.Faction, turnChar.Team, turnChar.Guest, newSlot);
                    else if (turnChar.Char == newSlot)
                        TurnToChar[ii] = new CharIndex(turnChar.Faction, turnChar.Team, turnChar.Guest, oldSlot);
                }
            }
        }


        public void AdjustLeaderSwap(Faction faction, int teamIndex, bool guest, int oldSlot, int newSlot)
        {
            if (faction != CurrentOrder.Faction)
                return;

            //find the index of the oldslot
            //move it to the place it belongs if it wasnt a leader
            int oldIdx = -1;
            int destIdx = -1;

            for (int ii = 0; ii < TurnToChar.Count; ii++)
            {
                CharIndex turnChar = TurnToChar[ii];
                if (turnChar.Team == teamIndex && turnChar.Guest == guest)
                {
                    if (turnChar.Char == oldSlot)
                        oldIdx = ii;
                    //keep updating the destIdx; we want the highest index equal to or lower than the leader's idx
                    if (turnChar.Char <= oldSlot)
                        destIdx = ii+1;
                }
            }

            //if we couldn't find the old leader index, don't need to change anything
            if (oldIdx > -1)
            {
                if (oldIdx < destIdx)
                    destIdx--;
                CharIndex leaderIndex = TurnToChar[oldIdx];
                TurnToChar.RemoveAt(oldIdx);
                TurnToChar.Insert(destIdx, leaderIndex);
            }

            //find the index of the newslot
            //move it to the front
            int newIdx = -1;
            int firstIdx = 0;

            for (int ii = 0; ii < TurnToChar.Count; ii++)
            {
                CharIndex turnChar = TurnToChar[ii];
                if (turnChar.Team == teamIndex && turnChar.Guest == guest)
                {
                    if (turnChar.Char == newSlot)
                        newIdx = ii;
                }
                if (turnChar.Team < teamIndex)
                    firstIdx = ii + 1;
            }

            if (newIdx > -1)
            {
                CharIndex leaderIndex = TurnToChar[newIdx];
                TurnToChar.RemoveAt(newIdx);
                TurnToChar.Insert(firstIdx, leaderIndex);
            }
        }

    }
}
