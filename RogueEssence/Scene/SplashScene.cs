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
            GameManager.Instance.SceneOutcome = GameManager.Instance.RestartToTitle();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        }

    }
}
