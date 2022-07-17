using System.Linq;
using NLua;
using RogueEssence.Content;

namespace RogueEssence.Script
{
    class ScriptSound : ILuaEngineComponent
    {
        //===========================
        //  Sound Effects
        //===========================

        /// <summary>
        /// Plays a sound effect
        /// </summary>
        /// <param name="name">Sound file name</param>
        public void PlaySE(string name)
        {
            GameManager.Instance.SE(name);
        }

        public void PlayBattleSE(string name)
        {
            GameManager.Instance.BattleSE(name);
        }

        public Coroutine _WaitSE()
        {
            return new Coroutine(GameManager.Instance.WaitFanfareEnds());
        }

        public LuaFunction WaitSE;


        public void LoopSE(string name)
        {
            GameManager.Instance.LoopSE(name);
        }

        public void LoopBattleSE(string name)
        {
            GameManager.Instance.LoopBattleSE(name);
        }

        public void FadeOutSE(string name, int fadeTime = GameManager.MUSIC_FADE_TOTAL)
        {
            GameManager.Instance.StopLoopSE(name, fadeTime);
        }

        public void FadeOutBattleSE(string name, int fadeTime = GameManager.MUSIC_FADE_TOTAL)
        {
            GameManager.Instance.StopLoopBattleSE(name, fadeTime);
        }

        public void StopSE(string name)
        {
            GameManager.Instance.StopLoopSE(name);
        }

        public void StopBattleSE(string name)
        {
            GameManager.Instance.StopLoopBattleSE(name);
        }

        //===========================
        //  Fanfare
        //===========================
        public void PlayFanfare(string name)
        {
            GameManager.Instance.Fanfare(name);
        }

        public Coroutine _WaitFanfare()
        {
            return new Coroutine(GameManager.Instance.WaitFanfareEnds());
        }

        public LuaFunction WaitFanfare;

        //===========================
        //  Music
        //===========================
        public void PlayBGM(string name, bool fade, int fadeTime = GameManager.MUSIC_FADE_TOTAL)
        {
            GameManager.Instance.BGM(name, fade, fadeTime);
        }

        public void FadeOutBGM(int fadeTime = GameManager.MUSIC_FADE_TOTAL)
        {
            GameManager.Instance.BGM("", true, fadeTime);
        }

        public void StopBGM()
        {
            GameManager.Instance.BGM("", false);
        }

        public void SetBGMVolume(float val)
        {
            SoundManager.SetBGMVolume(val);
        }


        public string GetCurrentSong()
        {
            if (GameManager.Instance.NextSong != null)
                return GameManager.Instance.NextSong;
            return GameManager.Instance.Song;
        }

        public override void SetupLuaFunctions(LuaEngine state)
        {
            //TODO
            WaitFanfare = state.RunString("return function(_) return coroutine.yield(_:_WaitFanfare()) end").First() as LuaFunction;
            WaitSE = state.RunString("return function(_) return coroutine.yield(_:_WaitSE()) end").First() as LuaFunction;
        }
    }
}
