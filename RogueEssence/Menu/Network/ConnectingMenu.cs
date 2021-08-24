using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Network;
using System;

namespace RogueEssence.Menu
{
    public class ConnectingMenu : InteractableMenu
    {
        public MenuText ConnectingMsg;
        public MenuText BackMsg;

        private Action action;
        public ConnectingMenu(Action successAction)
        {
            action = successAction;
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - 80, GraphicsManager.ScreenHeight / 2 - GraphicsManager.MenuBG.TileHeight - VERT_SPACE), new Loc(GraphicsManager.ScreenWidth / 2 + 80, GraphicsManager.ScreenHeight / 2 + GraphicsManager.MenuBG.TileHeight + VERT_SPACE));

            ConnectingMsg = new MenuText(Text.FormatKey("MENU_CONNECTING_SERVER"),
                new Loc(GraphicsManager.ScreenWidth / 2, Bounds.Y + GraphicsManager.MenuBG.TileHeight), DirH.None);
            BackMsg = new MenuText(Text.FormatKey("MENU_CONNECTING_CANCEL", DiagManager.Instance.CurSettings.ActionKeys[(int)FrameInput.InputType.Cancel].ToLocal()),
                new Loc(GraphicsManager.ScreenWidth / 2, Bounds.Y + GraphicsManager.MenuBG.TileHeight + VERT_SPACE), DirH.None);

            NetworkManager.Instance.Connect();
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return ConnectingMsg;
            yield return BackMsg;
        }

        public override void Update(InputManager input)
        {
            Visible = true;
            NetworkManager.Instance.Update();
            if (NetworkManager.Instance.Status == OnlineStatus.Connected)
            {
                MenuManager.Instance.RemoveMenu();
                action();
            }
            else if (NetworkManager.Instance.Status == OnlineStatus.Offline)
            {
                //give offline message in a dialogue
                MenuManager.Instance.RemoveMenu();
                MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(NetworkManager.Instance.ExitMsg), false);
            }
            else
            {
                if (NetworkManager.Instance.Status == OnlineStatus.FindingPartner || NetworkManager.Instance.Status == OnlineStatus.ReceivingPartner)
                    ConnectingMsg.SetText(Text.FormatKey("MENU_CONNECTING_PARTNER"));

                if (input.JustPressed(FrameInput.InputType.Menu) || input.JustPressed(FrameInput.InputType.Cancel))
                {
                    NetworkManager.Instance.Disconnect();
                    GameManager.Instance.SE("Menu/Cancel");
                    MenuManager.Instance.RemoveMenu();
                }
            }
        }
    }
}
