using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System;
using RogueEssence.Script;

namespace RogueEssence.Menu
{
    public class MainStartingMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 12;

        public SpeakerPortrait Portrait;
        OnChooseSlot chooseAction;
        Action onCancel;

        public MainStartingMenu(int startIndex, OnChooseSlot chooseAction, Action onCancel)
        {
            this.chooseAction = chooseAction;
            this.onCancel = onCancel;
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DataManager.Instance.StartChars.Count; ii++)
            {
                MonsterID startChar = DataManager.Instance.StartChars[ii].mon;
                string name = DataManager.Instance.GetMonster(startChar.Species).GetColoredName();
                if (DataManager.Instance.StartChars[ii].name != "")
                    name = DataManager.Instance.StartChars[ii].name;
                int index = ii;
                flatChoices.Add(new MenuTextChoice(name, () => { this.chooseAction(index); }));
            }
            List<MenuChoice[]> box = SortIntoPages(flatChoices, SLOTS_PER_PAGE);

            int totalSlots = SLOTS_PER_PAGE;
            if (box.Count == 1)
                totalSlots = box[0].Length;


            startIndex = Math.Clamp(startIndex, 0, flatChoices.Count - 1);

            Portrait = new SpeakerPortrait(new MonsterID(), new EmoteStyle(0), new Loc(200, 32), true);

            Initialize(new Loc(16, 16), 112, Text.FormatKey("MENU_CHARA_CHOICE_TITLE"), box.ToArray(), 0, 0, totalSlots, false, -1);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            if (Portrait.Speaker.Species > 0)
                Portrait.Draw(spriteBatch, new Loc());
        }

        protected override void ChoiceChanged()
        {
            Portrait.Speaker = DataManager.Instance.StartChars[CurrentChoiceTotal].mon;
            base.ChoiceChanged();
        }

        protected override void Canceled()
        {
            MenuManager.Instance.RemoveMenu();
            onCancel();
        }


        public static void StartFlow(MonsterID monId, string name, int backPhase)
        {
            if (monId.Species == -1 || backPhase == 0)
            {
                if (DataManager.Instance.StartChars.Count > 1)
                {
                    int startIndex = 0;
                    if (backPhase == 0)
                        startIndex = DataManager.Instance.StartChars.FindIndex(start => start.mon == monId);
                    MenuManager.Instance.AddMenu(new MainStartingMenu(startIndex, (int index) =>
                    {
                        string newName = null;
                        if (DataManager.Instance.StartChars[index].name != "")
                            newName = DataManager.Instance.StartChars[index].name;
                        StartFlow(DataManager.Instance.StartChars[index].mon, newName, -1);
                    }, () => { }), false);
                    return;
                }
                else if (backPhase == 0)
                    return;
                else if (DataManager.Instance.StartChars.Count == 1)
                {
                    monId = DataManager.Instance.StartChars[0].mon;
                    if (DataManager.Instance.StartChars[0].name != "")
                        name = DataManager.Instance.StartChars[0].name;
                }
                else
                {
                    MenuManager.Instance.ClearMenus();
                    GameManager.Instance.SceneOutcome = DefaultBegin();
                    return;
                }
            }

            if (monId.Gender == Gender.Unknown || backPhase == 1)
            {
                MonsterData monEntry = DataManager.Instance.GetMonster(monId.Species);
                BaseMonsterForm form = monEntry.Forms[monId.Form];
                List<Gender> genders = form.GetPossibleGenders();
                if (genders.Count > 1)
                {
                    int startIndex = 0;
                    if (backPhase == 1)
                        startIndex = monId.Gender == Gender.Female ? 1 : 0;
                    List<DialogueChoice> choices = new List<DialogueChoice>();
                    choices.Add(new DialogueChoice(Text.FormatKey("MENU_BOY"), () =>
                    {
                        StartFlow(new MonsterID(monId.Species, monId.Form, monId.Skin, Gender.Male), name, -1);
                    }));
                    choices.Add(new DialogueChoice(Text.FormatKey("MENU_GIRL"), () =>
                    {
                        StartFlow(new MonsterID(monId.Species, monId.Form, monId.Skin, Gender.Female), name, -1);
                    }));
                    choices.Add(new DialogueChoice(Text.FormatKey("MENU_CANCEL"), () =>
                    { }));
                    MenuManager.Instance.AddMenu(MenuManager.Instance.CreateMultiQuestion(Text.FormatKey("DLG_ASK_GENDER"), false, choices, startIndex, choices.Count - 1), false);
                    return;
                }
                else if (backPhase == 1)
                {
                    StartFlow(new MonsterID(monId.Species, monId.Form, monId.Skin, Gender.Unknown), name, 0);
                    return;
                }
                else
                    monId.Gender = genders[0];
            }

            if (name == null)
            {
                MenuManager.Instance.AddMenu(new NicknameMenu((string name) =>
                {
                    StartFlow(monId, name, -1);
                }, () =>
                {
                    StartFlow(monId, null, 1);
                }), false);
                return;
            }

            //begin

            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = Begin(monId, name);
        }


        public static IEnumerator<YieldInstruction> Begin(MonsterID monId, string name)
        {
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            DataManager.Instance.SetProgress(new MainProgress(MathUtils.Rand.NextUInt64(), Guid.NewGuid().ToString().ToUpper()));
            DataManager.Instance.Save.StartDate = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            DataManager.Instance.Save.ActiveTeam = new ExplorerTeam();

            Character newChar = DataManager.Instance.Save.ActiveTeam.CreatePlayer(MathUtils.Rand, monId, DataManager.Instance.StartLevel, -1, DataManager.Instance.StartPersonality);
            newChar.Nickname = name;
            newChar.IsFounder = true;
            DataManager.Instance.Save.ActiveTeam.Players.Add(newChar);
            DataManager.Instance.Save.RegisterMonster(DataManager.Instance.Save.ActiveTeam.Players[0].BaseForm.Species);

            try
            {
                LuaEngine.Instance.OnNewGame();
                if (DataManager.Instance.Save.ActiveTeam.Players.Count == 0)
                    throw new Exception("Script generated an invalid team!");
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }

            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.StartMap));
        }

        public static IEnumerator<YieldInstruction> DefaultBegin()
        {
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            GameManager.Instance.NewGamePlus(MathUtils.Rand.NextUInt64());

            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.StartMap));
        }
    }
}
