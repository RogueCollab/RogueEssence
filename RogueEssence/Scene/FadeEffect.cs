using System;
using System.Collections.Generic;
using System.IO;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Data;
using RogueEssence.Menu;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Script;
using RogueEssence.Dev;
using RogueEssence.LevelGen;
using System.Xml.Linq;

namespace RogueEssence
{
    public abstract class FadeFX
    {
        public float fadeAmount;
        public void Draw(SpriteBatch spriteBatch)
        {
            if (fadeAmount > 0)
                DrawInternal(spriteBatch);
        }

        protected abstract void DrawInternal(SpriteBatch spriteBatch);

        protected IEnumerator<YieldInstruction> FadeInternal(bool fadeIn, int fadeTime)
        {
            long currentFadeTime = fadeTime;
            while (currentFadeTime > 0)
            {
                currentFadeTime--;
                float amount = 0f;
                if (fadeIn)
                    amount = ((float)currentFadeTime / (float)fadeTime);
                else
                    amount = ((float)(fadeTime - currentFadeTime) / (float)fadeTime);
                fadeAmount = 1f - amount;
                yield return new WaitForFrames(1);
            }
        }
    }
    public class ScreenFadeFX : FadeFX
    {
        public bool fadeWhite;

        protected override void DrawInternal(SpriteBatch spriteBatch)
        {
            GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(0, 0, GraphicsManager.ScreenWidth, GraphicsManager.ScreenHeight), null, (fadeWhite ? Color.White : Color.Black) * fadeAmount);
        }

        public void SetFade(bool faded, bool useWhite)
        {
            fadeAmount = faded ? 1f : 0f;
            fadeWhite = useWhite;
        }

        public IEnumerator<YieldInstruction> Fade(bool fadeIn, bool useWhite, int fadeTime)
        {
            if (!fadeIn && fadeAmount == 0f)
                yield break;
            if (fadeIn && fadeAmount == 1f)
            {
                SetFade(true, useWhite);
                yield break;
            }

            fadeWhite = useWhite;

            yield return CoroutineManager.Instance.StartCoroutine(FadeInternal(fadeIn, fadeTime));
        }
    }
    public class TitleFadeFX : FadeFX
    {
        private string title;

        public TitleFadeFX()
        {
            title = "";
        }

        protected override void DrawInternal(SpriteBatch spriteBatch)
        {
            GraphicsManager.DungeonFont.DrawText(spriteBatch, GraphicsManager.ScreenWidth / 2, GraphicsManager.ScreenHeight / 2,
                        title, null, DirV.None, DirH.None, Color.White * fadeAmount);
        }

        public IEnumerator<YieldInstruction> Fade(bool fadeIn, string newTitle, int fadeTime)
        {
            if (fadeIn)
                title = newTitle;

            yield return CoroutineManager.Instance.StartCoroutine(FadeInternal(fadeIn, fadeTime));

            if (!fadeIn)
                title = "";
        }
    }
    public class BGFadeFX : FadeFX
    {
        private BGAnimData bg;

        public BGFadeFX()
        {
            bg = new BGAnimData();
        }

        protected override void DrawInternal(SpriteBatch spriteBatch)
        {
            if (bg.AnimIndex == "")
                return;

            DirSheet sheet = GraphicsManager.GetBackground(bg.AnimIndex);
            sheet.DrawDir(spriteBatch, new Vector2(GraphicsManager.ScreenWidth / 2 - sheet.TileWidth / 2, GraphicsManager.ScreenHeight / 2 - sheet.TileHeight / 2),
                bg.GetCurrentFrame(GraphicsManager.TotalFrameTick, sheet.TotalFrames), Dir8.Down, Color.White * ((float)bg.Alpha / 255) * fadeAmount);
        }

        public IEnumerator<YieldInstruction> Fade(bool fadeIn, BGAnimData newBG, int fadeTime)
        {
            if (fadeIn)
                bg = newBG;

            yield return CoroutineManager.Instance.StartCoroutine(FadeInternal(fadeIn, fadeTime));

            if (!fadeIn)
                bg = new BGAnimData();
        }
    }
}
