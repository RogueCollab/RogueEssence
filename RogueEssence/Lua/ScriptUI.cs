using RogueEssence.Menu;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using NLua;
using RogueEssence.Dungeon;
using RogueEssence.Content;
using RogueEssence.Network;
using RogueEssence.Data;
using RogueEssence.Dev;
using Microsoft.Xna.Framework;
using RogueElements;

namespace RogueEssence.Script
{
    class ScriptUI : ILuaEngineComponent
    {
        /// <summary>
        /// Used to store a list of choice and their return value for most multi-choice menus.
        /// </summary>
        struct ChoicePair
        {
            string Text { get; set; }
            int Value { get; set; }
        }

        //Variables for storing multi-step operations, like setting the speaker in a dialog
        private object                 m_choiceresult = -1;
        private MonsterID       m_curspeakerID = MonsterID.Invalid;
        private string              m_curspeakerName= "";
        private bool m_curcenter_h = false;
        private bool m_curcenter_v = false;
        private bool m_curautoFinish = false;
        private EmoteStyle  m_curspeakerEmo = new EmoteStyle(0);
        private bool                m_curspeakerSnd = true;
        private IEnumerator<YieldInstruction> _m_curdialogue;
        private Rect m_curbounds = DialogueBox.DefaultBounds;
        private Loc m_curspeakerLoc = SpeakerPortrait.DefaultLoc;
        private Loc m_curchoiceLoc = DialogueChoiceMenu.DefaultLoc;
        
        private IInteractable _m_curchoice;

        private IEnumerator<YieldInstruction> m_curdialogue
        {
            get => _m_curdialogue;
            set
            {
                if ((_m_curdialogue == null || _m_curdialogue.Current.FinishedYield()) && (_m_curchoice == null || _m_curchoice.Inactive))
                    _m_curdialogue = value;
            }
        }

        private IInteractable m_curchoice
        {
            get => _m_curchoice;
            set
            {
                if ((_m_curdialogue == null || _m_curdialogue.Current.FinishedYield()) && (_m_curchoice == null || _m_curchoice.Inactive))
                    _m_curchoice = value;
            }
        }

        public ScriptUI()
        {
            ResetSpeaker();
        }

        public void Reset()
        {
            ResetSpeaker();
        }


        

        //================================================================
        // Dialogue
        //================================================================

        /// <summary>
        /// Waits for the player to press a button before continuing.
        /// </summary>
        public LuaFunction WaitInput;

        public void EmptyWaitMenu(bool anyInput)
        {
            try
            {
                if (DataManager.Instance.CurrentReplay == null)
                    m_curdialogue = MenuManager.Instance.SetWaitMenu(anyInput);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.EmptyWaitMenu({0}): Encountered exception", anyInput), e), DiagManager.Instance.DevMode);
            }
        }


        /// <summary>
        /// Displays a dialogue box with text, waiting until the player completes it.
        /// Takes a string as an argument.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="callbacks">The Lua table of callbacks for the textbox to call.</param>
        /// <example>
        /// UI:WaitShowDialogue("Hello World!")
        /// </example>
        public LuaFunction WaitShowDialogue;

        /// <summary>
        /// Displays a dialogue box with text, waiting until the specified time has expired.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="waitTime">The time for the textbox to remain on screen. Pass -1 to wait for layer input.</param>
        /// <param name="callbacks">The Lua table of callbacks for the textbox to call.</param>
        /// <example>
        /// UI:WaitShowTimedDialogue("Hello World!", 120)
        /// </example>
        public LuaFunction WaitShowTimedDialogue;

