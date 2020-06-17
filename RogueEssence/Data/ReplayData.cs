using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{
    [Serializable]
    public class ReplayData
    {
        public enum ReplayLog
        {
            StateLog = 0,
            GameLog,
            UILog,
            QuicksaveLog
        }
        
        public string RecordDir;
        public Version RecordVersion;
        public string RecordLang;
        public long QuicksavePos;
        public int CurrentState;
        public int CurrentAction;
        public int CurrentUI;

        public bool Paused;
        public bool OpenMenu;
        public GameManager.GameSpeed ReplaySpeed;

        public List<GameState> States;
        public List<GameAction> Actions;
        public List<int> UICodes;

        public ReplayData()
        {
            RecordDir = "";
            States = new List<GameState>();
            Actions = new List<GameAction>();
            UICodes = new List<int>();
            RecordVersion = new Version();
            ReplaySpeed = GameManager.GameSpeed.Normal;
        }

        public GameState ReadState()
        {
            GameState save = States[CurrentState];
            CurrentState++;
            return save;
        }

        public GameAction ReadCommand()
        {
            GameAction cmd = Actions[CurrentAction];
            CurrentAction++;
            return cmd;
        }

        public int ReadUI()
        {
            int cmd = UICodes[CurrentUI];
            CurrentUI++;
            return cmd;
        }
    }

    [Serializable]
    public class GameState
    {
        public GameProgress Save;
        public ZoneManager Zone;
    }
}
