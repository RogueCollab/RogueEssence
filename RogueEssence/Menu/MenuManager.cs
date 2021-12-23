using System;
using System.Collections.Generic;
using RogueEssence.Data;
using RogueEssence.Content;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Dungeon;
using System.Text.RegularExpressions;

namespace RogueEssence.Menu
{
    public class MenuManager
    {
        public IEnumerator<YieldInstruction> NextAction;
        public IEnumerator<YieldInstruction> EndAction;

        private int menuModeDepth;

        private static MenuManager instance;
        public static void InitInstance()
        {
            instance = new MenuManager();
        }
        public static MenuManager Instance { get { return instance; } }

        private List<IInteractable> menus;

        public int MenuCount { get { return menus.Count; } }

        public MenuManager()
        {
            menus = new List<IInteractable>();
        }

        public void AddMenu(IInteractable menu, bool stackOn)
        {
            if (menuModeDepth == 0)
                throw new Exception("Can't add menu while not in menu mode");

            if (menus.Count > 0)
                menus[menus.Count - 1].Inactive = true;
            menu.BlockPrevious = !stackOn;
            menus.Add(menu);
        }

        public void ReplaceMenu(IInteractable menu)
        {
            if (menuModeDepth == 0)
                throw new Exception("Can't replace menu while not in menu mode");

            menu.BlockPrevious = menus[menus.Count - 1].BlockPrevious;
            menus.RemoveAt(menus.Count - 1);
            menus.Add(menu);
        }

        public void RemoveMenu()
        {
            if (menuModeDepth == 0)
                throw new Exception("Can't remove menu while not in menu mode");

            menus.RemoveAt(menus.Count - 1);
            if (menus.Count > 0)
                menus[menus.Count - 1].Inactive = false;
        }

        public void ClearMenus()
        {
            menus.Clear();
        }

        public void ClearToCheckpoint()
        {
            menus.RemoveAt(menus.Count - 1);
            while (menus.Count > 0 && menus[menus.Count-1].IsCheckpoint)
                menus.RemoveAt(menus.Count - 1);
        }

        public IEnumerator<YieldInstruction> ProcessMenuCoroutine(IInteractable menu)
        {
            if (menus.Count > 0)
                menus[menus.Count - 1].Inactive = true;

            menu.BlockPrevious = true;
            menus.Add(menu);
            yield return CoroutineManager.Instance.StartCoroutine(ProcessMenuCoroutine());
        }


        public IEnumerator<YieldInstruction> ProcessMenuCoroutine()
        {
            yield return CoroutineManager.Instance.StartCoroutine(processInternalCoroutine());
        }

        public List<IInteractable> SaveMenuState()
        {
            List<IInteractable> save = new List<IInteractable>();
            save.AddRange(menus);
            return save;
        }

        public void LoadMenuState(List<IInteractable> save)
        {
            if (menus.Count > 0)
                throw new Exception("Must load menus from empty.");
            menus.AddRange(save);
        }

        public void DrawMenus(SpriteBatch spriteBatch)
        {
            int startIndex = menus.Count;
            for (int ii = menus.Count - 1; ii >= 0; ii--)
            {
                startIndex = ii;
                if (menus[ii].BlockPrevious)
                    break;
            }
            for (int ii = startIndex; ii < menus.Count; ii++)
            {
                if (menus[ii].Visible)
                    menus[ii].Draw(spriteBatch);
            }
        }


        public IEnumerator<YieldInstruction> SetSign(params string[] msgs)
        {
            return SetDialogue(MonsterID.Invalid, null, new EmoteStyle(0), true, () => { }, -1, true, false, false, msgs);
        }
        public IEnumerator<YieldInstruction> SetDialogue(params string[] msgs)
        {
            return SetDialogue(MonsterID.Invalid, null, new EmoteStyle(0), true, msgs);
        }
        public IEnumerator<YieldInstruction> SetDialogue(Action finishAction, params string[] msgs)
        {
            return SetDialogue(MonsterID.Invalid, null, new EmoteStyle(0), true, finishAction, -1, false, false, false, msgs);
        }
        public IEnumerator<YieldInstruction> SetDialogue(bool sound, params string[] msgs)
        {
            return SetDialogue(MonsterID.Invalid, null, new EmoteStyle(0), sound, msgs);
        }
        public IEnumerator<YieldInstruction> SetDialogue(MonsterID speaker, string speakerName, EmoteStyle emotion, bool sound, params string[] msgs)
        {
            return SetDialogue(speaker, speakerName, emotion, sound, () => { }, -1, false, false, false, msgs);
        }
        public IEnumerator<YieldInstruction> SetDialogue(MonsterID speaker, string speakerName, EmoteStyle emotion, bool sound, Action finishAction, int waitTime, bool autoFinish, bool centerH, bool centerV, params string[] msgs)
        {
            DialogueBox box = CreateDialogue(speaker, speakerName, emotion, sound, finishAction, waitTime, autoFinish, centerH, centerV, msgs);
            yield return CoroutineManager.Instance.StartCoroutine(ProcessMenuCoroutine(box));
        }

