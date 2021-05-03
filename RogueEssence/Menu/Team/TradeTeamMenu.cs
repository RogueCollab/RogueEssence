using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueEssence.Network;

namespace RogueEssence.Menu
{
    public class TradeTeamMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 8;

        SummaryMenu yourTitle;
        SummaryMenu yourSummary;
        MenuText yourStatus;


        OfferFeaturesMenu theirInfo;
        SummaryMenu theirTitle;
        SummaryMenu theirSummary;
        MenuText theirStatus;

        public ExchangeState CurrentState;

        public TradeTeamMenu(int defaultChoice)
        {
            int menuWidth = 152;

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Assembly.Count; ii++)
            {
                int index = ii;
                Character character = DataManager.Instance.Save.ActiveTeam.Assembly[index];
                bool tradeable = !character.IsFounder && !character.IsFavorite;
                MenuText memberName = new MenuText(character.BaseName, new Loc(2, 1), tradeable ? Color.White : Color.Red);
                MenuText memberLv = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT", character.Level), new Loc(menuWidth - 8 * 4, 1),
                    DirV.Up, DirH.Right, tradeable ? Color.White : Color.Red);
                flatChoices.Add(new MenuElementChoice(() => { choose(index); }, tradeable, memberName, memberLv));
            }
            List<MenuChoice[]> box = SortIntoPages(flatChoices, SLOTS_PER_PAGE);
            
            int page = defaultChoice / SLOTS_PER_PAGE;
            int choice = defaultChoice % SLOTS_PER_PAGE;
            
            Initialize(new Loc(0, 16 + LINE_SPACE + GraphicsManager.MenuBG.TileHeight * 2), menuWidth, Text.FormatKey("MENU_ASSEMBLY_TITLE"), box.ToArray(), choice, page, SLOTS_PER_PAGE);

            theirInfo = new OfferFeaturesMenu(new Rect(GraphicsManager.ScreenWidth - 0 - menuWidth, 16 + LINE_SPACE + GraphicsManager.MenuBG.TileHeight * 2, Bounds.Width, Bounds.Height), null);

            yourTitle = new SummaryMenu(Rect.FromPoints(new Loc(Bounds.Start.X, Bounds.Start.Y - LINE_SPACE - GraphicsManager.MenuBG.TileHeight * 2), new Loc(Bounds.End.X, Bounds.Start.Y)));
            MenuText yourText = new MenuText(DataManager.Instance.Save.ActiveTeam.Name,
                new Loc((yourTitle.Bounds.X + yourTitle.Bounds.End.X) / 2, yourTitle.Bounds.Y + GraphicsManager.MenuBG.TileHeight), DirH.None);
            yourText.Color = TextTan;
            yourTitle.Elements.Add(yourText);

            yourSummary = new SummaryMenu(Rect.FromPoints(new Loc(Bounds.Start.X, Bounds.End.Y),
                new Loc(Bounds.End.X, Bounds.End.Y + LINE_SPACE + GraphicsManager.MenuBG.TileHeight * 2)));
            yourStatus = new MenuText("",
                new Loc((yourSummary.Bounds.X + yourSummary.Bounds.End.X) / 2, yourSummary.Bounds.Y + GraphicsManager.MenuBG.TileHeight), DirH.None);
            yourStatus.Color = TextTan;
            yourSummary.Elements.Add(yourStatus);



            

            theirTitle = new SummaryMenu(Rect.FromPoints(new Loc(theirInfo.Bounds.Start.X, theirInfo.Bounds.Start.Y - LINE_SPACE - GraphicsManager.MenuBG.TileHeight * 2), new Loc(theirInfo.Bounds.End.X, theirInfo.Bounds.Start.Y)));
            MenuText theirText = new MenuText("",
                new Loc((theirTitle.Bounds.X + theirTitle.Bounds.End.X) / 2, theirTitle.Bounds.Y + GraphicsManager.MenuBG.TileHeight), DirH.None);
            theirText.Color = TextTan;
            theirTitle.Elements.Add(theirText);

            theirSummary = new SummaryMenu(Rect.FromPoints(new Loc(theirInfo.Bounds.Start.X, theirInfo.Bounds.End.Y),
                new Loc(theirInfo.Bounds.End.X, theirInfo.Bounds.End.Y + LINE_SPACE + GraphicsManager.MenuBG.TileHeight * 2)));
            theirStatus = new MenuText("",
                new Loc((theirSummary.Bounds.X + theirSummary.Bounds.End.X) / 2, theirSummary.Bounds.Y + GraphicsManager.MenuBG.TileHeight), DirH.None);
            theirStatus.Color = TextTan;
            theirSummary.Elements.Add(theirStatus);

            ActivityTradeTeam tradeTeam = NetworkManager.Instance.Activity as ActivityTradeTeam;
            theirText.SetText(tradeTeam.TargetInfo.Data.TeamName);

            CurrentState = ExchangeState.Selecting;
        }

        private void choose(int choice)
        {
            //open summary
            CharData charData = DataManager.Instance.Save.ActiveTeam.Assembly[choice];
            OfferFeaturesMenu menu = new OfferFeaturesMenu(this.Bounds, this);
            menu.SetCurrentChar(charData);

            MenuManager.Instance.AddMenu(menu, true);
            
            CurrentState = ExchangeState.Viewing;

            ActivityTradeTeam tradeTeam = NetworkManager.Instance.Activity as ActivityTradeTeam;
            tradeTeam.OfferChar(charData);
            tradeTeam.SetReady(CurrentState);
        }


        protected override void MenuPressed()
        {
            attemptCancel();
        }
        protected override void Canceled()
        {
            attemptCancel();
        }

        private void attemptCancel()
        {
            QuestionDialog dialog = MenuManager.Instance.CreateQuestion(Text.FormatKey("DLG_ONLINE_TRADE_END_ASK"), () =>
            {
                MenuManager.Instance.RemoveMenu();
                NetworkManager.Instance.Disconnect();
            }, () => { });
            MenuManager.Instance.AddMenu(dialog, true);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            yourTitle.Draw(spriteBatch);
            yourSummary.Draw(spriteBatch);

            if (theirInfo.CurrentChar != null)
                theirInfo.Draw(spriteBatch);
            theirTitle.Draw(spriteBatch);
            theirSummary.Draw(spriteBatch);
        }

        public override void Update(InputManager input)
        {
            Visible = true;

            NetworkManager.Instance.Update();
            if (NetworkManager.Instance.Status == OnlineStatus.Offline)
            {
                //give offline message in a dialogue
                MenuManager.Instance.RemoveMenu();
                MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(NetworkManager.Instance.ExitMsg), false);
            }
            else
            {
                base.Update(input);
                UpdateStatus();
            }
        }

        public void UpdateStatus()
        {
            ActivityTradeTeam tradeTeam = NetworkManager.Instance.Activity as ActivityTradeTeam;

            if (tradeTeam.OfferedChar != theirInfo.CurrentChar)
                theirInfo.SetCurrentChar(tradeTeam.OfferedChar);

            //set status
            yourStatus.SetText(CurrentState.ToLocal("msg"));
            theirStatus.SetText(tradeTeam.CurrentState.ToLocal("msg"));
        }
    }
}
