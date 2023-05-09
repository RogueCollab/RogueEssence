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


namespace RogueEssence
{
    public abstract class MusicEffect
    {
        public abstract bool Finished { get; }
        public abstract void Update(FrameTick elapsedTime, ref float musicFadeFraction, ref float crossFadeFraction);
    }

    public abstract class MusicFadeEffect : MusicEffect
    {

        public string NextSong;
        public FrameTick MusicFadeTime;
        public int MusicFadeTotal;

        public override bool Finished { get { return NextSong == null; } }

        public MusicFadeEffect(string newBGM, FrameTick fadeTime, int fadeTotal)
        {
            NextSong = newBGM;
            MusicFadeTime = fadeTime;
            MusicFadeTotal = fadeTotal;
        }


        protected void onMusicChange(string newBGM)
        {
            if (newBGM.Length > 0)
            {
                string name = "";
                string originName = "";
                string origin = "";
                string artist = "";
                string spoiler = "";
                string fileName = PathMod.ModPath(GraphicsManager.MUSIC_PATH + newBGM);
                if (File.Exists(fileName))
                {
                    LoopedSong song = new LoopedSong(fileName);
                    name = song.Name;
                    if (song.Tags.ContainsKey("TITLE"))
                        originName = song.Tags["TITLE"];
                    if (song.Tags.ContainsKey("ALBUM"))
                        origin = song.Tags["ALBUM"];
                    if (song.Tags.ContainsKey("ARTIST"))
                        artist = song.Tags["ARTIST"];
                    if (song.Tags.ContainsKey("SPOILER"))
                        spoiler = song.Tags["SPOILER"];
                    LuaEngine.Instance.OnMusicChange(name, originName, origin, artist, spoiler);
                }
            }
        }
    }

    public class MusicCrossFadeEffect : MusicFadeEffect
    {
        public MusicCrossFadeEffect(string newBGM, FrameTick fadeTime, int fadeTotal)
            : base(newBGM, fadeTime, fadeTotal)
        {
        }

        public override void Update(FrameTick elapsedTime, ref float musicFadeFraction, ref float crossFadeFraction)
        {
            if (NextSong != null)
            {
                MusicFadeTime -= elapsedTime;
                if (MusicFadeTime <= FrameTick.Zero)
                {
                    SoundManager.TransferCrossBGM();
                    GameManager.Instance.Song = NextSong;
                    NextSong = null;
                }
                else
                    crossFadeFraction = 1f - MusicFadeTime.FractionOf(MusicFadeTotal);
            }
        }
    }

    public class MusicFadeOutEffect : MusicFadeEffect
    {
        public MusicFadeOutEffect(string newBGM, FrameTick fadeTime, int fadeTotal)
            : base(newBGM, fadeTime, fadeTotal)
        {
        }

        public override void Update(FrameTick elapsedTime, ref float musicFadeFraction, ref float crossFadeFraction)
        {
            if (NextSong != null)
            {
                MusicFadeTime -= elapsedTime;
                if (MusicFadeTime <= FrameTick.Zero)
                {
                    if (File.Exists(PathMod.ModPath(GraphicsManager.MUSIC_PATH + NextSong)))
                    {
                        GameManager.Instance.Song = NextSong;
                        onMusicChange(NextSong);
                        SoundManager.PlayBGM(PathMod.ModPath(GraphicsManager.MUSIC_PATH + GameManager.Instance.Song));
                    }
                    else
                    {
                        GameManager.Instance.Song = "";
                        SoundManager.PlayBGM(GameManager.Instance.Song);
                    }
                    NextSong = null;
                }
                else
                    musicFadeFraction *= MusicFadeTime.FractionOf(MusicFadeTotal);
            }
        }
    }

    public class FanfareEffect : MusicEffect
    {
        public const int FANFARE_FADE_START = 3;
        public const int FANFARE_FADE_END = 40;
        public const int FANFARE_WAIT_EXTRA = 20;

        public enum FanfarePhase
        {
            None,
            PhaseOut,
            Wait,
            PhaseIn
        }

        public string Fanfare;
        public FrameTick FanfareTime;
        public FanfarePhase CurrentPhase;

        public override bool Finished { get { return CurrentPhase == FanfarePhase.None; } }

        public FanfareEffect(string fanfare)
        {
            CurrentPhase = FanfarePhase.PhaseOut;
            FanfareTime = FrameTick.FromFrames(FANFARE_FADE_START);
            Fanfare = fanfare;
        }

        public override void Update(FrameTick elapsedTime, ref float musicFadeFraction, ref float crossFadeFraction)
        {
            if (CurrentPhase != FanfarePhase.None)
            {
                FanfareTime -= elapsedTime;
                if (CurrentPhase == FanfarePhase.PhaseOut)
                {
                    musicFadeFraction *= FanfareTime.FractionOf(FANFARE_FADE_START);
                    if (FanfareTime <= FrameTick.Zero)
                    {
                        int pauseFrames = 0;
                        if (File.Exists(PathMod.ModPath(GraphicsManager.SOUND_PATH + Fanfare + ".ogg")))
                            pauseFrames = SoundManager.PlaySound(PathMod.ModPath(GraphicsManager.SOUND_PATH + Fanfare + ".ogg"), 1) + FANFARE_WAIT_EXTRA;
                        CurrentPhase = FanfarePhase.Wait;
                        if (FanfareTime < pauseFrames)
                            FanfareTime = FrameTick.FromFrames(pauseFrames);
                    }
                }
                else if (CurrentPhase == FanfarePhase.Wait)
                {
                    musicFadeFraction *= 0;
                    if (FanfareTime <= FrameTick.Zero)
                    {
                        CurrentPhase = FanfarePhase.PhaseIn;
                        FanfareTime = FrameTick.FromFrames(FANFARE_FADE_END);
                    }
                }
                else if (CurrentPhase == FanfarePhase.PhaseIn)
                {
                    musicFadeFraction *= (1f - FanfareTime.FractionOf(FANFARE_FADE_END));
                    if (FanfareTime <= FrameTick.Zero)
                        CurrentPhase = FanfarePhase.None;
                }
            }
        }
    }
}