        public IEnumerator<YieldInstruction> SetTitleDialog(int holdTime, bool fadeIn, Action finishAction, params string[] msgs)
        {
            TitleDialog box = CreateTitleDialog(holdTime, fadeIn, finishAction, msgs);
            yield return CoroutineManager.Instance.StartCoroutine(ProcessMenuCoroutine(box));
        }

        public IEnumerator<YieldInstruction> SetWaitMenu(bool anyInput)
        {
            WaitMenu box = new WaitMenu(anyInput);
            yield return CoroutineManager.Instance.StartCoroutine(ProcessMenuCoroutine(box));
        }

        public DialogueBox CreateDialogue(params string[] msgs)
        {
            return CreateDialogue(MonsterID.Invalid, null, new EmoteStyle(0), true, msgs);
        }
        public DialogueBox CreateDialogue(Action finishAction, params string[] msgs)
        {
            return CreateDialogue(MonsterID.Invalid, null, new EmoteStyle(0), true, finishAction, -1, false, false, false, msgs);
        }
        public DialogueBox CreateDialogue(bool sound, params string[] msgs)
        {
            return CreateDialogue(MonsterID.Invalid, null, new EmoteStyle(0), sound, msgs);
        }
        public DialogueBox CreateDialogue(MonsterID speaker, string speakerName, EmoteStyle emotion, bool sound, params string[] msgs)
        {
            return CreateDialogue(speaker, speakerName, emotion, sound, () => { }, -1, false, false, false, msgs);
        }
        public DialogueBox CreateDialogue(MonsterID speaker, string speakerName, EmoteStyle emotion, bool sound, Action finishAction, int waitTime, bool autoFinish, bool centerH, bool centerV, params string[] msgs)
        {
            if (msgs.Length > 0)
            {
                List<string> sep_msgs = new List<string>();
                for (int ii = 0; ii < msgs.Length; ii++)
                {
                    string[] break_str = Regex.Split(msgs[ii], @"\[br\]", RegexOptions.IgnoreCase);
                    sep_msgs.AddRange(break_str);
                }
                DialogueBox box = null;
                for (int ii = sep_msgs.Count - 1; ii >= 0; ii--)
                {
                    DialogueBox prevBox = box;
                    box = CreateBox(speaker, speakerName, emotion, sound, (prevBox == null) ? finishAction : () => { AddMenu(prevBox, false); }, waitTime, autoFinish, centerH, centerV, sep_msgs[ii]);
                }
                return box;
            }
            return null;
        }

        public DialogueBox CreateBox(MonsterID speaker, string speakerName, EmoteStyle emotion, bool sound,
             Action finishAction, int waitTime, bool autoFinish, bool centerH, bool centerV, string msg)
        {
            DialogueBox box = null;
            if (waitTime > -1)
                box = new TimedDialog(msg, sound, centerH, centerV, waitTime, finishAction);
            else
                box = new ClickedDialog(msg, sound, centerH, centerV, finishAction);
            box.SetSpeaker(speaker, speakerName, emotion);
            if (autoFinish)
                box.FinishText();
            return box;
        }

        public TitleDialog CreateTitleDialog(int holdTime, bool fadeIn, Action finishAction, params string[] msgs)
        {
            if (msgs.Length > 0)
            {
                List<string> sep_msgs = new List<string>();
                for (int ii = 0; ii < msgs.Length; ii++)
                {
                    string[] break_str = Regex.Split(msgs[ii], @"\[br\]", RegexOptions.IgnoreCase);
                    sep_msgs.AddRange(break_str);
                }
                TitleDialog box = null;
                for (int ii = sep_msgs.Count - 1; ii >= 0; ii--)
                {
                    TitleDialog prevBox = box;
                    box = new TitleDialog(sep_msgs[ii], fadeIn, holdTime, (prevBox == null) ? finishAction : () => { AddMenu(prevBox, false); });
                }
                return box;
            }
            return null;
        }

