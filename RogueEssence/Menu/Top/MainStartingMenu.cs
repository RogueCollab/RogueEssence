using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System;

namespace RogueEssence.Menu
{
    public class MainStartingMenu : TitledStripMenu
    {
        
        public SpeakerPortrait Portrait;

        public MainStartingMenu()
        {
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DataManager.Instance.StartChars.Count; ii++)
            {
                int startChar = DataManager.Instance.StartChars[ii];
                flatChoices.Add(new MenuTextChoice(DataManager.Instance.GetMonster(startChar).Name.ToLocal(), () => { choose(startChar); }));
            }

            Portrait = new SpeakerPortrait(new MonsterID(), new EmoteStyle(0), new Loc(200, 32), true);

            Initialize(new Loc(16, 16), 112, Text.FormatKey("MENU_CHARA_CHOICE_TITLE"), flatChoices.ToArray(), 0);
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
            Portrait.Speaker = new MonsterID(DataManager.Instance.StartChars[CurrentChoice], 0, 0, Gender.Unknown);
            base.ChoiceChanged();
        }

        private void choose(int choice)
        {
            List<DialogueChoice> choices = new List<DialogueChoice>();
            choices.Add(new DialogueChoice(Text.FormatKey("MENU_BOY"), () =>
            {
                MenuManager.Instance.AddMenu(new NicknameMenu((string name) => {
                    start(choice, name, Gender.Male);
                }), false);
            }));
            choices.Add(new DialogueChoice(Text.FormatKey("MENU_GIRL"), () =>
            {
                MenuManager.Instance.AddMenu(new NicknameMenu((string name) => {
                    start(choice, name, Gender.Female);
                }), false);
            }));
            choices.Add(new DialogueChoice(Text.FormatKey("MENU_CANCEL"), () => { }));
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateMultiQuestion(Text.FormatKey("DLG_ASK_GENDER"), false, choices, 0, choices.Count - 1), false);
        }
        

        private void start(int choice, string name, Gender gender)
        {
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = Begin(choice, name, gender);
        }
        
        public IEnumerator<YieldInstruction> Begin(int choice, string name, Gender gender)
        {
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            DataManager.Instance.SetProgress(new MainProgress(MathUtils.Rand.NextUInt64(), Guid.NewGuid().ToString().ToUpper()));
            DataManager.Instance.Save.ActiveTeam = new ExplorerTeam();
            DataManager.Instance.Save.ActiveTeam.SetRank(0);
            DataManager.Instance.Save.ActiveTeam.Name = "";
            
            Character newChar = DataManager.Instance.Save.ActiveTeam.CreatePlayer(MathUtils.Rand, new MonsterID(choice, 0, 0, gender), DataManager.Instance.StartLevel, -1, DataManager.Instance.StartPersonality);
            newChar.Nickname = name;
            newChar.IsFounder = true;
            DataManager.Instance.Save.ActiveTeam.Players.Add(newChar);

            DataManager.Instance.Save.RegisterMonster(DataManager.Instance.Save.ActiveTeam.Players[0].BaseForm.Species);
            DataManager.Instance.Save.StartDate = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            DataManager.Instance.Save.UpdateTeamProfile(true);

            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.StartMap));
        }
    }
}
