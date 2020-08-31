using System;
using RogueElements;
using Microsoft.Xna.Framework.Input;

namespace RogueEssence
{
    public class InputManager
    {
        private FrameInput PrevInput;
        private FrameInput CurrentInput;
        public long InputTime { get; private set; }
        public long AddedInputTime { get; private set; }

        public int MouseWheelDiff
        {
            get
            {
                int diff = CurrentInput.MouseWheel - PrevInput.MouseWheel;
                if (diff > Int32.MaxValue / 2)
                    diff = (PrevInput.MouseWheel - Int32.MinValue) + (Int32.MaxValue - CurrentInput.MouseWheel);
                else if (diff < Int32.MinValue / 2)
                    diff = (CurrentInput.MouseWheel - Int32.MinValue) + (Int32.MaxValue - PrevInput.MouseWheel);
                return diff;
            }
        }


        public bool this[FrameInput.InputType i] { get { return CurrentInput[i]; } }

        public Dir8 PrevDirection { get { return PrevInput.Direction; } }
        public Dir8 Direction { get { return CurrentInput.Direction; } }

        public Loc MouseLoc { get { return CurrentInput.MouseLoc; } }

        public InputManager()
        {
            PrevInput = new FrameInput();
            CurrentInput = new FrameInput();
        }

        public void SetFrameInput(FrameInput input)
        {
            if (input == CurrentInput)
            {
                AddedInputTime = 1;
                InputTime++;
            }
            else
            {
                AddedInputTime = 1;
                InputTime = 1;
            }

            PrevInput = CurrentInput;
            CurrentInput = input;

        }

        public bool OnlyPressed(FrameInput.InputType input)
        {
            //nonmeta input only
            for (int ii = 0; ii < (int)FrameInput.InputType.RightMouse; ii++)
            {
                if (ii != (int)input && CurrentInput[(FrameInput.InputType)ii])
                    return false;
            }
            return true;
        }

        public bool JustPressed(FrameInput.InputType input)
        {
            if (PrevInput.Active && CurrentInput.Active)
                return !PrevInput[input] && CurrentInput[input];
            return false;
        }

        public bool JustReleased(FrameInput.InputType input)
        {
            if (PrevInput.Active && CurrentInput.Active)
                return PrevInput[input] && !CurrentInput[input];
            return false;
        }

        public bool BaseKeyDown(Keys key)
        {
            return CurrentInput.BaseKeyState.IsKeyDown(key);
        }

        public bool BaseKeyPressed(Keys key)
        {
            if (PrevInput.Active && CurrentInput.Active)
                return (CurrentInput.BaseKeyState.IsKeyDown(key) && !PrevInput.BaseKeyState.IsKeyDown(key));
            return false;
        }

        public bool AnyKeyPressed()
        {
            if (PrevInput.Active && CurrentInput.Active)
                return (CurrentInput.BaseKeyState.GetPressedKeys().Length > 0 && PrevInput.BaseKeyState.GetPressedKeys().Length == 0);
            return false;
        }

        public bool BaseButtonDown(Buttons button)
        {
            return CurrentInput.BaseGamepadState.IsButtonDown(button);
        }

        public bool BaseButtonPressed(Buttons button)
        {
            if (PrevInput.Active && CurrentInput.Active)
                return (CurrentInput.BaseGamepadState.IsButtonDown(button) && !PrevInput.BaseGamepadState.IsButtonDown(button));
            return false;
        }

        public bool AnyButtonPressed()
        {
            if (PrevInput.Active && CurrentInput.Active)
            {
                GamePadButtons untouchedButtons = new GamePadButtons();
                return (CurrentInput.BaseGamepadState.Buttons != untouchedButtons && PrevInput.BaseGamepadState.Buttons == untouchedButtons);
            }
            return false;
        }
    }
}
