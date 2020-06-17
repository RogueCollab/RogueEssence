using System.Collections.Generic;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using Microsoft.Xna.Framework;

namespace RogueEssence.Menu
{
    public class RestMenu : TitledStripMenu
    {

        public RestMenu()
        {
            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            bool isRecording = DataManager.Instance.RecordingReplay;
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REST_SUSPEND"), SuspendAction, isRecording, isRecording ? Color.White : Color.Red));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REST_QUIT"), QuitAction));

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), Text.FormatKey("MENU_REST_TITLE"), choices.ToArray(), 0);
        }

        private void SuspendAction()
        {
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateQuestion(Text.FormatKey("DLG_SUSPEND_ASK"), () =>
                {
                    MenuManager.Instance.ClearMenus();
                    //suspend
                    if (GameManager.Instance.CurrentScene == GroundScene.Instance) //the only time you can quicksave in ground mode is in rogue mode
                        GameManager.Instance.SceneOutcome = GroundScene.Instance.SuspendGame();
                    else if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                        GameManager.Instance.SceneOutcome = DungeonScene.Instance.SuspendGame();
                }, () => { }), false);
        }

        private void QuitAction()
        {
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateQuestion(MonsterID.Invalid,
                null, new EmoteStyle(0), Text.FormatKey("DLG_QUIT_ASK"), true, () =>
                {
                    MenuManager.Instance.ClearMenus();
                    //give up
                    MenuManager.Instance.EndAction = (GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.GiveUp, Dir8.None, (int)GameProgress.ResultType.GaveUp)) : GroundScene.Instance.ProcessInput(new GameAction(GameAction.ActionType.GiveUp, Dir8.None, (int)GameProgress.ResultType.GaveUp));
                }, () => { }, true), false);
        }
    }
}
