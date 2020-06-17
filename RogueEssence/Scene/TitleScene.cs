using System.Collections.Generic;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence
{
    public class TitleScene : BaseScene
    {
        const int ENTER_WAIT_TIME = 90;
        const int ENTER_FLASH_TIME = 60;

        private bool hideTitle;
        private ulong startTime;

        public static List<IInteractable> TitleMenuSaveState;

        public TitleScene(bool hideTitle) : base()
        {
            this.hideTitle = hideTitle;
        }

        public override void Exit() { }

        public override void Begin()
        {
            //set up title, fade, and start music
            GameManager.Instance.BGM("Title.ogg", true);
            startTime = GraphicsManager.TotalFrameTick;
        }

        public override IEnumerator<YieldInstruction> ProcessInput()
        {
            if (!hideTitle && GameManager.Instance.InputManager.BaseKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter) || GameManager.Instance.InputManager.BaseButtonPressed(Microsoft.Xna.Framework.Input.Buttons.Start))
            {
                GameManager.Instance.SE("Menu/Confirm");
                hideTitle = true;
            }
            if (hideTitle)
            {
                if (TitleMenuSaveState == null)
                {
                    DataManager.Instance.SetProgress(null);
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new TopMenu()));
                }
                else
                {
                    List<IInteractable> save = TitleMenuSaveState;
                    TitleMenuSaveState = null;
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.LoadMenuState(save));
                }
            }
            else
                yield return new WaitForFrames(1);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float window_scale = GraphicsManager.WindowZoom;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(new Vector3(window_scale, window_scale, 1)));


            BaseSheet bg = GraphicsManager.GetBackground("Sky");
            bg.Draw(spriteBatch, new Vector2(), null);

            if (!hideTitle)
            {
                BaseSheet title = GraphicsManager.Title;
                title.Draw(spriteBatch, new Vector2(GraphicsManager.ScreenWidth / 2 - title.Width / 2, 0), null);

                if ((GraphicsManager.TotalFrameTick - startTime) > (ulong)FrameTick.FrameToTick(ENTER_WAIT_TIME)
                    && ((GraphicsManager.TotalFrameTick - startTime) / (ulong)FrameTick.FrameToTick(ENTER_FLASH_TIME / 2)) % 2 == 0)
                {
                    BaseSheet subtitle = GraphicsManager.Subtitle;
                    subtitle.Draw(spriteBatch, new Vector2(GraphicsManager.ScreenWidth / 2 - subtitle.Width / 2, GraphicsManager.ScreenHeight * 3 / 4), null);
                }
            }
            spriteBatch.End();
        }

    }
}
