using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public abstract class DialogueBox : MenuBase, IInteractable
    {
        protected const int CURSOR_FLASH_TIME = 24;
        public const int TEXT_TIME = 1;
        public const int SIDE_BUFFER = 8;
        public const int TEXT_SPACE = 16;
        public const int PER_LINE_SPACE = 18;
        public const int MAX_LINES = 2;

        public bool Skippable;
        public List<TextPause> Pauses;

        //Dialogue Text needs to be able to set character index accurately
        public DialogueText Text;
        public bool Sound;

        protected FrameTick CurrentTextTime;

        public abstract void ProcessTextDone(InputManager input);

        //optional speaker box
        private SpeakerPortrait speakerPic;

        public bool IsCheckpoint { get { return false; } }
        public bool Inactive { get; set; }
        public bool BlockPrevious { get; set; }

        public DialogueBox(string message, bool sound)
        {
            Bounds = Rect.FromPoints(new Loc(SIDE_BUFFER, GraphicsManager.ScreenHeight - (16 + PER_LINE_SPACE * MAX_LINES)), new Loc(GraphicsManager.ScreenWidth - SIDE_BUFFER, GraphicsManager.ScreenHeight - 8));
            Sound = sound;

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

            Text = new DialogueText(newMessage, Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth*2, GraphicsManager.MenuBG.TileHeight),
                Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 4 - Bounds.X, TEXT_SPACE, false, true);
        }

        public void ProcessActions(FrameTick elapsedTime)
        {
            CurrentTextTime += elapsedTime;
        }

        public void Update(InputManager input)
        {
            if (!Text.Finished)
            {
                //if (input[FrameInput.InputType.Cancel] && (input.JustPressed(FrameInput.InputType.Confirm) || input.JustPressed(FrameInput.InputType.Start)))
                //{
                //    Text.CurrentCharIndex = Text.Text.Length;
                //    CurrentTextTime = new FrameTick();
                //    Pauses.Clear();
                //}
                //else
                //{
                    TextPause textPause = getCurrentTextPause();
                    bool continueText = false;
                    if (textPause != null)
                    {
                        if (textPause.Time > 0)
                            continueText = CurrentTextTime >= textPause.Time;
                        else
                            continueText = (input.JustPressed(FrameInput.InputType.Confirm) || input[FrameInput.InputType.Cancel]
                                || input.JustPressed(FrameInput.InputType.LeftMouse));
                    }
                    else
                        continueText = CurrentTextTime >= FrameTick.FromFrames(TEXT_TIME);

                    if (continueText)
                    {
                        CurrentTextTime = new FrameTick();
                        Text.CurrentCharIndex++;
                        if (Sound && Text.CurrentCharIndex % 3 == 0)
                            GameManager.Instance.SE("Menu/Speak");

                        if (textPause != null)//remove last text pause
                            Pauses.RemoveAt(0);
                    }
                //}
            }
            else
                ProcessTextDone(input);

        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);
            
            //when text is paused and waiting for input, flash a tick at the end
            TextPause textPause = getCurrentTextPause();
            if (textPause != null && textPause.Time == 0)
            {
                //text needs a "GetTextProgress" method, which returns the end loc of the string as its currently shown.
                //the coordinates are relative to the string's position
                Loc loc = Text.GetTextProgress() + Text.Start;

                if ((GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0)
                    GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(loc.X + 2, loc.Y), 0, 0);
            }

            if (speakerPic != null)
                speakerPic.Draw(spriteBatch, new Loc());

        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Text;
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

        public void SetPortrait(MonsterID speaker, EmoteStyle emotion)
        {
            if (speaker.IsValid())
            {
                Loc loc = new Loc(DialogueBox.SIDE_BUFFER, Bounds.Y - 56);
                speakerPic = new SpeakerPortrait(speaker, emotion, loc, true);
            }
        }

        public void SetSpeaker(MonsterID speaker, string speakerName, EmoteStyle emotion)
        {
            SetPortrait(speaker, emotion);

            if (!String.IsNullOrEmpty(speakerName))
            {
                Text.Text = speakerName + ": " + Text.Text;
                Text.CurrentCharIndex += speakerName.Length + 2;
                foreach(TextPause pause in Pauses)
                    pause.LetterIndex += speakerName.Length + 2;
            }
        }
    }

    public class TextPause
    {
        public int LetterIndex;
        public int Time;//1 in order to wait on button press
    }

}
