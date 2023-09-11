using System;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using RogueEssence.Menu;

namespace RogueEssence
{
    [Serializable]
    public class GamePadMap
    {
        public string Name;
        public Buttons[] ActionButtons;

        public GamePadMap()
        {
            Name = "";
            ActionButtons = new Buttons[(int)FrameInput.InputType.Wait];
        }

        public GamePadMap(GamePadMap other)
        {
            Name = other.Name;
            ActionButtons = new Buttons[(int)FrameInput.InputType.Wait];
            Array.Copy(other.ActionButtons, ActionButtons, ActionButtons.Length);
        }
    }

    [Serializable]
    public class Settings
    {
        public enum BattleSpeed
        {
            VerySlow,
            Slow,
            Normal,
            Fast,
            VeryFast
        }
        public enum SkillDefault
        {
            None,
            Attacks,
            All
        }

        public int BGMBalance;
        public int SEBalance;
        public BattleSpeed BattleFlow;


        public SkillDefault DefaultSkills;
        public int Minimap;

        private double textSpeed;
        public double TextSpeed
        {
            get { return textSpeed; }
            set
            {
                textSpeed = value;
                DialogueBox.TextSpeed = textSpeed;
            }
        }

        private int border;
        public int Border
        {
            get { return border; }
            set
            {
                border = value;
                MenuBase.BorderStyle = border;
            }
        }

        public int Window;
        public string Language;

        public Keys[] DirKeys;
        public Keys[] ActionKeys;
        public Dictionary<string, GamePadMap> GamepadMaps;
        public bool Enter;
        public bool NumPad;
        public bool InactiveInput;

        public List<ServerInfo> ServerList;
        public List<ContactInfo> ContactList;
        public List<PeerInfo> PeerList;


        public static HashSet<FrameInput.InputType> MenuConflicts;
        public static HashSet<FrameInput.InputType> DungeonConflicts;
        public static HashSet<FrameInput.InputType> ActionConflicts;
        public static HashSet<Keys> ForbiddenKeys;
        public static HashSet<Buttons> ForbiddenButtons;

