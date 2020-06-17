using RogueElements;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace RogueEssence
{
    public class FrameInput
    {

        public enum InputType
        {
            Confirm,
            Cancel,
            Attack,
            Run,
            Skills,
            Turn,
            Diagonal,
            TeamMode,
            Minimap,
            Menu,
            MsgLog,
            SkillMenu,
            ItemMenu,
            TacticMenu,
            TeamMenu,
            LeaderSwap1,
            LeaderSwap2,
            LeaderSwap3,
            LeaderSwap4,
            LeaderSwapBack,
            LeaderSwapForth,
            Skill1,
            Skill2,
            Skill3,
            Skill4,
            SortItems,
            SelectItems,
            Wait,
            LeftMouse,
            RightMouse,
            MuteMusic,
            ShowDebug,
            Ctrl,
            Pause,
            AdvanceFrame,
            Test,
            SpeedDown,
            SpeedUp,
            SeeAll,
            Restart,
            Count
        }


        private bool[] inputStates;

        public bool this[InputType i]
        {
            get
            {
                return inputStates[(int)i];
            }
        }

        public int TotalInputs { get { return inputStates.Length; } }

        public Dir8 Direction { get; private set; }
        public KeyboardState BaseKeyState { get; private set; }
        public GamePadState BaseGamepadState { get; private set; }

        public Loc MouseLoc { get; private set; }
        public int MouseWheel { get; private set; }
        public bool Active { get; private set; }

        public FrameInput()
        {
            inputStates = new bool[(int)InputType.Count];
            Direction = Dir8.None;
        }

        public FrameInput(GamePadState gamePad, KeyboardState keyboard, MouseState mouse)
        {
            Active = true;
            BaseGamepadState = gamePad;
            BaseKeyState = keyboard;

            Loc dirLoc = new Loc();

            if (gamePad.IsConnected)
            {
                if (gamePad.ThumbSticks.Left.Length() > 0.25f)
                    dirLoc = DirExt.ApproximateDir8(new Loc((int)(gamePad.ThumbSticks.Left.X * 100), (int)(-gamePad.ThumbSticks.Left.Y * 100))).GetLoc();

                //if (gamePad.ThumbSticks.Right.Length() > 0.25f)
                //    dirLoc = DirExt.ApproximateDir8(new Loc((int)(gamePad.ThumbSticks.Right.X * 100), (int)(-gamePad.ThumbSticks.Right.Y * 100))).GetLoc();


                if (gamePad.IsButtonDown(Buttons.DPadDown))
                    dirLoc = dirLoc + Dir4.Down.GetLoc();
                if (gamePad.IsButtonDown(Buttons.DPadLeft))
                    dirLoc = dirLoc + Dir4.Left.GetLoc();
                if (gamePad.IsButtonDown(Buttons.DPadUp))
                    dirLoc = dirLoc + Dir4.Up.GetLoc();
                if (gamePad.IsButtonDown(Buttons.DPadRight))
                    dirLoc = dirLoc + Dir4.Right.GetLoc();
            }

            if (dirLoc == Loc.Zero)
            {
                for (int ii = 0; ii < DiagManager.Instance.CurSettings.DirKeys.Length; ii++)
                {
                    if (keyboard.IsKeyDown(DiagManager.Instance.CurSettings.DirKeys[ii]))
                        dirLoc = dirLoc + ((Dir4)ii).GetLoc();
                }
            }

            if (dirLoc == Loc.Zero)
            {
                if (keyboard.IsKeyDown(Keys.NumPad2))
                    dirLoc = dirLoc + Dir8.Down.GetLoc();
                if (keyboard.IsKeyDown(Keys.NumPad4))
                    dirLoc = dirLoc + Dir8.Left.GetLoc();
                if (keyboard.IsKeyDown(Keys.NumPad8))
                    dirLoc = dirLoc + Dir8.Up.GetLoc();
                if (keyboard.IsKeyDown(Keys.NumPad6))
                    dirLoc = dirLoc + Dir8.Right.GetLoc();

                if (dirLoc == Loc.Zero)
                {
                    if (keyboard.IsKeyDown(Keys.NumPad3) || keyboard.IsKeyDown(Keys.NumPad1))
                        dirLoc = dirLoc + Dir8.Down.GetLoc();
                    if (keyboard.IsKeyDown(Keys.NumPad1) || keyboard.IsKeyDown(Keys.NumPad7))
                        dirLoc = dirLoc + Dir8.Left.GetLoc();
                    if (keyboard.IsKeyDown(Keys.NumPad7) || keyboard.IsKeyDown(Keys.NumPad9))
                        dirLoc = dirLoc + Dir8.Up.GetLoc();
                    if (keyboard.IsKeyDown(Keys.NumPad9) || keyboard.IsKeyDown(Keys.NumPad3))
                        dirLoc = dirLoc + Dir8.Right.GetLoc();
                }
            }

            Direction = dirLoc.GetDir();

            inputStates = new bool[(int)InputType.Count];

            if (gamePad.IsConnected)
            {
                for (int ii = 0; ii < DiagManager.Instance.CurSettings.ActionButtons.Length; ii++)
                    inputStates[ii] |= Settings.UsedByGamepad((InputType)ii) && gamePad.IsButtonDown(DiagManager.Instance.CurSettings.ActionButtons[ii]);
            }

            for (int ii = 0; ii < DiagManager.Instance.CurSettings.ActionKeys.Length; ii++)
                inputStates[ii] |= Settings.UsedByKeyboard((InputType)ii) && keyboard.IsKeyDown(DiagManager.Instance.CurSettings.ActionKeys[ii]);

            inputStates[(int)InputType.Wait] = keyboard.IsKeyDown(Keys.NumPad5);

            inputStates[(int)InputType.LeftMouse] = (mouse.LeftButton == ButtonState.Pressed);
            inputStates[(int)InputType.RightMouse] = (mouse.RightButton == ButtonState.Pressed);

            MouseLoc = new Loc(mouse.X, mouse.Y);

            inputStates[(int)InputType.MuteMusic] = keyboard.IsKeyDown(Keys.F11);
            inputStates[(int)InputType.ShowDebug] = keyboard.IsKeyDown(Keys.F1);

            ReadDevInput(keyboard, mouse);
        }

        public void ReadDevInput(KeyboardState keyboard, MouseState mouse)
        {
            if (DiagManager.Instance.DevMode)
            {
                MouseWheel = mouse.ScrollWheelValue;
                inputStates[(int)InputType.Ctrl] |= (keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift));

                inputStates[(int)InputType.Pause] |= keyboard.IsKeyDown(Keys.F2);
                inputStates[(int)InputType.AdvanceFrame] |= keyboard.IsKeyDown(Keys.F3);
                inputStates[(int)InputType.Test] |= keyboard.IsKeyDown(Keys.F4);
                inputStates[(int)InputType.SpeedDown] |= keyboard.IsKeyDown(Keys.F5);
                inputStates[(int)InputType.SpeedUp] |= keyboard.IsKeyDown(Keys.F6);
                //inputStates[(int)InputType.] |= keyboard.IsKeyDown(Keys.F7);
                //inputStates[(int)InputType.] |= keyboard.IsKeyDown(Keys.F8);
                inputStates[(int)InputType.SeeAll] |= keyboard.IsKeyDown(Keys.F9);
                inputStates[(int)InputType.Restart] |= keyboard.IsKeyDown(Keys.F12);
            }
        }


        public override bool Equals(object obj)
        {
            return (obj is FrameInput) && Equals((FrameInput)obj);
        }

        public bool Equals(FrameInput other)
        {
            if (Direction != other.Direction) return false;

            for (int ii = 0; ii < (int)InputType.Count; ii++)
            {
                if (inputStates[ii] != other.inputStates[ii]) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Direction.GetHashCode() ^ inputStates.GetHashCode();
        }

        public static bool operator ==(FrameInput input1, FrameInput input2)
        {
            return input1.Equals(input2);
        }

        public static bool operator !=(FrameInput input1, FrameInput input2)
        {
            return !(input1 == input2);
        }


        public static FrameInput Load(BinaryReader reader)
        {
            FrameInput input = new FrameInput();

            input.Direction = (Dir8)((int)reader.ReadByte());
            for (int ii = 0; ii < (int)FrameInput.InputType.Ctrl; ii++)
                input.inputStates[ii] = reader.ReadBoolean();
            //for (int ii = 0; ii < FrameInput.TOTAL_CHARS; ii++)
            //    input.CharInput[ii] = reader.ReadBoolean();
            return input;
        }
    }
}
