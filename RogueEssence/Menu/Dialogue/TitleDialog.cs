using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class TitleDialog : IInteractable
    {
        protected const int CURSOR_FLASH_TIME = 24;
        public const int TEXT_TIME = 1;
        public const int SIDE_BUFFER = 8;
        public const int TEXT_HEIGHT = 16;
        public const int MAX_LINES = 2;
        public const int FADE_TIME = 60;

        public List<TextPause> Pauses;

        //Dialogue Text needs to be able to set character index accurately
        public DialogueText Text;
        public bool FadeIn;
        public int HoldTime;

        private bool showing;
        private FrameTick CurrentFadeTime;
        private FrameTick CurrentTextTime;
        private Action action;

        public virtual bool IsCheckpoint { get { return false; } }
        public bool Inactive { get; set; }
        public bool BlockPrevious { get; set; }
        public bool Visible { get; set; }

        public TitleDialog(string message, bool fadeIn, int holdTime, Action action)
        {
            Visible = true;
            this.action = action;
            FadeIn = fadeIn;
            showing = true;
            if (!FadeIn)
                CurrentFadeTime = FrameTick.FromFrames(FADE_TIME);

            HoldTime = holdTime;

            //message will contain pauses, which get parsed here.
            //and colors, which will get parsed by the text renderer

            Pauses = new List<TextPause>();
            int startIndex = 0;
            int tabIndex = message.IndexOf("[pause=", startIndex, StringComparison.OrdinalIgnoreCase);
            while (tabIndex > -1)
            {
                int endIndex = message.IndexOf("]", tabIndex);
                if (endIndex == -1)
                    break;
                int param;
                if (Int32.TryParse(message.Substring(tabIndex + "[pause=".Length, endIndex - (tabIndex + "[pause=".Length)), out param))
                {
                    TextPause pause = new TextPause();
                    pause.LetterIndex = tabIndex;
                    pause.Time = param;
                    message = message.Remove(tabIndex, endIndex - tabIndex + "]".Length);
                    Pauses.Add(pause);

                    tabIndex = message.IndexOf("[pause=", startIndex, StringComparison.OrdinalIgnoreCase);
                }
                else
                    break;
            }
            string newMessage = message;

            Text = new DialogueText(newMessage, new Rect(0, 0, GraphicsManager.ScreenWidth, GraphicsManager.ScreenHeight), TEXT_HEIGHT, true, true, FadeIn ? -1 : 0);
        }

        public void ProcessActions(FrameTick elapsedTime)
        {
            if (showing)
            {
                if (CurrentFadeTime < FADE_TIME)
                    CurrentFadeTime += elapsedTime;
                else
                    CurrentTextTime += elapsedTime;
            }
            else
                CurrentFadeTime -= elapsedTime;

            Text.TextOpacity = CurrentFadeTime.FractionOf(FADE_TIME);
        }

        public void Update(InputManager input)
        {
            if (!showing || CurrentFadeTime < FADE_TIME)
            {
                if (!showing && CurrentFadeTime <= 0)
                {
                    //close this
                    MenuManager.Instance.RemoveMenu();

                    //do what it wants
                    action();
                }
            }
            else if (!Text.Finished)
            {
                TextPause textPause = getCurrentTextPause();
                bool continueText = false;
                if (textPause != null)
                {
                    if (textPause.Time > 0)
                        continueText = CurrentTextTime >= textPause.Time;
                    else
                        continueText = (input.JustPressed(FrameInput.InputType.Confirm));
                }
                else
                    continueText = CurrentTextTime >= FrameTick.FromFrames(TEXT_TIME);

                if (continueText)
                {
                    CurrentTextTime = new FrameTick();
                    Text.CurrentCharIndex++;

                    if (textPause != null)//remove last text pause
                        Pauses.RemoveAt(0);
                }
            }
            else
            {
                bool exit = false;
                if (HoldTime > -1)
                {
                    if (CurrentTextTime >= HoldTime)
                        exit = true;
                }
                else
                {
                    if (input.JustPressed(FrameInput.InputType.Confirm) || input[FrameInput.InputType.Cancel])
                        exit = true;
                }

                if (exit)
                {
                    if (FadeIn)
                        showing = false;
                    else
                    {
                        //close this
                        MenuManager.Instance.RemoveMenu();

                        //do what it wants
                        action();
                    }
                }
            }

        }


        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            Text.Draw(spriteBatch, Loc.Zero);
            
            //when text is paused and waiting for input, flash a tick at the end
            TextPause textPause = getCurrentTextPause();
            if (textPause != null && textPause.Time == 0)
            {
                //text needs a "GetTextProgress" method, which returns the end loc of the string as its currently shown.
                //the coordinates are relative to the string's position
                Loc loc = Text.GetTextProgress() + Text.Rect.Start;

                if ((GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0)
                    GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(loc.X + 2, loc.Y), 0, 0);
            }

            //draw down-tick
            if (HoldTime == -1 && CurrentFadeTime >= FADE_TIME && Text.Finished && (GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0)
            {
                Loc end = Text.GetTextProgress();
                GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(GraphicsManager.ScreenWidth / 2 - 5, GraphicsManager.ScreenHeight / 2 + end.Y + 16), 1, 0);
            }

        }

        private TextPause getCurrentTextPause()
        {
            if (Pauses.Count > 0)
            {
                if (Pauses[0].LetterIndex == Text.CurrentCharIndex)
                    return Pauses[0];
            }
            return null;
        }
    }

}