        public static void InitStatic()
        {
            MenuConflicts = new HashSet<FrameInput.InputType>();
            MenuConflicts.Add(FrameInput.InputType.Confirm);
            MenuConflicts.Add(FrameInput.InputType.Cancel);
            MenuConflicts.Add(FrameInput.InputType.Menu);
            MenuConflicts.Add(FrameInput.InputType.MsgLog);
            MenuConflicts.Add(FrameInput.InputType.SkillMenu);
            MenuConflicts.Add(FrameInput.InputType.ItemMenu);
            MenuConflicts.Add(FrameInput.InputType.TacticMenu);
            MenuConflicts.Add(FrameInput.InputType.TeamMenu);
            MenuConflicts.Add(FrameInput.InputType.Minimap);
            MenuConflicts.Add(FrameInput.InputType.SortItems);
            MenuConflicts.Add(FrameInput.InputType.SelectItems);

            DungeonConflicts = new HashSet<FrameInput.InputType>();
            DungeonConflicts.Add(FrameInput.InputType.Attack);
            DungeonConflicts.Add(FrameInput.InputType.Run);
            DungeonConflicts.Add(FrameInput.InputType.Skills);
            DungeonConflicts.Add(FrameInput.InputType.Turn);
            DungeonConflicts.Add(FrameInput.InputType.Diagonal);
            DungeonConflicts.Add(FrameInput.InputType.TeamMode);
            DungeonConflicts.Add(FrameInput.InputType.Menu);
            DungeonConflicts.Add(FrameInput.InputType.Minimap);

            DungeonConflicts.Add(FrameInput.InputType.MsgLog);
            DungeonConflicts.Add(FrameInput.InputType.SkillMenu);
            DungeonConflicts.Add(FrameInput.InputType.ItemMenu);
            DungeonConflicts.Add(FrameInput.InputType.TacticMenu);
            DungeonConflicts.Add(FrameInput.InputType.TeamMenu);
            DungeonConflicts.Add(FrameInput.InputType.Minimap);
            DungeonConflicts.Add(FrameInput.InputType.LeaderSwap1);
            DungeonConflicts.Add(FrameInput.InputType.LeaderSwap2);
            DungeonConflicts.Add(FrameInput.InputType.LeaderSwap3);
            DungeonConflicts.Add(FrameInput.InputType.LeaderSwap4);
            DungeonConflicts.Add(FrameInput.InputType.LeaderSwapBack);
            DungeonConflicts.Add(FrameInput.InputType.LeaderSwapForth);

            ActionConflicts = new HashSet<FrameInput.InputType>();
            ActionConflicts.Add(FrameInput.InputType.Skills);
            ActionConflicts.Add(FrameInput.InputType.Skill1);
            ActionConflicts.Add(FrameInput.InputType.Skill2);
            ActionConflicts.Add(FrameInput.InputType.Skill3);
            ActionConflicts.Add(FrameInput.InputType.Skill4);
            ActionConflicts.Add(FrameInput.InputType.SkillPreview);

            ForbiddenKeys = new HashSet<Keys>();
            ForbiddenKeys.Add(Keys.None);
            ForbiddenKeys.Add(Keys.CapsLock);
            ForbiddenKeys.Add(Keys.PageUp);
            ForbiddenKeys.Add(Keys.PageDown);
            ForbiddenKeys.Add(Keys.End);
            ForbiddenKeys.Add(Keys.Home);
            ForbiddenKeys.Add(Keys.Select);
            ForbiddenKeys.Add(Keys.Print);
            ForbiddenKeys.Add(Keys.Execute);
            ForbiddenKeys.Add(Keys.PrintScreen);
            ForbiddenKeys.Add(Keys.Insert);
            ForbiddenKeys.Add(Keys.Delete);
            ForbiddenKeys.Add(Keys.Help);
            ForbiddenKeys.Add(Keys.LeftWindows);
            ForbiddenKeys.Add(Keys.RightWindows);
            ForbiddenKeys.Add(Keys.Sleep);

            for (int ii = 0; ii < 24; ii++)
                ForbiddenKeys.Add(Keys.F1 + ii);

            ForbiddenKeys.Add(Keys.NumLock);
            ForbiddenKeys.Add(Keys.Scroll);
            ForbiddenKeys.Add(Keys.LeftControl);
            ForbiddenKeys.Add(Keys.RightControl);
            ForbiddenKeys.Add(Keys.LeftAlt);
            ForbiddenKeys.Add(Keys.RightAlt);
            for (int ii = 0; ii < 20; ii++)
                ForbiddenKeys.Add(Keys.BrowserBack + ii);
            ForbiddenKeys.Add(Keys.Oem8);
            ForbiddenKeys.Add(Keys.ProcessKey);
            ForbiddenKeys.Add(Keys.Attn);
            ForbiddenKeys.Add(Keys.Crsel);
            ForbiddenKeys.Add(Keys.Exsel);
            ForbiddenKeys.Add(Keys.EraseEof);
            ForbiddenKeys.Add(Keys.Play);
            ForbiddenKeys.Add(Keys.Zoom);
            ForbiddenKeys.Add(Keys.Pa1);
            ForbiddenKeys.Add(Keys.OemClear);
            ForbiddenKeys.Add(Keys.ChatPadGreen);
            ForbiddenKeys.Add(Keys.ChatPadOrange);
            ForbiddenKeys.Add(Keys.Pause);
            ForbiddenKeys.Add(Keys.ImeConvert);
            ForbiddenKeys.Add(Keys.ImeNoConvert);
            ForbiddenKeys.Add(Keys.Kana);
            ForbiddenKeys.Add(Keys.Kanji);
            ForbiddenKeys.Add(Keys.OemAuto);
            ForbiddenKeys.Add(Keys.OemCopy);
            ForbiddenKeys.Add(Keys.OemEnlW);

            ForbiddenButtons = new HashSet<Buttons>();

            ForbiddenButtons.Add(Buttons.DPadDown);
            ForbiddenButtons.Add(Buttons.DPadLeft);
            ForbiddenButtons.Add(Buttons.DPadUp);
            ForbiddenButtons.Add(Buttons.DPadRight);

            ForbiddenButtons.Add(Buttons.LeftThumbstickDown);
            ForbiddenButtons.Add(Buttons.LeftThumbstickLeft);
            ForbiddenButtons.Add(Buttons.LeftThumbstickUp);
            ForbiddenButtons.Add(Buttons.LeftThumbstickRight);
            ForbiddenButtons.Add(Buttons.RightThumbstickDown);
            ForbiddenButtons.Add(Buttons.RightThumbstickLeft);
            ForbiddenButtons.Add(Buttons.RightThumbstickUp);
            ForbiddenButtons.Add(Buttons.RightThumbstickRight);
        }