        /// <summary>
        /// Sets the current dialogue text to be shown.  Requires WaitDialog to actually display.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="waitTime">The time for the textbox to remain on screen. Pass -1 to wait for layer input.</param>
        /// <param name="callbacks">The Lua table of callbacks for the textbox to call.</param>
        public void TextDialogue(string text, int waitTime = -1, LuaTable callbacks = null)
        {
            try
            {
                object[] scripts = DialogueBox.CreateScripts(callbacks);
                if (DataManager.Instance.CurrentReplay == null)
                    m_curdialogue = MenuManager.Instance.SetDialogue(m_curspeakerID, m_curspeakerName, m_curspeakerEmo, m_curspeakerLoc, m_curspeakerSnd, () => { }, waitTime, m_curautoFinish, m_curcenter_h, m_curcenter_v, m_curbounds, scripts, new string[] { text });
                else
                {
                    if (!String.IsNullOrEmpty(m_curspeakerName))
                        DungeonScene.Instance.LogMsg(String.Format("{0}: {1}", m_curspeakerName, text));
                    else
                        DungeonScene.Instance.LogMsg(text);
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextDialogue({0}): Encountered exception.", text), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays a voice over, waiting until the player completes it.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="expireTime">The time for the textbox to remain on screen. Pass -1 to wait for layer input.</param>
        /// <param name="x">The X position of the box</param>
        /// <param name="y">The Y position of the box</param>
        /// <param name="width">Width of the box</param>
        /// <param name="height">Height of the box</param>
        /// <param name="callbacks">The Lua table of callbacks for the textbox to call.</param>
        /// <example>
        /// UI:WaitShowVoiceOver("Hello World!", 120)
        /// </example>
        public LuaFunction WaitShowVoiceOver;

        /// <summary>
        /// Sets the current voice-over text to be shown.  Requires WaitDialog to actually display.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="expireTime">The time for the textbox to remain on screen. Pass -1 to wait for layer input.</param>
        /// <param name="x">The X position of the box</param>
        /// <param name="y">The Y position of the box</param>
        /// <param name="width">Width of the box</param>
        /// <param name="height">Height of the box</param>
        /// <param name="callbacks">The Lua table of callbacks for the textbox to call.</param>
        public void TextVoiceOver(string text, int expireTime, int x = -1, int y = -1, int width = -1, int height = -1, LuaTable callbacks = null)
        {
            try
            {
                object[] scripts = DialogueBox.CreateScripts(callbacks);
                Rect bounds = new Rect(x, y, width, height);
                if (DataManager.Instance.CurrentReplay == null)
                    m_curdialogue = MenuManager.Instance.SetTitleDialog(expireTime, m_curautoFinish, bounds, scripts, () => { }, text);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextVoiceOver({0}, {1}): Encountered exception", text, expireTime), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Makes text pop up in the bottom-left corner by default. Displays concurrently with any other process.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="expireTime">The time for the textbox to remain on screen.</param>
        /// <param name="x">The X position of the box</param>
        /// <param name="y">The Y position of the box</param>
        /// <param name="width">Width of the box</param>
        /// <param name="height">Height of the box</param>
        /// <param name="centerH">Horizontal centering</param>
        /// <param name="centerV">Vertical centering</param>
        public void TextPopUp(string text, int expireTime, int x = -1, int y = -1, int width = -1, int height = -1, bool centerH = false, bool centerV = false)
        {
            try
            {
                Rect bounds = new Rect(x, y, width, height);
                GameManager.Instance.TextPopUp.SetMessage(text, expireTime, bounds, centerH, centerV);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextPopUp({0}, {1}): Encountered exception", text, expireTime), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Fades in a title text, waiting until the fade-in is complete.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="time">The time for the text to fade in.</param>
        /// <example>
        /// UI:WaitShowTitle("Hello World!", 60)
        /// </example>
        public LuaFunction WaitShowTitle;

        /// <summary>
        /// Shows text in the format of a title drop.  Requires WaitDialog to actually display.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="time">The time for the text to fade in.</param>
        public void TextShowTitle(string text, int time)
        {
            try
            {
                m_curdialogue = GameManager.Instance.FadeTitle(true, text, time);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextShowTitle({0}, {1}): Encountered exception", text, time), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Fades out the currently displayed title, waiting until the fade-out is complete.
        /// </summary>
        /// <param name="time">The time for the text to fade in.</param>
        /// <example>
        /// UI:WaitHideTitle(60)
        /// </example>
        public LuaFunction WaitHideTitle;

        /// <summary>
        /// Fades out the text set in a title drop.  Requires WaitDialog to actually fade.
        /// </summary>
        /// <param name="time">The time for the text to fade in.</param>
        public void TextFadeTitle(int time)
        {
            try
            {
                m_curdialogue = GameManager.Instance.FadeTitle(false, "", time);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextFadeTitle({0}, {1}): Encountered exception", time), e), DiagManager.Instance.DevMode);
            }
        }


        /// <summary>
        /// Fades in a chosen background image, with a chosen framerate, at a certain fade time, waiting until the fade-in is complete.
        /// </summary>
        /// <param name="bg">The background to show, from the list of BG textures.</param>
        /// <param name="frameTime">Framerate of the image animation.</param>
        /// <param name="fadeInTime">Time taken to fade in the image.</param>
        /// <example>
        /// UI:WaitShowBG("TestBG", 3, 60)
        /// </example>
        public LuaFunction WaitShowBG;

        /// <summary>
        /// Sets an image to display.  Requires WaitDialog to actually display.
        /// </summary>
        /// <param name="bg">The background to show, from the list of BG textures.</param>
        /// <param name="frameTime">Framerate of the image animation.</param>
        /// <param name="fadeInTime">Time taken to fade in the image.</param>
        public void ShowBG(string bg, int frameTime, int fadeInTime)
        {
            try
            {
                m_curdialogue = GameManager.Instance.FadeBG(true, new BGAnimData(bg, frameTime), fadeInTime);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextShowBG({0}, {1}, {2}): Encountered exception", bg, frameTime, fadeInTime), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Fades out the current background image, waiting until the fade-out is complete.
        /// </summary>
        /// <param name="time">Time taken to fade out the image.</param>
        /// <example>
        /// UI:WaitHideBG(60)
        /// </example>
        public LuaFunction WaitHideBG;

        /// <summary>
        /// Prepares a fade-out of the current image.  Requires WaitDialog to actually display.
        /// </summary>
        /// <param name="time">Time taken to fade out the image.</param>
        public void FadeBG(int time)
        {
            try
            {
                m_curdialogue = GameManager.Instance.FadeBG(false, new BGAnimData(), time);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ShowBG({0}): Encountered exception", time), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Clears the current speaker, so none is displayed the next time TextDialogue is called.
        /// This also resets any custom dialogue box positions, portrait positions, and choice positions.
        /// </summary>
        /// <param name="keysound">If turned on, the text from the dialogue boxes make sounds.  Default is on.</param>
        public void ResetSpeaker(bool keysound = true)
        {
            m_curspeakerID = MonsterID.Invalid;
            m_curspeakerName = null;
            m_curspeakerEmo = new EmoteStyle(0);
            m_curspeakerSnd = keysound;
            m_curautoFinish = false;
            m_curcenter_h = false;
            m_curcenter_v = false;
            m_curbounds = DialogueBox.DefaultBounds;
            m_curspeakerLoc = SpeakerPortrait.DefaultLoc;
            m_curchoiceLoc = DialogueChoiceMenu.DefaultLoc;
        }


        /// <summary>
        /// Sets the speaker to be displayed during the following calls to the TextDialogue functions.  It resets speaker emotion.
        /// </summary>
        /// <param name="name">Speaker name.</param>
        /// <param name="keysound">Plays sounds when text appears.</param>
        /// <param name="specie">Species of the speaker</param>
        /// <param name="form">Form of the speaker</param>
        /// <param name="skin">Skin of the speaker</param>
        /// <param name="gender">Gender of the speaker</param>
        public void SetSpeaker(string name, bool keysound = true, string specie = "", int form = -1, string skin = "", Gender gender = Gender.Unknown)
        {
            m_curspeakerID = new MonsterID(specie, form, skin, gender);
            m_curspeakerName = name;
            m_curspeakerEmo = new EmoteStyle(0);
            m_curspeakerSnd = keysound;
        }

        /// <summary>
        /// Sets the speaker to be displayed during the following calls to the TextDialogue functions.  It resets speaker emotion.
        /// </summary>
        /// <param name="chara">Character to set as speaker. This is a character in a ground map.</param>
        /// <param name="keysound">Plays sounds when text appears.</param>
        public void SetSpeaker(Ground.GroundChar chara, bool keysound = true)
        {
            if (chara != null)
            {
                m_curspeakerID = chara.CurrentForm;
                m_curspeakerName = chara.GetDisplayName();
            }
            else
            {
                m_curspeakerID = MonsterID.Invalid;
                m_curspeakerName = null;
            }

            m_curspeakerEmo = new EmoteStyle(0);
            m_curspeakerSnd = keysound;
        }

        /// <summary>
        /// Sets the speaker to be displayed during the following calls to the TextDialogue functions.  It resets speaker emotion.
        /// </summary>
        /// <param name="chara">Character to set as speaker. This is a character in a dungeon map.</param>
        /// <param name="keysound">Plays sounds when text appears.</param>
        public void SetSpeaker(Character chara, bool keysound = true)
        {
            if (chara != null)
            {
                m_curspeakerID = chara.CurrentForm;
                m_curspeakerName = chara.GetDisplayName(true);
            }
            else
            {
                m_curspeakerID = MonsterID.Invalid;
                m_curspeakerName = null;
            }

            m_curspeakerEmo = new EmoteStyle(0);
            m_curspeakerSnd = keysound;
        }

        /// <summary>
        /// Reverses the speaker orientation to face left instead of right.  This depends on the boolean passed in.
        /// </summary>
        /// <param name="reverse">Faces right if false, left if true.</param>
        public void SetSpeakerReverse(bool reverse)
        {
            m_curspeakerEmo.Reverse = reverse;
        }
        
        /// <summary>
        /// Sets the position of the choices for a question dialog.
        /// </summary>
        /// <param name="x">The X position</param>
        /// <param name="y">The Y position</param>
        public void SetChoiceLoc(int x, int y)
        {
            m_curchoiceLoc = new Loc(x, y);
        }

        /// <summary>
        /// Sets the position of the choices for a question dialog back to default.
        /// </summary>
        public void ResetChoiceLoc()
        {
            m_curchoiceLoc = DialogueChoiceMenu.DefaultLoc;
        }

        /// <summary>
        /// Sets the position and size of the dialogue box.
        /// </summary>
        /// <param name="x">The X position of the box</param>
        /// <param name="y">The Y position of the box</param>
        /// <param name="width">Width of the box</param>
        /// <param name="height">Height of the box</param>
        public void SetBounds(int x, int y, int width, int height)
        {
            m_curbounds = new Rect(x, y, width, height);
        }

        /// <summary>
        /// Resets the position and size of the dialogue box.
        /// </summary>
        public void ResetBounds()
        {
            m_curbounds = DialogueBox.DefaultBounds;
        }

        /// <summary>
        /// Sets the position of the speaker in a dialogue box.
        /// </summary>
        /// <param name="x">The X position</param>
        /// <param name="y">The Y position</param>
        public void SetSpeakerLoc(int x, int y)
        {
            m_curspeakerLoc = new Loc(x, y);
        }

        /// <summary>
        /// Resets the position of the speaker in a dialogue box.
        /// </summary>
        public void ResetSpeakerLoc()
        {
            m_curspeakerLoc = SpeakerPortrait.DefaultLoc;
        }

        /// <summary>
        /// Sets the emotion of the speaker in the dialogue box.
        /// </summary>
        /// <param name="emo">Emotion to display</param>
        /// <param name="reverse">Faces right if false, left if true.</param>
        public void SetSpeakerEmotion(string emo, bool reverse = false)
        {
            int emoteIndex = GraphicsManager.Emotions.FindIndex((EmotionType element) => element.Name.ToLower() == emo.ToLower());
            m_curspeakerEmo.Emote = emoteIndex;
            m_curspeakerEmo.Reverse = reverse;
        }

        /// <summary>
        /// Sets the centering of the text in the textbox.
        /// </summary>
        /// <param name="centerH">Horizontal centering</param>
        /// <param name="centerV">Vertical centering</param>
        public void SetCenter(bool centerH, bool centerV = false)
        {
            m_curcenter_h = centerH;
            m_curcenter_v = centerV;
        }

        /// <summary>
        /// Makes the text automatically finish when it shows up.
        /// </summary>
        /// <param name="autoFinish">Auto-finishes text if true.</param>
        public void SetAutoFinish(bool autoFinish)
        {
            m_curautoFinish = autoFinish;
        }


        /// <summary>
        /// Displays the currently set dialogue box and waits for the player to complete it.
        /// </summary>
        /// <example>
        /// UI:WaitDialog()
        /// </example>
        public LuaFunction WaitDialog;

        public Coroutine _WaitDialog()
        {
            if (DataManager.Instance.CurrentReplay != null)
                return new Coroutine(_DummyWait());

            return new Coroutine(m_curdialogue);
        }

        /// <summary>
        /// Wait for dialogue to finish and then CLEAN UP dialogue box
        /// </summary>
        /// <returns></returns>
        private IEnumerator<YieldInstruction> __WaitDialog()
        {
            yield return CoroutineManager.Instance.StartCoroutine(m_curdialogue);
            _m_curdialogue = null;
        }

        /// <summary>
        /// Instantly break. Used as default/invalid value when returning a yieldable value.
        /// </summary>
        private IEnumerator<YieldInstruction> _DummyWait()
        {
            yield break;
        }

        //================================================================
        // Menus
        //================================================================

        /// <summary>
        /// Ask a question answered by yes or no via character dialogue to the player.
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the integer value indicating the result of the menu, UI:ChoiceResult() must be called.
        ///
        /// The Yes/No menu returns 1 for yes, and 0 for no.
        /// </summary>
        /// <param name="message">Question to be asked to the user.</param>
        /// <param name="bdefaultstono">Whether the cursor starts on no by default</param>
        /// <param name="callbacks">The Lua table of callbacks for the textbox to call.</param>
        public void ChoiceMenuYesNo(string message, bool bdefaultstono = false, LuaTable callbacks = null)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI() == 0 ? false : true;
                return;
            }

            try
            {
                object[] scripts = DialogueBox.CreateScripts(callbacks);
                m_choiceresult = null;

                if (message == null)
                    message = "";

                if (m_curspeakerName != null)
                {
                    m_curchoice = MenuManager.Instance.CreateQuestion(m_curspeakerID,
                                                                      m_curspeakerName,
                                                                      m_curspeakerEmo,
                                                                      m_curspeakerLoc,
                                                                      message,
                                                                      m_curspeakerSnd, m_curautoFinish, m_curcenter_h, m_curcenter_v, m_curbounds, scripts, m_curchoiceLoc,
                                                                      () => { m_choiceresult = true; DataManager.Instance.LogUIPlay(1); },
                                                                      () => { m_choiceresult = false; DataManager.Instance.LogUIPlay(0); },
                                                                      bdefaultstono);
                }
                else
                {
                    m_curchoice = MenuManager.Instance.CreateQuestion(MonsterID.Invalid, null, new EmoteStyle(0), m_curspeakerLoc, message,
                        m_curspeakerSnd, m_curautoFinish, m_curcenter_h, m_curcenter_v, m_curbounds, scripts, m_curchoiceLoc,
                        () => { m_choiceresult = true; DataManager.Instance.LogUIPlay(1); },
                        () => { m_choiceresult = false; DataManager.Instance.LogUIPlay(0); }, bdefaultstono);
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ChoiceMenuYesNo({0}): Encountered exception.", message), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the name input box.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the string value indicating the result of the menu, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="title">The text to show above the input line.</param>
        /// <param name="desc">The text to show below the input line.</param>
        /// <param name="maxLength">The length of the text in pixels.</param>
        public void NameMenu(string title, string desc, int maxLength = 116)
        {
            try
            {
                m_choiceresult = "";
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new TeamNameMenu(title, desc, maxLength, (string name) => { m_choiceresult = name; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.NameMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays a menu for replacing party members with the assembly.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the boolean value indicating whether the team composition was changed or not, UI:ChoiceResult() must be called.
        /// </summary>
        public void AssemblyMenu()
        {
            try
            {
                m_choiceresult = false;
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new AssemblyMenu(0, () => { m_choiceresult = true; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.AssemblyMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Shop menu.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the table indicating the indices of items chosen, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="goods">A table of items to be sold.  The format is { Item=InvItem, Price=int } for each item.</param>
        public void ShopMenu(LuaTable goods)
        {
            try
            {
                m_choiceresult = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
                List<Tuple<InvItem, int>> goodsList = new List<Tuple<InvItem, int>>();
                foreach (object key in goods.Keys)
                {
                    LuaTable entry = goods[key] as LuaTable;
                    InvItem item = entry["Item"] as InvItem;
                    Int64 price = (Int64)entry["Price"];
                    goodsList.Add(new Tuple<InvItem, int>(item, (int)price));
                }
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new ShopMenu(goodsList, 0, (List<int> chosenGoods) =>
                {
                    LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, val) table.insert(tbl, val) end").First() as LuaFunction;
                    foreach (int chosenGood in chosenGoods)
                        addfn.Call(m_choiceresult, chosenGood+1);
                });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ShopMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Sell menu.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the table indicating the indices of items to sell, UI:ChoiceResult() must be called.
        /// </summary>
        public void SellMenu()
        {
            try
            {
                m_choiceresult = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new SellMenu(0, (List<InvSlot> chosenGoods) =>
                {
                    LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, val) table.insert(tbl, val) end").First() as LuaFunction;
                    foreach (InvSlot chosenGood in chosenGoods)
                        addfn.Call(m_choiceresult, chosenGood);
                });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SellMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Storage menu for which to exchange items in the inventory with.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the menu is exited.
        /// </summary>
        public void StorageMenu()
        {
            try
            {
                m_choiceresult = null;
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new DepositMenu(0);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.StorageMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Storage menu for which to withdraw from.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the menu is exited.
        /// </summary>
        public void WithdrawMenu()
        {
            try
            {
                m_choiceresult = null;
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new WithdrawMenu(0, true, onChooseSlot);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.WithdrawMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        private void onChooseSlot(List<WithdrawSlot> slots)
        {
            //store item
            List<InvItem> items = DataManager.Instance.Save.ActiveTeam.TakeItems(slots);

            foreach (InvItem item in items)
            {
                ItemData entry = DataManager.Instance.GetItem(item.ID);
                int existingStack = -1;
                if (entry.MaxStack > 1)
                {
                    for (int jj = 0; jj < DataManager.Instance.Save.ActiveTeam.GetInvCount(); jj++)
                    {
                        if (DataManager.Instance.Save.ActiveTeam.GetInv(jj).ID == item.ID && DataManager.Instance.Save.ActiveTeam.GetInv(jj).Amount < entry.MaxStack)
                        {
                            existingStack = jj;
                            break;
                        }
                    }
                }
                if (existingStack > -1)
                {
                    DataManager.Instance.Save.ActiveTeam.GetInv(existingStack).Amount += item.Amount;
                    DataManager.Instance.Save.ActiveTeam.UpdateInv(DataManager.Instance.Save.ActiveTeam.GetInv(existingStack), DataManager.Instance.Save.ActiveTeam.GetInv(existingStack));
                }
                else
                    DataManager.Instance.Save.ActiveTeam.AddToInv(item);
            }
        }


        /// <summary>
        /// Displays the Bank menu.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the menu is exited.
        /// </summary>
        public void BankMenu()
        {
            try
            {
                m_choiceresult = null;
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new BankMenu(DataManager.Instance.Save.ActiveTeam.Money, onChooseAmount);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.BankMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        private void onChooseAmount(int amount)
        {
            long total = (long)DataManager.Instance.Save.ActiveTeam.Bank + DataManager.Instance.Save.ActiveTeam.Money;
            DataManager.Instance.Save.ActiveTeam.Bank = (int)(total - amount);
            DataManager.Instance.Save.ActiveTeam.Money = amount;
        }

        /// <summary>
        /// Displays the Spoils menu.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the menu is exited.
        /// </summary>
        /// <param name="appraisalMap">A table of mappings from containers to items, in the format of { Box=InvItem , Item=InvItem }</param>
        public void SpoilsMenu(LuaTable appraisalMap)
        {
            try
            {
                List<Tuple<InvItem, InvItem>> goodsList = new List<Tuple<InvItem, InvItem>>();
                foreach (object key in appraisalMap.Keys)
                {
                    LuaTable entry = appraisalMap[key] as LuaTable;
                    InvItem box = entry["Box"] as InvItem;
                    InvItem item = entry["Item"] as InvItem;
                    goodsList.Add(new Tuple<InvItem, InvItem>(box, item));
                }
                m_curchoice = new SpoilsMenu(goodsList);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SpoilsMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Appraisal menu.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the table indicating the indices of items chosen, UI:ChoiceResult() must be called.
        /// </summary>
        public void AppraiseMenu()
        {
            try
            {
                m_choiceresult = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
                m_curchoice = new AppraiseMenu(0, (List<InvSlot> chosenGoods) =>
                {
                    LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, val) table.insert(tbl, val) end").First() as LuaFunction;
                    foreach (InvSlot chosenGood in chosenGoods)
                        addfn.Call(m_choiceresult, chosenGood);
                });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.AppraiseMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Tutor Team menu.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the integer representing the chosen team member, UI:ChoiceResult() must be called.
        /// </summary>
        public void TutorTeamMenu()
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                m_choiceresult = -1;
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new TutorTeamMenu(-1,
                    (int teamSlot) => { m_choiceresult = teamSlot; DataManager.Instance.LogUIPlay(teamSlot); },
                    () => { DataManager.Instance.LogUIPlay(-1); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TutorTeamMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Relearn menu for a character.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the integer representing the chosen skill, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="chara">The character to relearn skills</param>
        public void RelearnMenu(Character chara)
        {

            try
            {
                List<string> forgottenSkills = chara.GetRelearnableSkills(true);

                if (DataManager.Instance.CurrentReplay != null)
                {
                    m_choiceresult = forgottenSkills[DataManager.Instance.CurrentReplay.ReadUI()];
                    return;
                }

                m_choiceresult = "";
                m_curchoice = new SkillRecallMenu(chara, forgottenSkills.ToArray(),
                (int skillSlot) => { m_choiceresult = forgottenSkills[skillSlot]; DataManager.Instance.LogUIPlay(skillSlot); },
                () => { DataManager.Instance.LogUIPlay(-1); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.RelearnMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Learn menu for a character to replace an existing skill with a new one.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the integer representing the chosen skill, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="chara">The character to relearn skills</param>
        /// <param name="skillNum">The new skill</param>
        public void LearnMenu(Character chara, string skillNum)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                m_choiceresult = -1;
                m_curchoice = new SkillReplaceMenu(chara, skillNum,
                        (int slot) => { m_choiceresult = slot; DataManager.Instance.LogUIPlay(slot); },
                        () => { DataManager.Instance.LogUIPlay(-1); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.LearnMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Forget menu for a character to forget a skill.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the integer representing the chosen skill, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="chara">The character to relearn skills</param>
        public void ForgetMenu(Character chara)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                m_choiceresult = -1;
                m_curchoice = new SkillForgetMenu(chara,
                        (int slot) => { m_choiceresult = slot; DataManager.Instance.LogUIPlay(slot); },
                        () => { DataManager.Instance.LogUIPlay(-1); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ForgetMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Promote menu to choose a team member to promote.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the integer representing the chosen team slot, UI:ChoiceResult() must be called.
        /// </summary>
        public void ShowPromoteMenu()
        {
            try
            {
                m_choiceresult = -1;
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new PromoteMenu(-1,
                    (int teamSlot) => { m_choiceresult = teamSlot; },
                    () => { });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ShowPromoteMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool CanSwapMenu(LuaTable goods)
        {
            List<Tuple<string, string[]>> goodsList = new List<Tuple<string, string[]>>();
            foreach (object key in goods.Keys)
            {
                LuaTable entry = goods[key] as LuaTable;
                string item = (string)entry["Item"];
                List<string> reqs = new List<string>();
                LuaTable luaReqs = entry["ReqItem"] as LuaTable;
                foreach (object tradeIn in luaReqs.Values)
                    reqs.Add((string)tradeIn);
                goodsList.Add(new Tuple<string, string[]>(item, reqs.ToArray()));
            }
            return SwapShopMenu.CanView(goodsList);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void SwapMenu(LuaTable goods, LuaTable prices)
        {
            try
            {
                m_choiceresult = -1;
                List<Tuple<string, string[]>> goodsList = new List<Tuple<string, string[]>>();
                foreach (object key in goods.Keys)
                {
                    LuaTable entry = goods[key] as LuaTable;
                    string item = (string)entry["Item"];
                    List<string> reqs = new List<string>();
                    LuaTable luaReqs = entry["ReqItem"] as LuaTable;
                    foreach (object tradeIn in luaReqs.Values)
                        reqs.Add((string)tradeIn);
                    goodsList.Add(new Tuple<string, string[]>(item, reqs.ToArray()));
                }
                List<int> priceList = new List<int>();
                priceList.Add(0);
                foreach (object key in prices.Keys)
                {
                    int price = (int)((Int64)prices[key]);
                    priceList.Add(price);
                }
                m_curchoice = new SwapShopMenu(goodsList, priceList.ToArray(), 0, (chosenGood) =>
                {
                    m_choiceresult = chosenGood+1;
                });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SwapMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void TributeMenu(int spaces)
        {
            try
            {
                m_choiceresult = LuaEngine.Instance.RunString("return {}").First() as LuaTable;

                SwapGiveMenu menu = null;
                menu = new SwapGiveMenu(0, spaces, (List<int> chosenGoods) =>
                {
                    LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, val) table.insert(tbl, val) end").First() as LuaFunction;
                    foreach (int chosenGood in chosenGoods)
                        addfn.Call(m_choiceresult, menu.AllowedGoods[chosenGood]);
                });
                m_curchoice = menu;
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TributeMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Music menu to browse music for the game.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the string representing the chosen song, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="spoilerUnlocks">A lua table of strings representing progression flags that have been completed.
        /// Any ogg file that uses this tag as a spoiler tag will display in the menu only if the flag has been passed.</param>
        public void ShowMusicMenu(LuaTable spoilerUnlocks)
        {
            try
            {
                List<string> unlockedTags = new List<string>();
                foreach (object key in spoilerUnlocks.Keys)
                {
                    string entry = (string)spoilerUnlocks[key];
                    unlockedTags.Add(entry);
                }

                m_choiceresult = null;
                m_curchoice = new MusicMenu(unlockedTags, (string dir) => { m_choiceresult = dir; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ShowMusicMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Ask to enter a destintion via character dialogue to the player.
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the integer value indicating the result of the menu, UI:ChoiceResult() must be called.
        ///
        /// The Yes/No menu returns 1 for yes, and 0 for no.
        /// </summary>
        /// <param name="name">Name of the destination</param>
        /// <param name="dest">The ZoneLoc location of the destination.</param>
        public void DungeonChoice(string name, ZoneLoc dest)
        {
            try
            {
                m_choiceresult = null;
                ZoneData zoneEntry = DataManager.Instance.GetZone(dest.ID);
                DialogueChoice[] choices = new DialogueChoice[2];
                choices[0] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_YES"), () => { m_choiceresult = true; });
                choices[1] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_NO"), () => { m_choiceresult = false; });
                m_curchoice = new DungeonEnterDialog(Text.FormatKey("DLG_ASK_ENTER_DUNGEON", name), name, dest, false, choices, 0, 1);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.DungeonMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Marks the start of a choice menu for choosing destinations, showing a preview of restrictions and requirements for dungeons.
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the ZoneLoc indicating the chosen destination, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="destinations">A lua table representing the list of destinations with each element in the format of { Name=string, Dest=ZoneLoc }</param>
        public void DestinationMenu(LuaTable destinations)
        {
            try
            {
                List<string> names = new List<string>();
                List<string> titles = new List<string>();
                List<ZoneLoc> dests = new List<ZoneLoc>();
                foreach (object key in destinations.Keys)
                {
                    LuaTable entry = destinations[key] as LuaTable;
                    string name = (string)entry["Name"];
                    string title = entry["Title"] != null ? (string)entry["Title"] : name;
                    ZoneLoc item = (ZoneLoc)entry["Dest"];
                    names.Add(name);
                    titles.Add(title);
                    dests.Add(item);
                }

                //give the player the choice between all the possible dungeons
                m_choiceresult = ZoneLoc.Invalid;
                m_curchoice = new DungeonsMenu(names, titles, dests,
                    (int choice) => { m_choiceresult = dests[choice]; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.DungeonMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


        /// <summary>
        /// TODO
        /// </summary>
        public void ServersMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new ServersMenu();
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ServersMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void ContactsMenu()
        {
            try
            {
                m_choiceresult = 0;
                m_curchoice = new ContactsMenu(false, (OnlineActivity activity) => { m_choiceresult = 1; NetworkManager.Instance.PrepareActivity(activity); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ContactsMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void SOSMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new MailMenu(true, (string fileName) => { m_choiceresult = fileName; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SOSMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void AOKMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new MailMenu(false, (string fileName) => { });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.AOKMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void PeersMenu()
        {
            try
            {
                m_choiceresult = 0;
                m_curchoice = new PeersMenu((OnlineActivity activity) => { m_choiceresult = 1; NetworkManager.Instance.PrepareActivity(activity/*, true*/); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.PeersMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void ShowConnectMenu()
        {
            try
            {
                m_choiceresult = 0;
                m_curchoice = new ConnectingMenu(() => { m_choiceresult = 1; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ConnnectMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void CurrentActivityMenu()
        {
            try
            {
                m_choiceresult = "";
                switch (NetworkManager.Instance.Activity.Activity)
                {
                    case ActivityType.TradeTeam:
                        m_curchoice = new TradeTeamMenu(0);
                        break;
                    case ActivityType.TradeItem:
                        m_curchoice = new TradeItemMenu(0);
                        break;
                    case ActivityType.SendHelp:
                        m_curchoice = new SendHelpMenu();
                        break;
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.CurrentActivityMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


        /// <summary>
        /// Marks the start of a choice menu for choosing monsters, showing a preview of their appearances via portrait.
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the string indicating the chosen species, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="title">The title of the menu</param>
        /// <param name="choices">A lua table of choices with each element being a MonsterID.</param>
        /// <param name="canMenu">If set to true, the Menu Button exits the menu if pressed.</param>
        /// <param name="canCancel">If set to true, the Cancel Button exits the menu if pressed.</param>
        public void ChooseMonsterMenu(string title, LuaTable choices, bool canMenu = false, bool canCancel = false)
        {
            try
            {
                m_choiceresult = null;

                List<StartChar> monsters = new List<StartChar>();

                for (int ii = 1; choices[ii] is not null; ii++)
                {
                    var choice = choices[ii];
                    if (choice is MonsterID monster)
                        monsters.Add(new StartChar(monster, ""));
                    else
                        throw new ArgumentException($"Table must be array of '{nameof(MonsterID)}'", nameof(choices));
                }

                if (monsters.Count == 0)
                    throw new ArgumentException($"Table must be array of one or more '{nameof(MonsterID)}'", nameof(choices));

                void chooseAction(int slot)
                {
                    m_choiceresult = monsters[slot].ID;
                    MenuManager.Instance.RemoveMenu();
                }

                void cancelAction() { }

                m_curchoice = new ChooseMonsterMenu(title, monsters, 0, chooseAction, canCancel ? cancelAction : null, canMenu);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ChooseMonsterMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


        /// <summary>
        /// Displays a custom menu of the caller's choice.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the menu is exited.
        /// </summary>
        public void SetCustomMenu(InteractableMenu menu)
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = menu;
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SetCustomMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Marks the start of a multi-choice menu.
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the integer value indicating the result of the menu, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="message">The question to ask the user.</param>
        /// <param name="choicesPairs">A table of choices.  Each choice can be either a string, or { string, bool } representing the text and enabled status.</param>
        /// <param name="defaultChoice">The cursor starts on this choice.</param>
        /// <param name="cancelChoice">This choice is chosen if the player presses the cancel button.</param>
        /// <param name="callbacks">The Lua table of callbacks for the textbox to call.</param>
        public void BeginChoiceMenu(string message, LuaTable choicesPairs, object defaultChoice, object cancelChoice, LuaTable callbacks = null)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                object[] scripts = DialogueBox.CreateScripts(callbacks);
                m_choiceresult = null;
                int? mappedDefault = null;
                int? mappedCancel = null;
                //Intepret the choices from lua
                List<DialogueChoice> choices = new List<DialogueChoice>();
                IDictionaryEnumerator dict = choicesPairs.GetEnumerator();
                while (dict.MoveNext())
                {
                    string choicetext = "";
                    bool enabled = true;
                    if (dict.Value is string)
                        choicetext = dict.Value as string;
                    else if (dict.Value is LuaTable)
                    {
                        LuaTable tbl = dict.Value as LuaTable;
                        choicetext = (string)tbl[1];
                        enabled = (bool)tbl[2];
                    }
                    long choiceval = (long)dict.Key;

                    if (defaultChoice.Equals(choiceval))
                        mappedDefault = choices.Count;
                    if (cancelChoice.Equals(choiceval))
                        mappedCancel = choices.Count;
                    choices.Add(new DialogueChoice(choicetext, () => { m_choiceresult = choiceval; DataManager.Instance.LogUIPlay((int)choiceval); }, enabled));
                }

                if (mappedDefault == null)
                    mappedDefault = 0;
                if (mappedCancel == null)
                    mappedCancel = -1;

                //Make a choice menu, and check if we display a speaker or not
                if (m_curspeakerName != null)
                {
                    m_curchoice = MenuManager.Instance.CreateMultiQuestion(m_curspeakerID, m_curspeakerName, m_curspeakerEmo, m_curspeakerLoc, 
                            message, m_curspeakerSnd, m_curautoFinish, m_curcenter_h, m_curcenter_v, m_curbounds, scripts, m_curchoiceLoc, choices.ToArray(), mappedDefault.Value, mappedCancel.Value);
                }
                else
                {
                    m_curchoice = MenuManager.Instance.CreateMultiQuestion(MonsterID.Invalid, null, new EmoteStyle(0), m_curspeakerLoc,
                            message, m_curspeakerSnd, m_curautoFinish, m_curcenter_h, m_curcenter_v, m_curbounds, scripts, m_curchoiceLoc, choices.ToArray(), mappedDefault.Value, mappedCancel.Value);
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.BeginChoiceMenu({0}): Encountered exception.", message), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Marks the start of a multi-paged choice menu.
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the integer value indicating the result of the menu, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="x">X position of the menu</param>
        /// <param name="y">Y position of the menu</param>
        /// <param name="width">Width of the menu</param>
        /// <param name="title">Height of the menu</param>
        /// <param name="choicesPairs">A table of choices.  Each choice can be either a string, or { string, bool } representing the text and enabled status.</param>
        /// <param name="linesPerPage">Number of choices per page</param>
        /// <param name="defaultChoice">The cursor starts on this choice.</param>
        /// <param name="cancelChoice">This choice is chosen if the player presses the cancel button.</param>
        public void BeginMultiPageMenu(int x, int y, int width, string title, LuaTable choicesPairs, int linesPerPage, object defaultChoice, object cancelChoice)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                m_choiceresult = null;
                int? mappedDefault = null;
                int? mappedCancel = null;
                //Intepret the choices from lua
                List<MenuTextChoice> choices = new List<MenuTextChoice>();
                IDictionaryEnumerator dict = choicesPairs.GetEnumerator();
                while (dict.MoveNext())
                {
                    string choicetext = "";
                    bool enabled = true;
                    if (dict.Value is string)
                        choicetext = dict.Value as string;
                    else if (dict.Value is LuaTable)
                    {
                        LuaTable tbl = dict.Value as LuaTable;
                        choicetext = (string)tbl[1];
                        enabled = (bool)tbl[2];
                    }
                    long choiceval = (long)dict.Key;

                    if (defaultChoice.Equals(choiceval))
                        mappedDefault = choices.Count;
                    if (cancelChoice.Equals(choiceval))
                        mappedCancel = choices.Count;

                    choices.Add(new MenuTextChoice(choicetext, () => { MenuManager.Instance.RemoveMenu(); m_choiceresult = choiceval; DataManager.Instance.LogUIPlay((int)choiceval); }, enabled, enabled ? Color.White : Color.Red));
                }

                if (mappedDefault == null)
                    mappedDefault = 0;

                Action cancelAction = null;
                if (mappedCancel != null)
                    cancelAction = () => { MenuManager.Instance.RemoveMenu(); m_choiceresult = (int)(long)cancelChoice; DataManager.Instance.LogUIPlay((int)(long)cancelChoice); };

                //Make a choice menu, and check if we display a speaker or not
                m_curchoice = new CustomMultiPageMenu(new RogueElements.Loc(x, y), width, title, choices.ToArray(), mappedDefault.Value, linesPerPage, cancelAction, cancelAction);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.BeginMultiPageMenu({0}): Encountered exception.", title), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Get the result of the last choice menu
        /// </summary>
        /// <returns>The result of the choice</returns>
        public object ChoiceResult()
        {
            return m_choiceresult;
        }

        /// <summary>
        /// It's complicated.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Action GetChoiceAction(object obj)
        {
            return () =>
            {
                MenuManager.Instance.RemoveMenu();
                m_choiceresult = obj;
            };
        }


        /// <summary>
        /// Displays the currently set choice menu and waits for the player's selection to complete.
        /// </summary>
        /// <example>
        /// UI:WaitForChoice()
        /// </example>
        public LuaFunction WaitForChoice;

        public Coroutine _WaitForChoice()
        {
            if (DataManager.Instance.CurrentReplay != null)
                return new Coroutine(_DummyWait());

            if (m_curchoice != null)
                return new Coroutine(__WaitForChoice());
            else
                return new Coroutine(_DummyWait());

        }

        /// <summary>
        /// Wait for choice and then CLEAN UP m_curchoice
        /// </summary>
        /// <returns></returns>
        private IEnumerator<YieldInstruction> __WaitForChoice()
        {
            yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(m_curchoice));
            _m_curchoice = null;
        }

        //================================================================
        // Menu Utilities
        //================================================================
        public Coroutine ProcessMenuCoroutine( object menu )
        {
            try
            {
                IInteractable imenu = (IInteractable)menu;
                return new Coroutine(MenuManager.Instance.ProcessMenuCoroutine(imenu));
            }
            catch(Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                return null;
            }
        }

        //
        // Function setup
        //
        public override void SetupLuaFunctions(LuaEngine state)
        {
            WaitDialog = state.RunString(@"
            return function(_)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitDialog").First() as LuaFunction;

            WaitForChoice = state.RunString(@"
            return function(_)
                return coroutine.yield(UI:_WaitForChoice())
            end", "WaitForChoice").First() as LuaFunction;

            WaitShowDialogue = state.RunString(@"
            return function(_, text, callbacks)
                UI:TextDialogue(text, -1, callbacks)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowDialogue").First() as LuaFunction;

            WaitShowTimedDialogue = state.RunString(@"
            return function(_, text, time, callbacks)
                UI:TextDialogue(text, time, callbacks)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowDialogue").First() as LuaFunction;

            WaitShowVoiceOver = state.RunString(@"
            return function(_, text, expiretime, x, y, width, height, callbacks)
                x = x == nil and -1 or x
                y = y == nil and -1 or y
                width = width == nil and -1 or width
                height = height == nil and -1 or height
                UI:TextVoiceOver(text, expiretime, x, y, width, height, callbacks)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowVoiceOver").First() as LuaFunction;

            WaitInput = state.RunString(@"
            return function(_, any)
                UI:EmptyWaitMenu(any)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitInput").First() as LuaFunction;

            WaitShowTitle = state.RunString(@"
            return function(_, text, time)
                UI:TextShowTitle(text, time)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowTitle").First() as LuaFunction;

            WaitHideTitle = state.RunString(@"
            return function(_, time)
                UI:TextFadeTitle(time)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitHideTitle").First() as LuaFunction;

            WaitShowBG = state.RunString(@"
            return function(_, text, frameTime, fadeInTime)
                UI:ShowBG(text, frameTime, fadeInTime)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowBG").First() as LuaFunction;

            WaitHideBG = state.RunString(@"
            return function(_, time)
                UI:FadeBG(time)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitHideBG").First() as LuaFunction;
        }
    }
}
