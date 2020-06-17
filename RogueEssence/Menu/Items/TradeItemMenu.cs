using System;
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
    public class TradeItemMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 8;

        SummaryMenu yourTitle;
        SummaryMenu yourSummary;
        MenuText yourStatus;


        OfferItemsMenu theirInfo;
        SummaryMenu theirTitle;
        SummaryMenu theirSummary;
        MenuText theirStatus;

        public ExchangeState CurrentState;

        public List<int> AllowedGoods;

        public TradeItemMenu(int defaultChoice)
        {
            int menuWidth = 152;
            AllowedGoods = new List<int>();

            int[] itemPresence = new int[DataManager.Instance.DataIndices[DataManager.DataType.Item].Count];
            for (int ii = 0; ii < itemPresence.Length; ii++)
            {
                itemPresence[ii] += DataManager.Instance.Save.ActiveTeam.Storage[ii];
            }

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < itemPresence.Length; ii++)
            {
                int index = ii;
                if (itemPresence[index] > 0)
                {
                    ItemData itemEntry = DataManager.Instance.GetItem(index);
                    if (itemEntry.ItemStates.Get<MaterialState>() != null)
                    {
                        AllowedGoods.Add(index);
                        int slot = flatChoices.Count;

                        MenuText menuText = new MenuText((itemEntry.Icon > -1 ? ((char)(itemEntry.Icon + 0xE0A0)).ToString() : "") + DataManager.Instance.GetItem(ii).Name.ToLocal(), new Loc(2, 1));
                        MenuText menuCount = new MenuText("(" + itemPresence[index] + ")", new Loc(menuWidth - 8 * 4, 1), DirV.Up, DirH.Right, Color.White);
                        flatChoices.Add(new MenuElementChoice(() => { choose(slot); }, true, menuText, menuCount));
                    }
                }
            }

            defaultChoice = Math.Min(defaultChoice, flatChoices.Count - 1);
            int startChoice = defaultChoice % SLOTS_PER_PAGE;
            int startPage = defaultChoice / SLOTS_PER_PAGE;
            List<MenuChoice[]> inv = SortIntoPages(flatChoices, SLOTS_PER_PAGE);


            Initialize(new Loc(0, 16 + LINE_SPACE + GraphicsManager.MenuBG.TileHeight * 2), menuWidth, Text.FormatKey("MENU_STORAGE_TITLE"), inv.ToArray(), startChoice, startPage, SLOTS_PER_PAGE, false, 8);

            theirInfo = new OfferItemsMenu(new Rect(GraphicsManager.ScreenWidth - 0 - menuWidth, 16 + LINE_SPACE + GraphicsManager.MenuBG.TileHeight * 2, Bounds.Width, Bounds.Height), null);

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

            theirText.Text = NetworkManager.Instance.Activity.TargetInfo.Data.TeamName;

            CurrentState = ExchangeState.Selecting;
        }

        private void choose(int choice)
        {
            ChoseMultiIndex(new List<int> { choice });
        }


        protected override void ChoseMultiIndex(List<int> slots)
        {
            int startIndex = CurrentPage * SpacesPerPage + CurrentChoice;

            List<InvItem> indices = new List<InvItem>();
            foreach (int slot in slots)
                indices.Add(new InvItem(AllowedGoods[slot]));

            OfferItemsMenu menu = new OfferItemsMenu(this.Bounds, this);
            menu.SetCurrentItems(indices);

            MenuManager.Instance.AddMenu(menu, true);

            CurrentState = ExchangeState.Viewing;

            ActivityTradeItem tradeTeam = NetworkManager.Instance.Activity as ActivityTradeItem;
            tradeTeam.OfferItems(indices);
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

            if (theirInfo.CurrentOffer != null)
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
            ActivityTradeItem tradeTeam = NetworkManager.Instance.Activity as ActivityTradeItem;

            if (tradeTeam.OfferedItems != theirInfo.CurrentOffer)
                theirInfo.SetCurrentItems(tradeTeam.OfferedItems);

            //set status
            yourStatus.Text = CurrentState.ToLocal("msg");
            theirStatus.Text = tradeTeam.CurrentState.ToLocal("msg");
        }
    }
}
