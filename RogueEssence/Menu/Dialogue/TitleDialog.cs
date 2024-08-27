using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLua;
using RogueEssence.Content;
using System.Text.RegularExpressions;
using RogueEssence.Script;

namespace RogueEssence.Menu
{
    public class TitleDialog : IInteractable
    {
        public static Regex SplitTags = new Regex(@"\[scroll\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected const int HOLD_CANCEL_TIME = 30;
        private const int SCROLL_SPEED = 2;

        protected const int CURSOR_FLASH_TIME = 24;
        
        public const int SIDE_BUFFER = 8;
        public const int TEXT_HEIGHT = 16;
        public const int FADE_TIME = 60;

        public List<List<TextPause>> Pauses;
        protected List<TextPause> CurrentPause { get { return Pauses[curTextIndex]; } }
        public List<List<TextScript>> ScriptCalls;
        
        public object[] Scripts;
        protected List<TextScript> CurrentScript { get { return ScriptCalls[curTextIndex]; } }

        public List<List<TextSpeed>> Speeds;
        protected List<TextSpeed> CurrentSpeed { get { return Speeds[curTextIndex]; } }
        //Dialogue Text needs to be able to set character index accurately
        protected List<DialogueText> Texts;
        private int curTextIndex;
        private bool scrolling;
        protected DialogueText CurrentText { get { return Texts[curTextIndex]; } }
        protected bool CurrentBoxFinished { get { return CurrentText.Finished && CurrentPause.Count == 0 && CurrentSpeed.Count == 0 && CurrentScript.Count == 0; } }
        public bool Finished { get { return CurrentBoxFinished && curTextIndex == Texts.Count - 1; } }

        public bool UseFade;
        public int HoldTime;

        private bool fading;
        private bool fadingIn;
        private FrameTick CurrentFadeTime;
        private FrameTick TotalTextTime;
        private FrameTick CurrentTextTime;
        private FrameTick CurrentScrollTime;
        private Action action;
        private string message;
        private double currSpeed;

        private bool runningScript;
        private bool startedScript;
        public virtual bool IsCheckpoint { get { return false; } }
        public bool Inactive { get; set; }
        public bool BlockPrevious { get; set; }
        public bool Visible { get; set; }

        public virtual string Label { get; protected set; }
        public Rect Bounds;
        public int MaxLines;

        DepthStencilState s1;
        DepthStencilState s2;
        AlphaTestEffect alphaTest;

        public bool HasLabel()
        {
            return !string.IsNullOrEmpty(Label);
        }
        public bool LabelContains(string substr)
        {
            return HasLabel() && Label.Contains(substr);
        }

        public TitleDialog(string msg, bool fadeIn, int holdTime, Rect bounds, object[] scripts, Action action) : this("", msg, fadeIn, holdTime, bounds, scripts, action) { }
        public TitleDialog(string label, string msg, bool fadeIn, int holdTime, Rect bounds, object[] scripts, Action action)
        {
            Label = label;
            s1 = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = 1,
                DepthBufferEnable = false,
            };

            s2 = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.LessEqual,
                StencilPass = StencilOperation.Keep,
                ReferenceStencil = 1,
                DepthBufferEnable = false,
            };
            alphaTest = new AlphaTestEffect(GraphicsManager.GraphicsDevice);

            Visible = true;
            this.action = action;
            UseFade = fadeIn;
            if (UseFade)
            {
                fading = true;
                fadingIn = true;
            }
            else
                CurrentFadeTime = FrameTick.FromFrames(FADE_TIME);

            HoldTime = holdTime;

            //message will contain pauses, which get parsed here.
            //and colors, which will get parsed by the text renderer

            Pauses = new List<List<TextPause>>();
            Speeds = new List<List<TextSpeed>>();
            ScriptCalls = new List<List<TextScript>>();
            message = msg;
            Bounds = bounds;
            Scripts = scripts;
            Texts = new List<DialogueText>();
            updateMessage();
        }


