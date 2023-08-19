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
        /// <param name="name">Sound file name, relative to the Content/SE folder</param>
        public void PlaySE(string name)
        {
            GameManager.Instance.SE(name);
        }

        /// <summary>
        /// Plays a sound effect specifically from the Battle/ subdirectory
        /// </summary>
        /// <param name="name">Sound file name, relative to the Content/SE/Battle folder</param>
        public void PlayBattleSE(string name)
        {
            GameManager.Instance.BattleSE(name);
        }

        /// <summary>
        /// Plays a sound effect, and waits for it to complete before continuing.
        /// </summary>
        /// <example>
        /// SOUND:WaitSE("Battle/Hit")
        /// </example>
        public LuaFunction WaitSE;
        public Coroutine _WaitSE()
        {
            return new Coroutine(GameManager.Instance.WaitFanfareEnds());
        }

        /// <summary>
        /// Plays a continuous sound effect
        /// </summary>
        /// <param name="name">Sound file name, relative to the Content/SE folder</param>
        public void LoopSE(string name)
        {
            GameManager.Instance.LoopSE(name);
        }

        /// <summary>
        /// Plays a continuous sound effect
        /// </summary>
        /// <param name="name">Sound file name, relative to the Content/SE/Battle folder</param>
        public void LoopBattleSE(string name)
        {
            GameManager.Instance.LoopBattleSE(name);
        }

        /// <summary>
        /// Stops a continuous sound effect
        /// </summary>
        /// <param name="name">Sound file name, relative to the Content/SE folder</param>
        public void StopSE(string name)
        {
            GameManager.Instance.StopLoopSE(name);
        }

        /// <summary>
        /// Stops a continuous sound effect
        /// </summary>
        /// <param name="name">Sound file name, relative to the Content/SE/Battle folder</param>
        public void StopBattleSE(string name)
        {
            GameManager.Instance.StopLoopBattleSE(name);
        }

        /// <summary>
        /// Plays a continuous sound effect, fading in over a specified amount of time
        /// </summary>
        /// <param name="name">Sound file name, relative to the Content/SE folder</param>
        /// <param name="fadeTime">Time in frames for the sound to fade in</param>
        public void FadeInSE(string name, int fadeTime = GameManager.MUSIC_FADE_TOTAL)
        {
            GameManager.Instance.LoopSE(name, fadeTime);
        }

        /// <summary>
        /// Plays a continuous sound effect, fading in over a specified amount of time
        /// </summary>
        /// <param name="name">Sound file name, relative to the Content/SE/Battle folder</param>
        /// <param name="fadeTime">Time in frames for the sound to fade in</param>
        public void FadeInBattleSE(string name, int fadeTime = GameManager.MUSIC_FADE_TOTAL)
        {
            GameManager.Instance.LoopBattleSE(name, fadeTime);
        }

        /// <summary>
        /// Stops a continuous sound effect, fading out over a specified amount of time
        /// </summary>
        /// <param name="name">Sound file name, relative to the Content/SE folder</param>
        /// <param name="fadeTime">Time in frames for the sound to fade out</param>
        public void FadeOutSE(string name, int fadeTime = GameManager.MUSIC_FADE_TOTAL)
        {
            GameManager.Instance.StopLoopSE(name, fadeTime);
        }

        /// <summary>
        /// Stops a continuous sound effect, fading out over a specified amount of time
        /// </summary>
        /// <param name="name">Sound file name, relative to the Content/SE/Battle folder</param>
        /// <param name="fadeTime">Time in frames for the sound to fade out</param>
        public void FadeOutBattleSE(string name, int fadeTime = GameManager.MUSIC_FADE_TOTAL)
        {
            GameManager.Instance.StopLoopBattleSE(name, fadeTime);
        }

        //===========================
        //  Fanfare
        //===========================

        /// <summary>
        /// Plays a sound effect that temporarily mutes the music for its duration
        /// </summary>
        /// <param name="name">Sound file name, relative to the Content/SE folder</param>
        public void PlayFanfare(string name)
        {
            GameManager.Instance.Fanfare(name);
        }


        /// <summary>
        /// Plays a sound effect that temporarily mutes the music for its duration.
        /// This function waits for the sound to complete before continuing.
        /// </summary>
        /// <example>
        /// SOUND:WaitFanfare("Battle/LevelUp")
        /// </example>
        public LuaFunction WaitFanfare;
        public Coroutine _WaitFanfare()
        {
            return new Coroutine(GameManager.Instance.WaitFanfareEnds());
        }

        //===========================
        //  Music
        //===========================

        /// <summary>
        /// Plays a song, replacing the current one.
        /// </summary>
        /// <param name="name">The file name of the song, relative to the Content/Music folder.</param>
        /// <param name="fade">Whether to fade the old song out, or start a new one.</param>
        /// <param name="fadeTime">The amount of time, in frames, to fade out the old song.</param>
        public void PlayBGM(string name, bool fade, int fadeTime = GameManager.MUSIC_FADE_TOTAL)
        {
            GameManager.Instance.BGM(name, fade, fadeTime);
        }

        /// <summary>
        /// Stops playing the current song.
        /// </summary>
        public void StopBGM()
        {
            GameManager.Instance.BGM("", false);
        }

        /// <summary>
        /// Fades out the current song.
        /// </summary>
        /// <param name="fadeTime">The amount of time, in frames, to fade out the song.</param>
        public void FadeOutBGM(int fadeTime = GameManager.MUSIC_FADE_TOTAL)
        {
            GameManager.Instance.BGM("", true, fadeTime);
        }

        /// <summary>
        /// Sets the current volume of the song.
        /// </summary>
        /// <param name="val">A float value between 0 and 1</param>
        public void SetBGMVolume(float val)
        {
            SoundManager.SetBGMVolume(val);
        }

        /// <summary>
        /// Gets the currently playing song.  If the current song is fading out, gets the next song to be played.
        /// </summary>
        /// <returns>The filename of the song, relative to the Content/Music folder</returns>
        public string GetCurrentSong()
        {
            for (int ii = 0; ii < GameManager.Instance.MusicEffects.Count; ii++)
            {
                if (GameManager.Instance.MusicEffects[ii] is MusicFadeEffect)
                {
                    MusicFadeEffect oldFade = (MusicFadeEffect)GameManager.Instance.MusicEffects[ii];
                    return oldFade.NextSong;
                }
            }
            return GameManager.Instance.Song;
        }

        public override void SetupLuaFunctions(LuaEngine state)
        {
            WaitFanfare = state.RunString("return function(_) return coroutine.yield(_:_WaitFanfare()) end").First() as LuaFunction;
            WaitSE = state.RunString("return function(_) return coroutine.yield(_:_WaitSE()) end").First() as LuaFunction;
        }
    }
}
