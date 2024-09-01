using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Content;
using Microsoft.Xna.Framework;

namespace RogueEssence.Menu
{
    public class MailMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 8;

        public delegate void OnChoosePath(string path);
        private OnChoosePath action;

        MailMiniSummary summaryMenu;

        private string[] files;
        private bool sosMode;

        public MailMenu(bool sosMode, OnChoosePath action)
        {
            this.sosMode = sosMode;
            this.action = action;

            string parentPath = sosMode ? PathMod.FromApp(DataManager.RESCUE_IN_PATH + DataManager.SOS_FOLDER) : PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.AOK_FOLDER);
            files = Directory.GetFiles(parentPath, "*" + (sosMode ? DataManager.SOS_EXTENSION : DataManager.AOK_EXTENSION));
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < files.Length; ii++)
            {
                string filename = files[ii];
                flatChoices.Add(new MenuTextChoice(files[ii].Substring(files[ii].LastIndexOf('/')+1), () => { choose(filename); }));
            }
            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);


            summaryMenu = new MailMiniSummary(Rect.FromPoints(new Loc(8,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - VERT_SPACE * 4),
                new Loc(GraphicsManager.ScreenWidth - 8, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(8, 8), 196, Text.FormatKey(sosMode ? "MENU_MAIL_SOS_TITLE" : "MENU_MAIL_AOK_TITLE"), choices, 0, 0, SLOTS_PER_PAGE);
        }


        private void choose(string fileName)
        {
            SOSMail mail = null;
            bool offVersion = false;
            if (sosMode)
            {
                mail = DataManager.LoadRescueMail(fileName) as SOSMail;
                if (mail != null)
                {
                    List<ModVersion> curVersions = PathMod.GetModVersion();
                    List<ModDiff> versionDiff = PathMod.DiffModVersions(mail.DefeatedVersion, curVersions);
                    if (versionDiff.Count > 0)
                    {
                        mail = null;
                        offVersion = true;
                    }
                }
            }
            MenuManager.Instance.AddMenu(new MailChosenMenu(sosMode && (mail != null), offVersion, fileName, action, () => { DeleteAction(fileName); }), true);
        }


        private void DeleteAction(string fileName)
        {
            File.Delete(fileName);

            MenuManager.Instance.RemoveMenu();

            string parentPath = sosMode ? PathMod.FromApp(DataManager.RESCUE_IN_PATH + DataManager.SOS_FOLDER) : PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.AOK_FOLDER);
            files = Directory.GetFiles(parentPath, "*" + (sosMode ? DataManager.SOS_EXTENSION : DataManager.AOK_EXTENSION));

            if (files.Length > 0)
                MenuManager.Instance.ReplaceMenu(new MailMenu(sosMode, action));
            else
                MenuManager.Instance.RemoveMenu();
        }

        protected override void ChoiceChanged()
        {
            int totalChoice = CurrentChoice + CurrentPage * SLOTS_PER_PAGE;
            string fileName = files[totalChoice];

            BaseRescueMail mail = DataManager.LoadRescueMail(fileName);
            if (sosMode)
                summaryMenu.SetSOS(mail as SOSMail);
            else
                summaryMenu.SetAOK(mail as AOKMail);

            base.ChoiceChanged();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            //draw other windows
            summaryMenu.Draw(spriteBatch);
        }
    }
}