        public Settings()
        {

            BGMBalance = 5;
            SEBalance = 5;
            BattleFlow = BattleSpeed.Normal;
            TextSpeed = 1.0;
            DefaultSkills = SkillDefault.Attacks;
            Language = "";

            Minimap = 100;
            Window = 2;

            DirKeys = new Keys[4];
            ActionKeys = new Keys[(int)FrameInput.InputType.Wait];
            GamepadMaps = new Dictionary<string, GamePadMap>();
            ServerList = new List<ServerInfo>();
            ContactList = new List<ContactInfo>();
            PeerList = new List<PeerInfo>();

            GamePadMap defaultMap = new GamePadMap();
            defaultMap.Name = "Unknown";
            DefaultControls(DirKeys, ActionKeys, defaultMap.ActionButtons);
            GamepadMaps["default"] = defaultMap;
            Enter = true;
            NumPad = true;
            InactiveInput = false;
        }

        public static void DefaultControls(Keys[] dirKeys, Keys[] actionKeys, Buttons[] actionButtons)
        {
            if (dirKeys != null)
            {
                dirKeys[0] = Keys.Down;
                dirKeys[1] = Keys.Left;
                dirKeys[2] = Keys.Up;
                dirKeys[3] = Keys.Right;
            }

            if (actionKeys != null)
            {
                actionKeys[(int)FrameInput.InputType.Confirm] = Keys.X;
                actionKeys[(int)FrameInput.InputType.Cancel] = Keys.Z;
                actionKeys[(int)FrameInput.InputType.Attack] = Keys.X;
                actionKeys[(int)FrameInput.InputType.Run] = Keys.Z;
                actionKeys[(int)FrameInput.InputType.Skills] = Keys.A;
                actionKeys[(int)FrameInput.InputType.Turn] = Keys.S;
                actionKeys[(int)FrameInput.InputType.Diagonal] = Keys.D;
                actionKeys[(int)FrameInput.InputType.Menu] = Keys.Escape;
                actionKeys[(int)FrameInput.InputType.MsgLog] = Keys.Tab;
                actionKeys[(int)FrameInput.InputType.SkillMenu] = Keys.Q;
                actionKeys[(int)FrameInput.InputType.ItemMenu] = Keys.W;
                actionKeys[(int)FrameInput.InputType.TacticMenu] = Keys.E;
                actionKeys[(int)FrameInput.InputType.TeamMenu] = Keys.R;
                actionKeys[(int)FrameInput.InputType.TeamMode] = Keys.C;
                actionKeys[(int)FrameInput.InputType.Minimap] = Keys.Back;
                actionKeys[(int)FrameInput.InputType.LeaderSwap1] = Keys.D1;
                actionKeys[(int)FrameInput.InputType.LeaderSwap2] = Keys.D2;
                actionKeys[(int)FrameInput.InputType.LeaderSwap3] = Keys.D3;
                actionKeys[(int)FrameInput.InputType.LeaderSwap4] = Keys.D4;
                actionKeys[(int)FrameInput.InputType.Skill1] = Keys.S;
                actionKeys[(int)FrameInput.InputType.Skill2] = Keys.D;
                actionKeys[(int)FrameInput.InputType.Skill3] = Keys.Z;
                actionKeys[(int)FrameInput.InputType.Skill4] = Keys.X;
                actionKeys[(int)FrameInput.InputType.SortItems] = Keys.S;
                actionKeys[(int)FrameInput.InputType.SelectItems] = Keys.A;
                actionKeys[(int)FrameInput.InputType.SkillPreview] = Keys.Back;
            }

            if (actionButtons != null)
            {
                actionButtons[(int)FrameInput.InputType.Confirm] = Buttons.A;
                actionButtons[(int)FrameInput.InputType.Cancel] = Buttons.B;
                actionButtons[(int)FrameInput.InputType.Attack] = Buttons.A;
                actionButtons[(int)FrameInput.InputType.Run] = Buttons.B;
                actionButtons[(int)FrameInput.InputType.Skills] = Buttons.LeftTrigger;
                actionButtons[(int)FrameInput.InputType.Turn] = Buttons.X;
                actionButtons[(int)FrameInput.InputType.Diagonal] = Buttons.RightTrigger;
                actionButtons[(int)FrameInput.InputType.Menu] = Buttons.Y;
                actionButtons[(int)FrameInput.InputType.TeamMode] = Buttons.Start;
                actionButtons[(int)FrameInput.InputType.Minimap] = Buttons.Back;
                actionButtons[(int)FrameInput.InputType.LeaderSwapBack] = Buttons.LeftShoulder;
                actionButtons[(int)FrameInput.InputType.LeaderSwapForth] = Buttons.RightShoulder;
                actionButtons[(int)FrameInput.InputType.Skill1] = Buttons.X;
                actionButtons[(int)FrameInput.InputType.Skill2] = Buttons.Y;
                actionButtons[(int)FrameInput.InputType.Skill3] = Buttons.A;
                actionButtons[(int)FrameInput.InputType.Skill4] = Buttons.B;
                actionButtons[(int)FrameInput.InputType.SortItems] = Buttons.X;
                actionButtons[(int)FrameInput.InputType.SelectItems] = Buttons.LeftTrigger;
                actionButtons[(int)FrameInput.InputType.SkillPreview] = Buttons.RightTrigger;
            }
        }

