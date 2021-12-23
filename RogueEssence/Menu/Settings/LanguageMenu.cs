using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Menu
{
    public class LanguageMenu : SingleStripMenu
    {
        public override bool CanMenu { get { return false; } }
        public override bool CanCancel { get { return false; } }

        public LanguageMenu()
        {
            List<MenuTextChoice> choices = new List<MenuTextChoice>();

            for (int ii = 0; ii < Text.SupportedLangs.Length; ii++)
            {
                string lang = Text.SupportedLangs[ii];
                choices.Add(new MenuTextChoice(lang.ToName(), () => { choose(lang); }));
            }
            
            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);
        }

        private void choose(string lang)
        {
            DiagManager.Instance.CurSettings.Language = lang;
            DiagManager.Instance.SaveSettings(DiagManager.Instance.CurSettings);
            Text.SetCultureCode(lang);
            MenuManager.Instance.RemoveMenu();
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_LANGUAGE_SET", DiagManager.Instance.CurSettings.Language.ToName())), false);
        }

        protected override void MenuPressed()
        {

        }

        protected override void Canceled()
        {

        }
    }
}
