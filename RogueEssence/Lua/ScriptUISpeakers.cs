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
    public partial class ScriptUI : ILuaEngineComponent
    {


        /// <summary>
        /// Exports the speaker settings as a lua table.
        /// </summary>
        /// <returns></returns>
        public LuaTable ExportSpeakerSettings()
        {
            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, key, itm) tbl[key] = itm end").First() as LuaFunction;
            addfn.Call(tbl, "SpeakerID", m_curspeakerID);
            addfn.Call(tbl, "SpeakerName", m_curspeakerName);
            addfn.Call(tbl, "TextCenterH", m_curcenter_h);
            addfn.Call(tbl, "TextCenterV", m_curcenter_v);
            addfn.Call(tbl, "AutoFinish", m_curautoFinish);
            addfn.Call(tbl, "SpeakerEmotion", m_curspeakerEmo);
            addfn.Call(tbl, "SpeakerSound", m_curspeakerSnd);
            addfn.Call(tbl, "SpeakerSE", m_curspeakerSe);
            addfn.Call(tbl, "SpeakTime", m_curspeakTime);
            addfn.Call(tbl, "TextBounds", m_curbounds);
            addfn.Call(tbl, "SpeakerLoc", m_curspeakerLoc);
            addfn.Call(tbl, "ChoiceLoc", m_curchoiceLoc);
            return tbl;
        }

        /// <summary>
        /// Imports speaker settings from a lua table.
        /// </summary>
        /// <param name="tbl"></param>
        public void ImportSpeakerSettings(LuaTable tbl)
        {
            m_curspeakerID = (MonsterID)tbl["SpeakerID"];
            m_curspeakerName = (string)tbl["SpeakerName"];
            m_curcenter_h = (bool)tbl["TextCenterH"];
            m_curcenter_v = (bool)tbl["TextCenterV"];
            m_curautoFinish = (bool)tbl["AutoFinish"];
            m_curspeakerEmo = (EmoteStyle)tbl["SpeakerEmotion"];
            m_curspeakerSnd = (bool)tbl["SpeakerSound"];
            m_curspeakerSe = (string)tbl["SpeakerSE"];
            m_curspeakTime = (int)(long)tbl["SpeakTime"];
            m_curbounds = (Rect)tbl["TextBounds"];
            m_curspeakerLoc = (Loc)tbl["SpeakerLoc"];
            m_curchoiceLoc = (Loc)tbl["ChoiceLoc"];
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
            m_curspeakerSe = DialogueBox.SOUND_EFFECT;
            m_curspeakTime = DialogueBox.SPEAK_FRAMES;
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
        /// <param name="keysound">If set to true, plays sounds when text appears.</param>
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
        /// Sets the speaker to be displayed during the following calls to the TextDialogue functions.
        /// It takes an existing GroundChar as a parameter.
        /// It resets speaker emotion.
        /// </summary>
        /// <param name="chara">Character to set as speaker. This is a character in a ground map.</param>
        /// <param name="keysound">If set to true, plays sounds when text appears.</param>
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
        /// Sets the speaker to be displayed during the following calls to the TextDialogue functions.
        /// It takes an existing Character as a parameter.
        /// It resets speaker emotion.
        /// </summary>
        /// <param name="chara">Character to set as speaker. This is a character in a dungeon map.</param>
        /// <param name="keysound">If set to true, plays sounds when text appears.</param>
        public void SetSpeaker(Character chara, bool keysound = true)
        {
            if (chara != null)
            {
                m_curspeakerID = chara.Appearance;
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
        /// Makes the text automatically finish when it shows up.
        /// </summary>
        /// <param name="autoFinish">Auto-finishes text if true.</param>
        public void SetAutoFinish(bool autoFinish)
        {
            m_curautoFinish = autoFinish;
        }



        /// <summary>
        /// Sets the speaker sound effect and speak frames played in the TextDialogue functions. 
        /// </summary>
        /// <param name="newSe">The sound effect of the box</param>
        /// <param name="speakTime">The amount of frames to wait between each sound effect</param>
        /// <example>
        /// UI:SetSe("Battle/_UNK_DUN_Water_Drop", 3)
        /// </example>
        public void SetSe(string newSe, int speakTime)
        {
            m_curspeakerSe = newSe;
            m_curspeakTime = speakTime;
        }

        /// <summary>
        /// Sets the speaker sound effect played in the TextDialogue functions. 
        /// </summary>
        /// <param name="newSe">The sound effect of the box</param>
        /// <example>
        /// UI:SetSe("Menu/Unknown-3")
        /// </example>
        public void SetSe(string newSe)
        {
            m_curspeakerSe = newSe;
        }

        /// <summary>
        /// Sets the speak frames played in the TextDialogue functions. 
        /// </summary>
        /// <param name="speakTime">The amount of frames to wait between each sound effect</param>
        /// <example>
        /// UI:SetSpeakTime(10)
        /// </example>
        public void SetSpeakTime(int speakTime)
        {
            m_curspeakTime = speakTime;
        }

        /// <summary>
        /// Resets to the default speaker sound effect and speaker frames.
        /// </summary>
        public void ResetSe()
        {
            m_curspeakerSe = DialogueBox.SOUND_EFFECT;
            m_curspeakTime = DialogueBox.SPEAK_FRAMES;
        }


        /// <summary>
        /// Displays the currently set dialogue box and waits for the player to complete it.
        /// </summary>
        /// <example>
        /// UI:WaitDialog()
        /// </example>
        public LuaFunction WaitDialog;

        /// <summary>
        /// [LuaFunction] WaitDialog
        /// </summary>
        public Coroutine _WaitDialog()
        {
            if (DataManager.Instance.CurrentReplay != null)
                return new Coroutine(_DummyWait());

            return new Coroutine(m_curdialogue);
        }

        /// <summary>
        /// Instantly break. Used as default/invalid value when returning a yieldable value.
        /// </summary>
        private IEnumerator<YieldInstruction> _DummyWait()
        {
            yield break;
        }


    }
}
