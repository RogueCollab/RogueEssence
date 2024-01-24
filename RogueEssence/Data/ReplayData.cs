using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;
using System.Text;

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
            QuicksaveLog,
            GroundsaveLog,
            OptionLog,
        }
        
        [NonSerialized]
        public string RecordDir;
        public Version RecordVersion;
        public string RecordLang;

        //TODO: remove the need for these variables when gameprogress is saved on quicksave
        public long SessionTime;
        public long SessionStartTime;

        public long QuicksavePos;
        public long GroundsavePos;
        public int CurrentState;
        public int CurrentAction;
        public int CurrentUI;

        public bool Paused;
        public bool OpenMenu;
        public GameManager.GameSpeed ReplaySpeed;

        public List<GameState> States;
        public List<GameAction> Actions;
        public List<int> UICodes;


        // Replay verification booleans
        public int Desyncs;
        public bool SilentVerify;

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

        public string ReadUIString()
        {
            int count = ReadUI();
            StringBuilder str = new StringBuilder();
            for (int ii = 0; ii < count; ii++)
                str.Append((char)ReadUI());
            return str.ToString();
        }
    }

    [Serializable]
    public class GameState
    {
        public GameProgress Save;
        public ZoneManager Zone;
    }
}
