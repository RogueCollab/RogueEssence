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
        public static Regex SplitTags = new Regex(@"\[scroll\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected const int HOLD_CANCEL_TIME = 30;
        private const int SCROLL_SPEED = 2;
        protected const int CURSOR_FLASH_TIME = 24;

        public static double TextSpeed;

        /// <summary>
        /// frames between speech blip
        /// </summary>
        const int SPEAK_FRAMES = 2;

        public const int SIDE_BUFFER = 8;
        public const int TEXT_HEIGHT = 16; //14
        public const int VERT_PAD = 2; //1
        public const int VERT_OFFSET = -2; //-3
        public const int HORIZ_PAD = 4;
        public const int MAX_LINES = 2; //3

        public bool Skippable;

        public List<List<TextPause>> Pauses;

        protected List<TextPause> CurrentPause { get { return Pauses[curTextIndex]; } }

        public List<List<TextScript>> ScriptCalls;

        protected List<TextScript> CurrentScript
        {
            get { return ScriptCalls[curTextIndex]; }
        }

        public List<List<TextSpeed>> Speeds;
        protected List<TextSpeed> CurrentSpeed { get { return Speeds[curTextIndex]; } }
        
        public List<List<TextEmote>> Emotes;
        protected List<TextEmote> CurrentEmote { get { return Emotes[curTextIndex]; } }
        
        //Dialogue Text needs to be able to set character index accurately
        protected List<DialogueText> Texts;
        private int curTextIndex;
        private bool scrolling;
        private bool centerH;
        private bool centerV;

        private int nextTextIndex;

        protected DialogueText CurrentText { get { return Texts[curTextIndex]; } }
        protected DialogueText NextText { get { return nextTextIndex > -1 ? Texts[nextTextIndex] : null; } }
        
        protected bool CurrentBoxFinished { get { return CurrentText.Finished && CurrentPause.Count == 0 && CurrentSpeed.Count == 0 && CurrentEmote.Count == 0 && CurrentScript.Count == 0; } }
        public bool Finished { get { return CurrentText.Finished && curTextIndex == Texts.Count-1; } }
        
        public bool Sound;

        protected FrameTick TotalTextTime;
        protected FrameTick CurrentTextTime;
        protected FrameTick LastSpeakTime;
        protected FrameTick CurrentScrollTime;

        public abstract void ProcessTextDone(InputManager input);

        //optional speaker box
        private SpeakerPortrait speakerPic;

        //the speakername, alone
        private string speakerName;

        //message with pauses, without speaker name
        private string message;

        private double currSpeed;

        public bool IsCheckpoint { get { return false; } }
        
        public bool Inactive { get; set; }
        public bool BlockPrevious { get; set; }
        public static Rect DefaultBounds => Rect.FromPoints(
            new Loc(SIDE_BUFFER, GraphicsManager.ScreenHeight - (16 + TEXT_HEIGHT * MAX_LINES + VERT_PAD * 2)),
            new Loc(GraphicsManager.ScreenWidth - SIDE_BUFFER, GraphicsManager.ScreenHeight - 8)
        );

        public DialogueBox(string msg, bool sound, bool centerH, bool centerV, Rect bounds)
        {
            Bounds = bounds;
            Pauses = new List<List<TextPause>>();
            ScriptCalls = new List<List<TextScript>>();
            Speeds = new List<List<TextSpeed>>();
            Emotes = new List<List<TextEmote>>();
            speakerName = "";

            Sound = sound;
            message = msg;

            Texts = new List<DialogueText>();
            this.centerH = centerH;
            this.centerV = centerV;
            updateMessage();
        }

        public virtual void ProcessActions(FrameTick elapsedTime)
        {
            TotalTextTime += elapsedTime;
            if (!CurrentBoxFinished)
            {
                CurrentTextTime += elapsedTime;
                LastSpeakTime += elapsedTime;
            }
            if (scrolling)
                CurrentScrollTime += elapsedTime;
        }

        public void Update(InputManager input)
        {
            if (!CurrentBoxFinished)
            {
                TextScript textScript = getCurrentTextScript();
                if (textScript != null)
                {
                    //TODO: execute callback and wait for its completion
                    CurrentScript.RemoveAt(0);
                }
                TextSpeed textSpeed = getCurrentTextSpeed();
                if (textSpeed != null)
                {
                    currSpeed = textSpeed.Speed;
                    CurrentSpeed.RemoveAt(0);
                }
                
                TextEmote textEmote = getCurrentTextEmote();
                if (textEmote != null)
                {
                    SetPortraitEmote(textEmote.Emote);
                    CurrentEmote.RemoveAt(0);
                }
                
                TextPause textPause = getCurrentTextPause();
                if (textPause != null)
                {
                    bool continueText;
                    if (textPause.Time > 0)
                        continueText = CurrentTextTime >= textPause.Time;
                    else
                        continueText = (input.JustPressed(FrameInput.InputType.Confirm) || input[FrameInput.InputType.Cancel]
                            || input.JustPressed(FrameInput.InputType.LeftMouse));

                    if (!continueText)
                        return;
                    else
                    {
                        //remove last text pause
                        CurrentPause.RemoveAt(0);
                        CurrentTextTime = new FrameTick();
                    }
                }

                bool addedText = false;
                
                double speed = currSpeed > 0 ? currSpeed : DialogueBox.TextSpeed;
                FrameTick subTick = speed > 0 ? new FrameTick((long)(FrameTick.FrameToTick(1) / speed)) : FrameTick.FromFrames(1);
                while (true)
                {
                    if (CurrentText.Finished || getCurrentTextScript() != null || getCurrentTextPause() != null)
                    {
                        CurrentTextTime = new FrameTick();
                        break;
                    }

                    if (CurrentTextTime < subTick)
                        break;
                    CurrentTextTime -= subTick;
                    CurrentText.CurrentCharIndex++;
                    addedText = true;
                }


                if (addedText && Sound && LastSpeakTime > SPEAK_FRAMES)
                {
                    LastSpeakTime = new FrameTick();
                    GameManager.Instance.SE("Menu/Speak");
                }
            }
            else if (curTextIndex < Texts.Count - 1)
            {
                if (input.JustPressed(FrameInput.InputType.Confirm) || input[FrameInput.InputType.Cancel] && TotalTextTime >= HOLD_CANCEL_TIME
                    || input.JustPressed(FrameInput.InputType.LeftMouse))
                {
                    scrolling = true;
                    nextTextIndex = curTextIndex + 1;
                    NextText.Rect.Start = CurrentText.Rect.Start + new Loc(0, TEXT_HEIGHT * MAX_LINES);
                }

                int scrollFrames = TEXT_HEIGHT * MAX_LINES / SCROLL_SPEED;
                if (CurrentScrollTime < FrameTick.FromFrames(scrollFrames))
                {
                    if (scrolling)
                    {
                        CurrentText.Rect.Start -= new Loc(0, SCROLL_SPEED);
                        NextText.Rect.Start -= new Loc(0, SCROLL_SPEED);
                    }
                }
                else
                {
                    nextTextIndex = -1;
                    scrolling = false;
                    curTextIndex++;
                    CurrentScrollTime = new FrameTick();
                }
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
                Loc loc = Bounds.Start + CurrentText.GetTextProgress() + CurrentText.Rect.Start;
                
                if ((GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0)
                    GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(loc.X + 2, loc.Y), 0, 0);
            }

            if (speakerPic != null)
                speakerPic.Draw(spriteBatch, new Loc());

            //draw down-tick
            if (CurrentBoxFinished && !Finished && !scrolling)
            {
                if ((GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0)
                    GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(Bounds.Center.X - 5, Bounds.End.Y - 6), 1, 0);
            }
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return CurrentText;
            if (nextTextIndex > -1)
                yield return NextText;
        }

        protected TextPause getCurrentTextPause()
        {
            if (CurrentPause.Count > 0)
            {
                if (CurrentText.CurrentCharIndex < 0 || CurrentPause[0].LetterIndex <= CurrentText.CurrentCharIndex)
                    return CurrentPause[0];
            }
            return null;
        }
        
        protected TextSpeed getCurrentTextSpeed()
        {
            if (CurrentSpeed.Count > 0)
            {
                if (CurrentText.CurrentCharIndex < 0 || CurrentSpeed[0].LetterIndex <= CurrentText.CurrentCharIndex)
                    return CurrentSpeed[0];
            }
            return null;
        }
        
        protected TextEmote getCurrentTextEmote()
        {
            if (CurrentEmote.Count > 0)
            {
                if (CurrentText.CurrentCharIndex < 0 || CurrentEmote[0].LetterIndex <= CurrentText.CurrentCharIndex)
                    return CurrentEmote[0];
            }
            return null;
        }

        protected TextScript getCurrentTextScript()
        {
            if (CurrentScript.Count > 0)
            {
                if (CurrentText.CurrentCharIndex < 0 || CurrentScript[0].LetterIndex <= CurrentText.CurrentCharIndex)
                    return CurrentScript[0];
            }
            return null;
        }

        public void SetPortrait(MonsterID speaker, EmoteStyle emotion, Loc speakerLoc)
        {
            if (speaker.IsValid())
            {
                speakerPic = new SpeakerPortrait(speaker, emotion, speakerLoc, true);
            }
            else
                speakerPic = null;
        }
        
        public void SetPortraitEmote(int emote)
        {
            if (speakerPic != null)
            {
                speakerPic = new SpeakerPortrait(speakerPic, emote);
            }
        }

        public void SetSpeaker(MonsterID speaker, string name, EmoteStyle emotion, Loc speakerLoc)
        {
            SetPortrait(speaker, emotion, speakerLoc);

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

        public void FinishText()
        {
            foreach(DialogueText text in Texts)
                text.FinishText();
        }

        private void updateMessage()
        {
            //message will contain pauses, which get parsed here.
            //and colors, which will get parsed by the text renderer
            Texts.Clear();
            curTextIndex = 0;
            nextTextIndex = -1;
            Pauses.Clear();
            Speeds.Clear();
            Emotes.Clear();
            ScriptCalls.Clear();

            int curCharIndex = 0;
            string msg = message;
            if (speakerName != "")
            {
                msg = String.Format("{0}: {1}", speakerName, msg);
                curCharIndex = speakerName.Length + 2;
            }
            int startLag = 0;

            string[] scrolls = SplitTags.Split(msg);
            for (int nn = 0; nn < scrolls.Length; nn++)
            {
                List<TextPause> pauses = new List<TextPause>();
                List<TextScript> scripts = new List<TextScript>();
                List<IntRange> tagRanges = new List<IntRange>();
                List<TextSpeed> speeds = new List<TextSpeed>();
                List<TextEmote> emotes = new List<TextEmote>();
                
                int lag = 0;
                MatchCollection matches = Text.MsgTags.Matches(scrolls[nn]);
                
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
                                    pauses.Add(pause);
                                    tagRanges.Add(new IntRange(match.Index, match.Index + match.Length));
                                }
                                break;
                            case "script":
                                {
                                    TextScript script = new TextScript();
                                    script.LetterIndex = match.Index - lag;
                                    int param;
                                    if (Int32.TryParse(match.Groups["scriptval"].Value, out param))
                                        script.Script = param;
                                    scripts.Add(script);
                                    tagRanges.Add(new IntRange(match.Index, match.Index + match.Length));
                                }
                                break;
                            case "speed":
                                {
                                    TextSpeed speed = new TextSpeed();
                                    speed.LetterIndex = match.Index - lag;
                                    double param;
                                    if (Double.TryParse(match.Groups["speedval"].Value, out param))
                                        speed.Speed = param;
                                    speeds.Add(speed);
                                    tagRanges.Add(new IntRange(match.Index, match.Index + match.Length));
                                }
                                break;
                            case "emote":
                                {
                                    TextEmote emote = new TextEmote();
                                    emote.LetterIndex = match.Index - lag;
                                    emote.Emote = GraphicsManager.Emotions.FindIndex((EmotionType element) => element.Name.ToLower() == match.Groups["emoteval"].Value.ToLower());
                                    emotes.Add(emote);
                                    tagRanges.Add(new IntRange(match.Index, match.Index + match.Length));
                                }
                                break;
                            case "colorstart":
                            case "colorend":
                                break;
                        }
                    }

                    lag += match.Length;

                    if (nn == 0 && match.Index + match.Length <= curCharIndex)
                        startLag += match.Length;
                }

                //remove the tags, leaving pure text (except for the color tags)
                for (int ii = tagRanges.Count - 1; ii >= 0; ii--)
                    scrolls[nn] = scrolls[nn].Remove(tagRanges[ii].Min, tagRanges[ii].Length);

                //split the text, being mindful of color tags
                List<DialogueText> texts = DialogueText.SplitFormattedText(scrolls[nn], new Rect(GraphicsManager.MenuBG.TileWidth + HORIZ_PAD, GraphicsManager.MenuBG.TileHeight + VERT_PAD + VERT_OFFSET,
                    Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - HORIZ_PAD * 2, Bounds.Height - GraphicsManager.MenuBG.TileHeight * 2 - VERT_PAD * 2 - VERT_OFFSET * 2), TEXT_HEIGHT, centerH, centerV, 0);

                int totalTrim = 0;
                int totalLength = 0;
                int curPause = 0;
                int curScript = 0;
                int curSpeed = 0;
                int curEmote = 0;
                for (int kk = 0; kk < texts.Count; kk++)
                {
                    DialogueText text = texts[kk];

                    //pauses and scripts need to be re-aligned to the removals done by breaking the text into lines
                    List<TextPause> subPauses = new List<TextPause>();
                    List<TextScript> subScripts = new List<TextScript>();
                    List<TextSpeed> subSpeeds = new List<TextSpeed>();
                    List<TextEmote> subEmotes = new List<TextEmote>();
                    int lineCount = text.GetLineCount();
                    for (int ii = 0; ii < lineCount; ii++)
                    {
                        totalTrim += text.GetLineTrim(ii);
                        int oldLength = totalLength;
                        totalLength += text.GetLineTrim(ii) + text.GetLineLength(ii);
                        for (; curPause < pauses.Count; curPause++)
                        {
                            TextPause pause = pauses[curPause];
                            if (pause.LetterIndex <= totalLength)
                            {
                                pause.LetterIndex -= totalTrim;
                                subPauses.Add(pause);
                            }
                            else
                                break;
                        }
                        for (; curScript < scripts.Count; curScript++)
                        {
                            TextScript script = scripts[curScript];
                            if (script.LetterIndex <= totalLength)
                            {
                                script.LetterIndex -= totalTrim;
                                subScripts.Add(script);
                            }
                            else
                                break;
                        }
                        
                        for (; curSpeed < speeds.Count; curSpeed++)
                        {
                            TextSpeed speed = speeds[curSpeed];
                            if (speed.LetterIndex <= totalLength)
                            {
                                speed.LetterIndex -= totalTrim;
                                subSpeeds.Add(speed);
                            }
                            else
                                break;
                        }
                        
                        for (; curEmote < emotes.Count; curEmote++)
                        {
                            TextEmote emote = emotes[curEmote];
                            if (emote.LetterIndex <= totalLength)
                            {
                                emote.LetterIndex -= totalTrim;
                                subEmotes.Add(emote);
                            }
                            else
                                break;
                        }
                    }
                    totalTrim = totalLength;

                    Pauses.Add(subPauses);
                    ScriptCalls.Add(subScripts);
                    Speeds.Add(subSpeeds);
                    Emotes.Add(subEmotes);
                    Texts.Add(text);
                }

            }
            CurrentText.CurrentCharIndex = curCharIndex - startLag;
        }
    }

    public class TextPause
    {
        public int LetterIndex;
        public int Time;//1 in order to wait on button press
    }
    
    public class TextSpeed
    {
        public int LetterIndex;
        public double Speed;
    }

    public class TextEmote
    {
        public int LetterIndex;
        public int Emote;
    }

    public class TextScript
    {
        public int LetterIndex;
        public int Script;
    }

}