        public DialogueBox CreateQuestion(string message, Action yes, Action no)
        {
            return CreateQuestion(MonsterID.Invalid, null, new EmoteStyle(0), message, true, false, false, false, yes, no, false);
        }
        public DialogueBox CreateQuestion(string message, bool sound, Action yes, Action no)
        {
            return CreateQuestion(MonsterID.Invalid, null, new EmoteStyle(0), message, sound, false, false, false, yes, no, false);
        }
        public DialogueBox CreateQuestion(string message, bool sound, Action yes, Action no, bool defaultNo)
        {
            return CreateQuestion(MonsterID.Invalid, null, new EmoteStyle(0), message, sound, false, false, false, yes, no, defaultNo);
        }

        public DialogueBox CreateQuestion(MonsterID speaker, string speakerName, EmoteStyle emotion,
            string msg, bool sound, bool autoFinish, bool centerH, bool centerV, Action yes, Action no, bool defaultNo)
        {
            string[] break_str = Regex.Split(msg, "\\[br\\]", RegexOptions.IgnoreCase);

            DialogueChoice[] choices = new DialogueChoice[2];
            choices[0] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_YES"), yes);
            choices[1] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_NO"), no);

            DialogueBox box = new QuestionDialog(break_str[break_str.Length-1], sound, false, false, choices, defaultNo ? 1 : 0, 1);
            box.SetSpeaker(speaker, speakerName, emotion);
            if (autoFinish)
                box.FinishText();

            if (break_str.Length > 1)
            {
                string[] pre_str = new string[break_str.Length - 1];
                Array.Copy(break_str, pre_str, pre_str.Length);
                return CreateDialogue(speaker, speakerName, emotion, sound, () => { AddMenu(box, false); }, -1, autoFinish, centerH, centerV, pre_str);
            }
            else
                return box;
        }

        public DialogueBox CreateMultiQuestion(string message, bool sound, List<DialogueChoice> choices, int defaultChoice, int cancelChoice)
        {
            return CreateMultiQuestion(MonsterID.Invalid, null, new EmoteStyle(0), message, sound, false, false, false, choices.ToArray(), defaultChoice, cancelChoice);
        }

        public DialogueBox CreateMultiQuestion(string message, bool sound, DialogueChoice[] choices, int defaultChoice, int cancelChoice)
        {
            return CreateMultiQuestion(MonsterID.Invalid, null, new EmoteStyle(0), message, sound, false, false, false, choices, defaultChoice, cancelChoice);
        }

        public DialogueBox CreateMultiQuestion(MonsterID speaker, string speakerName, EmoteStyle emotion,
            string msg, bool sound, bool autoFinish, bool centerH, bool centerV, DialogueChoice[] choices, int defaultChoice, int cancelChoice)
        {
            string[] break_str = Regex.Split(msg, "\\[br\\]", RegexOptions.IgnoreCase);

            DialogueBox box = new QuestionDialog(break_str[break_str.Length - 1], sound, false, false, choices, defaultChoice, cancelChoice);
            box.SetSpeaker(speaker, speakerName, emotion);
            if (autoFinish)
                box.FinishText();

            if (break_str.Length > 1)
            {
                string[] pre_str = new string[break_str.Length - 1];
                Array.Copy(break_str, pre_str, pre_str.Length);
                return CreateDialogue(speaker, speakerName, emotion, sound, () => { AddMenu(box, false); }, -1, autoFinish, centerH, centerV, pre_str);
            }
            else
                return box;
        }

        private IEnumerator<YieldInstruction> processInternalCoroutine()
        {
            menuModeDepth++;
            while (MenuCount > 0)
            {
                yield return new WaitForFrames(1);
                ProcessMenus(GameManager.Instance.InputManager);

                if (NextAction != null)
                {
                    IEnumerator<YieldInstruction> action = NextAction;
                    NextAction = null;
                    yield return CoroutineManager.Instance.StartCoroutine(action);
                }
            }
            menuModeDepth--;

            if (EndAction != null)
            {
                IEnumerator<YieldInstruction> action = EndAction;
                EndAction = null;
                yield return CoroutineManager.Instance.StartCoroutine(action);
            }
            GameManager.Instance.FrameProcessed = false;
        }


        private void ProcessMenus(InputManager input)
        {
            try
            {
                //process most recent menu
                if (menus.Count > 0)
                    menus[menus.Count - 1].Update(input);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public void ProcessActions(FrameTick elapsedTime)
        {
            //process most recent menu
            if (menus.Count > 0)
                menus[menus.Count - 1].ProcessActions(elapsedTime);
        }

    }
}
