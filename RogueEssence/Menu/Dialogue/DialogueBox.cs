using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using System.Text.RegularExpressions;

namespace RogueEssence.Menu
{
    public abstract class DialogueBox : MenuBase, IInteractable
    {
        protected const int CURSOR_FLASH_TIME = 24;
        public const int TEXT_TIME = 1;
        public const int SIDE_BUFFER = 8;
        public const int TEXT_SPACE = 16;//14
        public const int VERT_PAD = 2;//1
        public const int VERT_OFFSET = -2;//-3
        public const int HORIZ_PAD = 4;
        public const int MAX_LINES = 2;//3

        public bool Skippable;
        public List<TextPause> Pauses;

        //Dialogue Text needs to be able to set character index accurately
        public DialogueText Text;
        public bool Sound;

        protected FrameTick TotalTextTime;
        protected FrameTick CurrentTextTime;

        public abstract void ProcessTextDone(InputManager input);

        //optional speaker box
        private SpeakerPortrait speakerPic;
        //the speakername, alone
        private string speakerName;
        //message with pauses, without speaker name
        private string message;

        public bool IsCheckpoint { get { return false; } }
        public bool Inactive { get; set; }
        public bool BlockPrevious { get; set; }

        public DialogueBox(string msg, bool sound)
        {
            Bounds = Rect.FromPoints(new Loc(SIDE_BUFFER, GraphicsManager.ScreenHeight - (16 + TEXT_SPACE * MAX_LINES + VERT_PAD * 2)), new Loc(GraphicsManager.ScreenWidth - SIDE_BUFFER, GraphicsManager.ScreenHeight - 8));

            Pauses = new List<TextPause>();
            speakerName = "";

            Sound = sound;
            message = msg;

            Text = new DialogueText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + HORIZ_PAD, GraphicsManager.MenuBG.TileHeight + VERT_PAD + VERT_OFFSET),
                Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 2 - HORIZ_PAD * 2 - Bounds.X, TEXT_SPACE, false, 0);

            updateMessage();
        }

        public void ProcessActions(FrameTick elapsedTime)
        {
            TotalTextTime += elapsedTime;
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
            else
                speakerPic = null;
        }

        public void SetSpeaker(MonsterID speaker, string name, EmoteStyle emotion)
        {
            SetPortrait(speaker, emotion);

            if (!String.IsNullOrEmpty(name))
                speakerName = name;
            else
                speakerName = "";
            updateMessage();
        }

        public void SetMessage(string msg, bool sound)
        {
            message = msg;
            Sound = sound;
            updateMessage();
        }

        private void updateMessage()
        {
            //message will contain pauses, which get parsed here.
            //and colors, which will get parsed by the text renderer
            Pauses.Clear();

            string msg = message;
            if (speakerName == "")
                Text.CurrentCharIndex = 0;
            else
            {
                msg = String.Format("{0}: {1}", speakerName, msg);
                Text.CurrentCharIndex = speakerName.Length + 2;
            }
            int startLag = 0;

            List<IntRange> ranges = new List<IntRange>();
            int lag = 0;
            MatchCollection matches = RogueEssence.Text.MsgTags.Matches(msg);
            foreach (Match match in matches)
            {
                foreach (string key in match.Groups.Keys)
                {
                    if (!match.Groups[key].Success)
                        continue;
                    switch (key)
                    {
                        case "pause":
                            {
                                TextPause pause = new TextPause();
                                pause.LetterIndex = match.Index - lag;
                                int param;
                                if (Int32.TryParse(match.Groups["pauseval"].Value, out param))
                                    pause.Time = param;
                                Pauses.Add(pause);
                                ranges.Add(new IntRange(match.Index, match.Index + match.Length));
                            }
                            break;
                        case "colorstart":
                        case "colorend":
                            break;
                    }
                }

                lag += match.Length;

                if (match.Index + match.Length <= Text.CurrentCharIndex)
                    startLag += match.Length;
            }

            for (int ii = ranges.Count - 1; ii >= 0; ii--)
                msg = msg.Remove(ranges[ii].Min, ranges[ii].Length);

            Text.CurrentCharIndex -= startLag;

            Text.SetText(msg);
        }
    }

    public class TextPause
    {
        public int LetterIndex;
        public int Time;//1 in order to wait on button press
    }

}
