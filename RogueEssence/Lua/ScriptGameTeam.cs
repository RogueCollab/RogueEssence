using RogueEssence.Ground;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Linq;

using NLua;
using System.Collections.Generic;
using RogueElements;
using System;

namespace RogueEssence.Script
{
    public partial class ScriptGame : ILuaEngineComponent
    {



        //===================================
        // Team Access
        //===================================

        /// <summary>
        /// Returns the index of the currently player controlled entity in the party.
        /// </summary>
        /// <returns>Index of the currently player controlled entity in the party.</returns>
        public int GetTeamLeaderIndex()
        {
            return DataManager.Instance.Save.ActiveTeam.LeaderIndex;
        }

        /// <summary>
        /// Sets the leader to the chosen index within the party.
        /// </summary>
        /// <param name="idx">The index of the team member within the team.</param>
        public void SetTeamLeaderIndex(int idx)
        {
            //make leader
            int oldIdx = DataManager.Instance.Save.ActiveTeam.LeaderIndex;
            DataManager.Instance.Save.ActiveTeam.LeaderIndex = idx;

            //update team
            if (GameManager.Instance.CurrentScene == GroundScene.Instance)
            {
                ZoneManager.Instance.CurrentGround.SetPlayerChar(new GroundChar(DataManager.Instance.Save.ActiveTeam.Leader,
                    ZoneManager.Instance.CurrentGround.ActiveChar.MapLoc,
                    ZoneManager.Instance.CurrentGround.ActiveChar.CharDir, "PLAYER"));
            }
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustLeaderSwap(Faction.Player, 0, false, oldIdx, idx);
                DungeonScene.Instance.ReloadFocusedPlayer();
            }
        }

        /// <summary>
        /// Prevents or allows the switching of leaders for the save file.
        /// </summary>
        /// <param name="canSwitch">Set to true to allow switching, set to false to prevent it.</param>
        public void SetCanSwitch(bool canSwitch)
        {
            DataManager.Instance.Save.NoSwitching = !canSwitch;
        }

        /// <summary>
        /// Prevents or allows the joining of recruits for the save file.
        /// </summary>
        /// <param name="canRecruit">Set to true to allow recruit joins, set to false to prevent it.</param>
        public void SetCanRecruit(bool canRecruit)
        {
            DataManager.Instance.Save.NoRecruiting = !canRecruit;
        }

        /// <summary>
        /// Returns the player party count.  Does not include guests.
        /// </summary>
        /// <returns>The count of players</returns>
        public int GetPlayerPartyCount()
        {
            return DataManager.Instance.Save.ActiveTeam.Players.Count;
        }

