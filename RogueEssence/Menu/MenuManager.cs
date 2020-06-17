using System;
using System.Collections.Generic;
using RogueEssence.Data;
using RogueEssence.Content;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class MenuManager
    {
        public IEnumerator<YieldInstruction> NextAction;
        public IEnumerator<YieldInstruction> EndAction;

        private bool freeMode;

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
            if (!freeMode)
                throw new Exception("Can't add menu while in coroutine mode");

            if (menus.Count > 0)
                menus[menus.Count - 1].Inactive = true;
            menu.BlockPrevious = !stackOn;
            menus.Add(menu);
        }

        public void ReplaceMenu(IInteractable menu)
        {
            if (!freeMode)
                throw new Exception("Can't replace menu while in coroutine mode");

            menu.BlockPrevious = menus[menus.Count - 1].BlockPrevious;
            menus.RemoveAt(menus.Count - 1);
            menus.Add(menu);
        }

        public void RemoveMenu()
        {
            if (!freeMode)
                throw new Exception("Can't remove menu while in coroutine mode");

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
            yield return CoroutineManager.Instance.StartCoroutine(processInternalCoroutine());
        }

        public List<IInteractable> SaveMenuState()
        {
            List<IInteractable> save = new List<IInteractable>();
            save.AddRange(menus);
            return save;
        }

        public IEnumerator<YieldInstruction> LoadMenuState(List<IInteractable> save)
        {
            if (menus.Count > 0)
                throw new Exception("Must load menus from empty.");
            menus.AddRange(save);
            yield return CoroutineManager.Instance.StartCoroutine(processInternalCoroutine());
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
            if (msgs.Length > 0)
            {
                DialogueBox box = null;
                Action emptyAction = () => { };
                for (int ii = msgs.Length - 1; ii >= 0; ii--)
                {
                    DialogueBox prevBox = box;
                    box = new ClickedDialog(msgs[ii], false, (prevBox == null) ? emptyAction : () => { AddMenu(prevBox, false); });
                    box.Text.FinishText();
                }
                yield return CoroutineManager.Instance.StartCoroutine(ProcessMenuCoroutine(box));
            }
        }

        public IEnumerator<YieldInstruction> SetDialogue(params string[] msgs)
        {
            return SetDialogue(MonsterID.Invalid, null, new EmoteStyle(0), true, msgs);
        }
        public IEnumerator<YieldInstruction> SetDialogue(Action finishAction, params string[] msgs)
        {
            return SetDialogue(MonsterID.Invalid, null, new EmoteStyle(0), true, finishAction, -1, msgs);
        }
        public IEnumerator<YieldInstruction> SetDialogue(bool sound, params string[] msgs)
        {
            return SetDialogue(MonsterID.Invalid, null, new EmoteStyle(0), sound, msgs);
        }
        public IEnumerator<YieldInstruction> SetDialogue(MonsterID speaker, EmoteStyle emotion, params string[] msgs)
        {
            return SetDialogue(speaker, DataManager.Instance.GetMonster(speaker.Species).Name.ToLocal(), emotion, true, () => { }, -1, msgs);
        }
        public IEnumerator<YieldInstruction> SetDialogue(MonsterID speaker, string speakerName, EmoteStyle emotion, bool sound, params string[] msgs)
        {
            return SetDialogue(speaker, speakerName, emotion, sound, () => { }, -1, msgs);
        }
        public IEnumerator<YieldInstruction> SetDialogue(MonsterID speaker, string speakerName, EmoteStyle emotion, bool sound, Action finishAction, int waitTime, params string[] msgs)
        {
            DialogueBox box = CreateDialogue(speaker, speakerName, emotion, sound, finishAction, waitTime, msgs);
            yield return CoroutineManager.Instance.StartCoroutine(ProcessMenuCoroutine(box));
        }


        public DialogueBox CreateDialogue(params string[] msgs)
        {
            return CreateDialogue(MonsterID.Invalid, null, new EmoteStyle(0), true, msgs);
        }
        public DialogueBox CreateDialogue(Action finishAction, params string[] msgs)
        {
            return CreateDialogue(MonsterID.Invalid, null, new EmoteStyle(0), true, finishAction, -1, msgs);
        }
        public DialogueBox CreateDialogue(bool sound, params string[] msgs)
        {
            return CreateDialogue(MonsterID.Invalid, null, new EmoteStyle(0), sound, msgs);
        }
        public DialogueBox CreateDialogue(MonsterID speaker, string speakerName, EmoteStyle emotion, bool sound, params string[] msgs)
        {
            return CreateDialogue(speaker, speakerName, emotion, sound, () => { }, -1, msgs);
        }
        public DialogueBox CreateDialogue(MonsterID speaker, string speakerName, EmoteStyle emotion, bool sound, Action finishAction, int waitTime, params string[] msgs)
        {
            if (msgs.Length > 0)
            {
                DialogueBox box = null;
                for (int ii = msgs.Length - 1; ii >= 0; ii--)
                {
                    DialogueBox prevBox = box;
                    box = CreateBox(speaker, speakerName, emotion, sound, (prevBox == null) ? finishAction : () => { AddMenu(prevBox, false); }, waitTime, msgs[ii]);
                }
                return box;
            }
            return null;
        }

        public DialogueBox CreateBox(MonsterID speaker, string speakerName, EmoteStyle emotion, bool sound,
             Action finishAction, int waitTime, string msg)
        {
            DialogueBox box = null;
            if (waitTime > -1)
                box = new TimedDialog(msg, sound, waitTime, finishAction);
            else
                box = new ClickedDialog(msg, sound, finishAction);
            box.SetSpeaker(speaker, speakerName, emotion);
            return box;
        }

        public QuestionDialog CreateQuestion(string message, Action yes, Action no)
        {
            return CreateQuestion(MonsterID.Invalid, null, new EmoteStyle(0), message, true, yes, no, false);
        }
        public QuestionDialog CreateQuestion(string message, bool sound, Action yes, Action no)
        {
            return CreateQuestion(MonsterID.Invalid, null, new EmoteStyle(0), message, sound, yes, no, false);
        }
        public QuestionDialog CreateQuestion(string message, bool sound, Action yes, Action no, bool defaultNo)
        {
            return CreateQuestion(MonsterID.Invalid, null, new EmoteStyle(0), message, sound, yes, no, defaultNo);
        }

        public QuestionDialog CreateQuestion(MonsterID speaker, string speakerName, EmoteStyle emotion,
            string msg, bool sound, Action yes, Action no, bool defaultNo)
        {
            DialogueChoice[] choices = new DialogueChoice[2];
            choices[0] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_YES"), yes);
            choices[1] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_NO"), no);

            QuestionDialog box = new QuestionDialog(msg, sound, choices, defaultNo ? 1 : 0, 1);
            box.SetSpeaker(speaker, speakerName, emotion);
            return box;
        }

        public QuestionDialog CreateMultiQuestion(string message, bool sound, List<DialogueChoice> choices, int defaultChoice, int cancelChoice)
        {
            return CreateMultiQuestion(MonsterID.Invalid, null, new EmoteStyle(0), message, sound, choices.ToArray(), defaultChoice, cancelChoice);
        }

        public QuestionDialog CreateMultiQuestion(string message, bool sound, DialogueChoice[] choices, int defaultChoice, int cancelChoice)
        {
            return CreateMultiQuestion(MonsterID.Invalid, null, new EmoteStyle(0), message, sound, choices, defaultChoice, cancelChoice);
        }

        public QuestionDialog CreateMultiQuestion(MonsterID speaker, string speakerName, EmoteStyle emotion,
            string msg, bool sound, DialogueChoice[] choices, int defaultChoice, int cancelChoice)
        {
            QuestionDialog box = new QuestionDialog(msg, sound, choices, defaultChoice, cancelChoice);
            box.SetSpeaker(speaker, speakerName, emotion);
            return box;
        }

        private IEnumerator<YieldInstruction> processInternalCoroutine()
        {
            if (freeMode)
                yield break;

            freeMode = true;
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
            freeMode = false;

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
