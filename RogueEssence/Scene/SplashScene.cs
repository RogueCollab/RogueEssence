using System.Collections.Generic;
using RogueEssence.Menu;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence
{
    public class SplashScene : BaseScene
    {
        public SplashScene()
        {
        }

        public override void Exit() { }

        public override void Begin()
        {

        }

        public override IEnumerator<YieldInstruction> ProcessInput()
        {
            if (DiagManager.Instance.CurSettings.Language == "")
            {
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new LanguageMenu()));
                yield return new WaitForFrames(30);
            }
            GameManager.Instance.SetFade(true, false);
            GameManager.Instance.SceneOutcome = StartToTitle();
        }

        /// <summary>
        /// Start to title, without all the unneeded restart logic.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> StartToTitle()
        {
            GameManager.Instance.MoveToScene(new TitleScene(false));
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeIn());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        }

    }
}