        /// <summary>
        /// Return the party as a LuaTable.  Does not include guests.
        /// </summary>
        /// <returns>A Lua Table of Characters</returns>
        public LuaTable GetPlayerPartyTable()
        {
            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, chara) table.insert(tbl, chara) end").First() as LuaFunction;
            foreach (Character ent in DataManager.Instance.Save.ActiveTeam.Players)
                addfn.Call(tbl, ent);
            return tbl;
        }

        /// <summary>
        /// Gets the character at the specified index within the player's team.
        /// </summary>
        /// <param name="index">The specified index</param>
        /// <returns>The team member retrieved.</returns>
        public Character GetPlayerPartyMember(int index)
        {
            return DataManager.Instance.Save.ActiveTeam.Players[index];
        }



        /// <summary>
        /// Adds a character to the player's team.
        /// </summary>
        /// <param name="character">The character to add.</param>
        public void AddPlayerTeam(Character character)
        {
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                DungeonScene.Instance.AddCharToTeam(Faction.Player, 0, false, character);
                character.Absentee = false;
                character.RefreshTraits();
                character.Tactic.Initialize(character);
            }
            else
            {
                DataManager.Instance.Save.ActiveTeam.Players.Add(character);
            }
        }

        /// <summary>
        /// Removes the character from the team, placing its item back in the inventory.
        /// </summary>
        /// <param name="slot">The slot of the player to remove.</param>
        public void RemovePlayerTeam(int slot)
        {
            Character player = DataManager.Instance.Save.ActiveTeam.Players[slot];

            if (GameManager.Instance.CurrentScene == GroundScene.Instance)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                {
                    InvItem heldItem = player.EquippedItem;
                    player.SilentDequipItem();
                    DataManager.Instance.Save.ActiveTeam.AddToInv(heldItem);
                }

                GroundScene.Instance.RemoveChar(slot);
            }
            else if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                {
                    InvItem heldItem = player.EquippedItem;
                    player.SilentDequipItem();
                    if (DataManager.Instance.Save.ActiveTeam.GetInvCount() + 1 < DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone))
                        DataManager.Instance.Save.ActiveTeam.AddToInv(heldItem);
                }

                DungeonScene.Instance.RemoveChar(new CharIndex(Faction.Player, 0, false, slot));
            }
            else
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                {
                    InvItem heldItem = player.EquippedItem;
                    player.SilentDequipItem();
                    DataManager.Instance.Save.ActiveTeam.AddToInv(heldItem);
                }

                ExplorerTeam team = DataManager.Instance.Save.ActiveTeam;

                team.Players.RemoveAt(slot);

                //update leader
                if (slot < team.LeaderIndex)
                    team.LeaderIndex--;
            }
        }

        /// <summary>
        /// Gets the number of guests currently in the player's party.
        /// </summary>
        /// <returns>The number of guests</returns>
        public int GetPlayerGuestCount()
        {
            return DataManager.Instance.Save.ActiveTeam.Guests.Count;
        }

        /// <summary>
        /// Return the guests as a LuaTable
        /// </summary>
        /// <returns>A Lua Table of Characters</returns>
        public LuaTable GetPlayerGuestTable()
        {
            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, chara) table.insert(tbl, chara) end").First() as LuaFunction;
            foreach (var ent in DataManager.Instance.Save.ActiveTeam.Guests)
                addfn.Call(tbl, ent);
            return tbl;
        }

        /// <summary>
        /// Gets the character at the specified index within the player's guests.
        /// </summary>
        /// <param name="index">The specified index</param>
        /// <returns>The team member retrieved.</returns>
        public Character GetPlayerGuestMember(int index)
        {
            return DataManager.Instance.Save.ActiveTeam.Guests[index];
        }


        /// <summary>
        /// Adds a character to the player's guests.
        /// </summary>
        /// <param name="character">The character to add.</param>
        public void AddPlayerGuest(Character character)
        {
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                DungeonScene.Instance.AddCharToTeam(Faction.Player, 0, true, character);
                character.RefreshTraits();
                character.Tactic.Initialize(character);
            }
            else
            {
                DataManager.Instance.Save.ActiveTeam.Guests.Add(character);
            }
        }

        /// <summary>
        /// Removes the character from the team's guests, placing its item back in the inventory.
        /// </summary>
        /// <param name="slot">The slot of the player to remove.</param>
        public void RemovePlayerGuest(int slot)
        {
            Character player = DataManager.Instance.Save.ActiveTeam.Guests[slot];

            if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                player.SilentDequipItem();

            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                DungeonScene.Instance.RemoveChar(new CharIndex(Faction.Player, 0, true, slot));
            else
                DataManager.Instance.Save.ActiveTeam.Guests.RemoveAt(slot);
        }


        /// <summary>
        /// Gets the number of characters currently in the player's assembly.
        /// </summary>
        /// <returns>The number of characters</returns>
        public object GetPlayerAssemblyCount()
        {
            return DataManager.Instance.Save.ActiveTeam.Assembly.Count;
        }

        /// <summary>
        /// Return the assembly as a LuaTable
        /// </summary>
        /// <returns>A Lua Table of Characters</returns>
        public LuaTable GetPlayerAssemblyTable()
        {
            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, chara) table.insert(tbl, chara) end").First() as LuaFunction;
            foreach (var ent in DataManager.Instance.Save.ActiveTeam.Assembly)
                addfn.Call(tbl, ent);
            return tbl;
        }

        /// <summary>
        /// Gets the character at the specified index within the player's assembly.
        /// </summary>
        /// <param name="index">The specified index</param>
        /// <returns>The assembly member retrieved.</returns>
        public Character GetPlayerAssemblyMember(int index)
        {
            return DataManager.Instance.Save.ActiveTeam.Assembly[index];
        }


        /// <summary>
        /// Adds a character to the player's assembly.
        /// </summary>
        /// <param name="character">The character to add.</param>
        public void AddPlayerAssembly(Character character)
        {
            DataManager.Instance.Save.ActiveTeam.Assembly.Insert(0, character);
        }

        /// <summary>
        /// Removes the character from the assembly, placing its item back in the inventory.
        /// </summary>
        /// <param name="slot">The slot of the player to remove.</param>
        public void RemovePlayerAssembly(int slot)
        {
            DataManager.Instance.Save.ActiveTeam.Assembly.RemoveAt(slot);
        }



    }
}