        private void updateMessage()
        {
            //message will contain pauses, which get parsed here.
            //and colors, which will get parsed by the text renderer
            Texts.Clear();
            curTextIndex = 0;
            Pauses.Clear();
            Speeds.Clear();
            ScriptCalls.Clear();

            string msg = message;

            Loc maxSize = Loc.Zero;
            MaxLines = 1;

            string[] scrolls = SplitTags.Split(msg);
            for (int nn = 0; nn < scrolls.Length; nn++)
            {
                List<TextPause> pauses = new List<TextPause>();
                List<TextScript> scripts = new List<TextScript>();
                List<IntRange> tagRanges = new List<IntRange>();
                List<TextSpeed> speeds = new List<TextSpeed>();
                
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
                            case "colorstart":
                            case "colorend":
                                break;
                        }
                    }

                    lag += match.Length;
                }

                for (int ii = tagRanges.Count - 1; ii >= 0; ii--)
                    scrolls[nn] = scrolls[nn].Remove(tagRanges[ii].Min, tagRanges[ii].Length);

                Pauses.Add(pauses);
                ScriptCalls.Add(scripts);

                Speeds.Add(speeds);
                int startX = Bounds.X == -1 ? 0 : Bounds.X;
                int width = Bounds.Width <= -1 ? GraphicsManager.ScreenWidth - startX : Bounds.Width;


                DialogueText text = new DialogueText("", new Rect(startX, 0, width, GraphicsManager.ScreenHeight), TEXT_HEIGHT, true, true, UseFade ? -1 : 0);

                text.SetAndFormatText(scrolls[nn]);
                Texts.Add(text);

                Loc size = text.GetTextSize();
                maxSize = Loc.Max(maxSize, size);
                MaxLines = Math.Max(MaxLines, text.GetLineCount());
            }
            maxSize += new Loc(8 + 4);//Magic number plus VERT_BUFFER
            
            int boundX = Bounds.X == -1 ? (GraphicsManager.ScreenWidth - maxSize.X) / 2 : Bounds.X;
            int boundY = Bounds.Y == -1 ? (GraphicsManager.ScreenHeight - maxSize.Y) / 2 : Bounds.Y;
            int boundWidth = (Bounds.Width <= -1 ? maxSize.X : Bounds.Width) + 2; // Width buffer
            int boundHeight = Bounds.Height <= -1 ? maxSize.Y : Bounds.Height;
            
            
            Bounds = new Rect(boundX, boundY, boundWidth, boundHeight);
            foreach (DialogueText text in Texts)
                text.Rect = Bounds;
        }

        public void ProcessActions(FrameTick elapsedTime)
        {
            if (fading)
            {
                if (fadingIn)
                {
                    CurrentFadeTime += elapsedTime;
                    if (CurrentFadeTime >= FADE_TIME)
                        CurrentFadeTime = FrameTick.FromFrames(FADE_TIME);
                }
                else
                {
                    CurrentFadeTime -= elapsedTime;
                    if (CurrentFadeTime <= 0)
                        CurrentFadeTime = FrameTick.FromFrames(0);
                }
            }
            else
            {
                TotalTextTime += elapsedTime;
                CurrentTextTime += elapsedTime;
                if (scrolling)
                    CurrentScrollTime += elapsedTime;
            }
            CurrentText.TextOpacity = CurrentFadeTime.FractionOf(FADE_TIME);
        }

        public void Update(InputManager input)
        {
            if (fading)
            {
                if (!fadingIn && CurrentFadeTime <= 0)
                {
                    fading = false;
                    //close this
                    MenuManager.Instance.RemoveMenu();

                    //do what it wants
                    action();
                }
                else if (fadingIn && CurrentFadeTime >= FrameTick.FromFrames(FADE_TIME))
                    fading = false;
            }
            else if (!CurrentBoxFinished)
            {
                TextScript textScript = getCurrentTextScript();
                if (textScript != null)
                {
                    if (!startedScript && textScript.Script < Scripts.Length && textScript.Script >= 0)
                    {
                        object script = Scripts[textScript.Script];
                        if (script is Coroutine)
                        {
                            MenuManager.Instance.NextAction = waitForTaskDone(CoroutineManager.Instance.StartCoroutine((Coroutine) script, true) ); 
                            runningScript = true;
                            startedScript = true;
                        }
                        else if (script is LuaFunction)
                        {
                            LuaFunction luaFun = script as LuaFunction;
                            MenuManager.Instance.NextAction = waitForTaskDone(CoroutineManager.Instance.StartCoroutine(new Coroutine(LuaEngine.Instance.CallScriptFunction(luaFun)), true) );
                            runningScript = true;
                            startedScript = true;
                        }
                    }
                    
                    if (runningScript)
                    {
                        return;
                    }
                    else
                    {
                        startedScript = false;
                        runningScript = false;
                        CurrentScript.RemoveAt(0);      
                        CurrentTextTime = new FrameTick();
                    }
                }
                
                TextSpeed textSpeed = getCurrentTextSpeed();
                if (textSpeed != null)
                {
                    currSpeed = textSpeed.Speed;
                    CurrentSpeed.RemoveAt(0);
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
                        CurrentPause.RemoveAt(0);
                        CurrentTextTime = new FrameTick();
                    }
                }

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
                }
                if (CurrentText.Finished)
                    CurrentTextTime = new FrameTick();
            }
            else if (curTextIndex < Texts.Count - 1)
            {
                if (input.JustPressed(FrameInput.InputType.Confirm) || input[FrameInput.InputType.Cancel] && TotalTextTime >= HOLD_CANCEL_TIME
                    || input.JustPressed(FrameInput.InputType.LeftMouse))
                {
                    scrolling = true;
                }

                int scrollFrames = TEXT_HEIGHT * MaxLines / SCROLL_SPEED;
                if (CurrentScrollTime < FrameTick.FromFrames(scrollFrames))
                {
                    if (scrolling)
                        CurrentText.Rect.Start -= new Loc(0, SCROLL_SPEED);
                }
                else
                {
                    curTextIndex++;
                    CurrentScrollTime = new FrameTick();
                    scrolling = false;
                }
            }
            else//ProcessTextDone
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
                    if (UseFade)
                    {
                        fading = true;
                        fadingIn = false;
                    }
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

            spriteBatch.End();
            float scale = GraphicsManager.WindowZoom;
            Matrix zoomMatrix = Matrix.CreateScale(new Vector3(scale, scale, 1));
            Matrix orthMatrix = zoomMatrix * Matrix.CreateOrthographicOffCenter(
                0, GraphicsManager.WindowWidth, GraphicsManager.WindowHeight, 0,
                0, 1);

            alphaTest.Projection = orthMatrix;
            BlendState blend = new BlendState();
            blend.ColorWriteChannels = ColorWriteChannels.None;
            spriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointWrap, s1, null, alphaTest);

            GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height), null, Color.White);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, s2, null, null, zoomMatrix);

            //actual draw call
            {
                CurrentText.Draw(spriteBatch, Loc.Zero);

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

                //draw down-tick
                if (HoldTime == -1 && CurrentFadeTime >= FADE_TIME && CurrentBoxFinished)
                {
                    if ((GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0)
                        GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(Bounds.Center.X - 5, Bounds.End.Y - 6), 1, 0);
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, zoomMatrix);
        }
        
        private IEnumerator<YieldInstruction> waitForTaskDone(Coroutine coroutine)
        {
            while (true)
            {
                if (coroutine.FinishedYield())
                {
                    runningScript = false;
                    yield break;
                }
                yield return new WaitForFrames(1);
            }
        }


        private TextPause getCurrentTextPause()
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

        
        private TextScript getCurrentTextScript()
        {
            if (CurrentScript.Count > 0)
            {
                if (CurrentText.CurrentCharIndex < 0 || CurrentScript[0].LetterIndex <= CurrentText.CurrentCharIndex)
                    return CurrentScript[0];
            }
            return null;
        }

    }

}