        public static bool UsedByKeyboard(FrameInput.InputType input)
        {
            switch (input)
            {
                case FrameInput.InputType.Confirm: return true;
                case FrameInput.InputType.Cancel: return true;
                case FrameInput.InputType.Attack: return true;
                case FrameInput.InputType.Run: return true;
                case FrameInput.InputType.Skills: return true;
                case FrameInput.InputType.Turn: return true;
                case FrameInput.InputType.Diagonal: return true;
                case FrameInput.InputType.TeamMode: return true;
                case FrameInput.InputType.Minimap: return true;
                case FrameInput.InputType.Menu: return true;
                case FrameInput.InputType.MsgLog: return true;
                case FrameInput.InputType.SkillMenu: return true;
                case FrameInput.InputType.ItemMenu: return true;
                case FrameInput.InputType.TacticMenu: return true;
                case FrameInput.InputType.TeamMenu: return true;
                case FrameInput.InputType.LeaderSwap1: return true;
                case FrameInput.InputType.LeaderSwap2: return true;
                case FrameInput.InputType.LeaderSwap3: return true;
                case FrameInput.InputType.LeaderSwap4: return true;
                case FrameInput.InputType.Skill1: return true;
                case FrameInput.InputType.Skill2: return true;
                case FrameInput.InputType.Skill3: return true;
                case FrameInput.InputType.Skill4: return true;
                case FrameInput.InputType.SortItems: return true;
                case FrameInput.InputType.SelectItems: return true;
                case FrameInput.InputType.SkillPreview: return true;
                default: return false;
            }
        }

        public static bool UsedByGamepad(FrameInput.InputType input)
        {
            switch (input)
            {
                case FrameInput.InputType.Confirm: return true;
                case FrameInput.InputType.Cancel: return true;
                case FrameInput.InputType.Attack: return true;
                case FrameInput.InputType.Run: return true;
                case FrameInput.InputType.Skills: return true;
                case FrameInput.InputType.Turn: return true;
                case FrameInput.InputType.Diagonal: return true;
                case FrameInput.InputType.TeamMode: return true;
                case FrameInput.InputType.Minimap: return true;
                case FrameInput.InputType.Menu: return true;
                case FrameInput.InputType.LeaderSwapBack: return true;
                case FrameInput.InputType.LeaderSwapForth: return true;
                case FrameInput.InputType.Skill1: return true;
                case FrameInput.InputType.Skill2: return true;
                case FrameInput.InputType.Skill3: return true;
                case FrameInput.InputType.Skill4: return true;
                case FrameInput.InputType.SortItems: return true;
                case FrameInput.InputType.SelectItems: return true;
                case FrameInput.InputType.SkillPreview: return true;
                default: return false;
            }
        }

    }

    [Serializable]
    public class LocalFormatControls : LocalFormat
    {
        public List<FrameInput.InputType> Enums;

        public LocalFormatControls() { Enums = new List<FrameInput.InputType>(); }
        public LocalFormatControls(string keyString, params FrameInput.InputType[] inputs)
        {
            Key = new StringKey(keyString);
            Enums = new List<FrameInput.InputType>();
            Enums.AddRange(inputs);
        }
        public LocalFormatControls(LocalFormatControls other) : base(other)
        {
            Enums = new List<FrameInput.InputType>();
            Enums.AddRange(other.Enums);
        }
        public override LocalFormat Clone() { return new LocalFormatControls(this); }

        public override string FormatLocal()
        {
            List<string> enumStrings = new List<string>();
            foreach (FrameInput.InputType t in Enums)
                enumStrings.Add(DiagManager.Instance.GetControlString(t));
            return Text.FormatGrammar(Key.ToLocal(), enumStrings.ToArray());
        }
    }
}
