using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Network;

namespace RogueEssence.Menu
{
    public class OfferItemsMenu : InteractableMenu
    {
        public MenuText Title;
        public MenuDivider MainDiv;
        public MenuText[] Items;

        public List<InvItem> CurrentOffer { get; private set; }

        private TradeItemMenu baseMenu;

        public OfferItemsMenu(Rect bounds, TradeItemMenu baseMenu)
        {
            Bounds = bounds;
            this.baseMenu = baseMenu;

            Title = new MenuText(Text.FormatKey("MENU_TRADE_ITEM_OFFER"), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            MainDiv = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);

            Items = new MenuText[0];
        }

        public void SetCurrentItems(List<InvItem> offer)
        {
            CurrentOffer = offer;
            if (CurrentOffer == null)
                return;
            List<MenuText> validItems = new List<MenuText>();

            for (int ii = 0; ii < offer.Count; ii++)
                validItems.Add(new MenuText(offer[ii].GetDisplayName(), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * ii + TitledStripMenu.TITLE_OFFSET)));
            
            if (validItems.Count > 0)
                Items = validItems.ToArray();
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Title;

            yield return MainDiv;

            //all items
            foreach (MenuText item in Items)
                yield return item;
        }

        public override void Update(InputManager input)
        {
            Visible = true;


            NetworkManager.Instance.Update();
            if (NetworkManager.Instance.Status == OnlineStatus.Offline)
            {
                //give offline message in a dialogue
                MenuManager.Instance.RemoveMenu();
                MenuManager.Instance.RemoveMenu();
                MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(NetworkManager.Instance.ExitMsg), false);
            }
            else
            {
                ActivityTradeItem tradeItem = NetworkManager.Instance.Activity as ActivityTradeItem;

                if (baseMenu.CurrentState == ExchangeState.Viewing)
                {
                    if (input.JustPressed(FrameInput.InputType.Confirm))
                    {
                        baseMenu.CurrentState = ExchangeState.Ready;

                        tradeItem.SetReady(baseMenu.CurrentState);
                    }
                    else if (input.JustPressed(FrameInput.InputType.Cancel))
                    {
                        GameManager.Instance.SE("Menu/Cancel");
                        MenuManager.Instance.RemoveMenu();

                        baseMenu.CurrentState = ExchangeState.Selecting;

                        tradeItem.OfferItems(new List<InvItem>());
                        tradeItem.SetReady(baseMenu.CurrentState);
                    }
                }
                else if (baseMenu.CurrentState == ExchangeState.Ready)
                {
                    if (tradeItem.CurrentState == ExchangeState.Ready)
                    {
                        DialogueBox dialog = MenuManager.Instance.CreateQuestion(Text.FormatKey("DLG_TRADE_ITEM_ASK"), () =>
                        {
                            baseMenu.CurrentState = ExchangeState.Exchange;
                            tradeItem.SetReady(baseMenu.CurrentState);
                        }, () =>
                        {
                            baseMenu.CurrentState = ExchangeState.Viewing;
                            tradeItem.SetReady(baseMenu.CurrentState);
                        });
                        MenuManager.Instance.AddMenu(dialog, true);
                    }
                    else
                    {
                        if (input.JustPressed(FrameInput.InputType.Cancel))
                        {
                            GameManager.Instance.SE("Menu/Cancel");

                            baseMenu.CurrentState = ExchangeState.Viewing;
                            tradeItem.SetReady(baseMenu.CurrentState);
                        }
                    }
                }
                else if (baseMenu.CurrentState == ExchangeState.Exchange)
                {
                    if (tradeItem.CurrentState == ExchangeState.Exchange || tradeItem.CurrentState == ExchangeState.PostTradeWait)
                    {
                        int chosenIndex = baseMenu.CurrentPage * baseMenu.SpacesPerPage + baseMenu.CurrentChoice;

                        List<int> offerItems = new List<int>();
                        foreach (InvItem item in CurrentOffer)
                            offerItems.Add(item.ID);
                        DataManager.Instance.Save.ActiveTeam.TakeItems(offerItems);

                        DataManager.Instance.Save.ActiveTeam.StoreItems(tradeItem.OfferedItems);

                        baseMenu.CurrentState = ExchangeState.PostTradeWait;
                        tradeItem.SetReady(baseMenu.CurrentState);
                    }
                    else if (tradeItem.CurrentState != ExchangeState.Ready)
                    {
                        MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_TRADE_CANCELED")), true);

                        baseMenu.CurrentState = ExchangeState.Viewing;
                        tradeItem.SetReady(baseMenu.CurrentState);
                    }
                }
                else if (baseMenu.CurrentState == ExchangeState.PostTradeWait)
                {
                    if (tradeItem.CurrentState != ExchangeState.Exchange)
                    {
                        DataManager.Instance.SaveMainGameState();

                        int chosenIndex = baseMenu.CurrentPage * baseMenu.SpacesPerPage + baseMenu.CurrentChoice;

                        tradeItem.OfferItems(new List<InvItem>());

                        MenuManager.Instance.RemoveMenu();
                        MenuManager.Instance.ReplaceMenu(new TradeItemMenu(chosenIndex));

                        GameManager.Instance.Fanfare("Fanfare/Treasure");
                        MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_TRADE_ITEM_COMPLETE")), false);

                        tradeItem.SetReady(ExchangeState.Selecting);
                    }
                }

                baseMenu.UpdateStatus();
            }
        }
    }
}
