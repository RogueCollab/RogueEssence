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
    public class MainStartingMenu : TitledStripMenu
    {
        public delegate void OnChooseChar(MonsterID chosenID);
        public SpeakerPortrait Portrait;
        OnChooseChar chooseAction;
        Action onCancel;

        public MainStartingMenu(int startIndex, OnChooseChar chooseAction, Action onCancel)
        {
            this.chooseAction = chooseAction;
            this.onCancel = onCancel;
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DataManager.Instance.StartChars.Count; ii++)
            {
                MonsterID startChar = DataManager.Instance.StartChars[ii];
                flatChoices.Add(new MenuTextChoice(DataManager.Instance.GetMonster(startChar.Species).Name.ToLocal(), () => { this.chooseAction(startChar); }));
            }

            startIndex = Math.Clamp(startIndex, 0, flatChoices.Count - 1);

            Portrait = new SpeakerPortrait(new MonsterID(), new EmoteStyle(0), new Loc(200, 32), true);

            Initialize(new Loc(16, 16), 112, Text.FormatKey("MENU_CHARA_CHOICE_TITLE"), flatChoices.ToArray(), startIndex);
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
            Portrait.Speaker = DataManager.Instance.StartChars[CurrentChoice];
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
                        startIndex = DataManager.Instance.StartChars.IndexOf(monId);
                    MenuManager.Instance.AddMenu(new MainStartingMenu(startIndex, (MonsterID chosenID) =>
                    {
                        StartFlow(chosenID, name, -1);
                    }, () => { }), false);
                    return;
                }
                else if (backPhase == 0)
                    return;
                else if (DataManager.Instance.StartChars.Count == 1)
                    monId = DataManager.Instance.StartChars[0];
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
                    {
                        StartFlow(new MonsterID(monId.Species, monId.Form, monId.Skin, Gender.Unknown), name, 0);
                    }));
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

            if (name == "")
            {
                if (DataManager.Instance.StartName == "")
                {
                    MenuManager.Instance.AddMenu(new NicknameMenu((string name) =>
                    {
                        StartFlow(monId, name, -1);
                    }, () =>
                    {
                        StartFlow(monId, "", 1);
                    }), false);
                    return;
                }
                else
                    name = DataManager.Instance.StartName;
            }

            //begin

            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = Begin(monId, name);
        }


        public static IEnumerator<YieldInstruction> Begin(MonsterID monId, string name)
        {
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            DataManager.Instance.SetProgress(new MainProgress(MathUtils.Rand.NextUInt64(), Guid.NewGuid().ToString().ToUpper()));
            DataManager.Instance.Save.ActiveTeam = new ExplorerTeam();
            DataManager.Instance.Save.ActiveTeam.SetRank(0);
            DataManager.Instance.Save.ActiveTeam.Name = "";

            Character newChar = DataManager.Instance.Save.ActiveTeam.CreatePlayer(MathUtils.Rand, monId, DataManager.Instance.StartLevel, -1, DataManager.Instance.StartPersonality);
            newChar.Nickname = name;
            newChar.IsFounder = true;
            DataManager.Instance.Save.ActiveTeam.Players.Add(newChar);

            DataManager.Instance.Save.RegisterMonster(DataManager.Instance.Save.ActiveTeam.Players[0].BaseForm.Species);
            DataManager.Instance.Save.StartDate = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            DataManager.Instance.Save.UpdateTeamProfile(true);

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
