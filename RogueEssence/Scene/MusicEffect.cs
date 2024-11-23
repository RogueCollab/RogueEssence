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
using System.Linq;

namespace RogueEssence
{
    public abstract class MusicEffect
    {
        public abstract bool Finished { get; }
        public abstract void Update(FrameTick elapsedTime, ref float musicFadeFraction, Dictionary<string, float> crossFadeFraction);
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
                        originName = song.Tags["TITLE"][0];
                    if (song.Tags.ContainsKey("ALBUM"))
                        origin = song.Tags["ALBUM"][0];
                    if (song.Tags.ContainsKey("ARTIST"))
                        artist = song.Tags["ARTIST"][0];
                    if (song.Tags.ContainsKey("SPOILER"))
                        spoiler = song.Tags["SPOILER"][0];
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

        public override void Update(FrameTick elapsedTime, ref float musicFadeFraction, Dictionary<string, float> crossFadeFraction)
        {
            if (NextSong != null)
            {
                MusicFadeTime -= elapsedTime;
                if (MusicFadeTime <= FrameTick.Zero)
                {
                    GameManager.Instance.Song = NextSong;
                    NextSong = null;

                    foreach (string songName in GameManager.Instance.SongFamily.Keys)
                    {
                        float defaultVol = (GameManager.Instance.Song == songName) ? 1f : 0f;
                        crossFadeFraction[GameManager.Instance.SongFamily[songName]] = defaultVol;
                    }
                }
                else
                {
                    foreach (string songName in GameManager.Instance.SongFamily.Keys)
                    {
                        float defaultVol = 0;
                        if (GameManager.Instance.Song == songName)
                            defaultVol = MusicFadeTime.FractionOf(MusicFadeTotal);
                        else if (NextSong == songName)
                            defaultVol = 1f - MusicFadeTime.FractionOf(MusicFadeTotal);
                        crossFadeFraction[GameManager.Instance.SongFamily[songName]] = defaultVol;
                    }
                }
            }
        }
    }

    public class MusicFadeOutEffect : MusicFadeEffect
    {
        public HashSet<string> Family;

        public MusicFadeOutEffect(string newBGM, HashSet<string> family, FrameTick fadeTime, int fadeTotal)
            : base(newBGM, fadeTime, fadeTotal)
        {
            Family = family;
        }

        public override void Update(FrameTick elapsedTime, ref float musicFadeFraction, Dictionary<string, float> crossFadeFraction)
        {
            if (NextSong != null)
            {
                MusicFadeTime -= elapsedTime;
                if (MusicFadeTime <= FrameTick.Zero)
                {
                    string moddedPath = PathMod.ModPath(GraphicsManager.MUSIC_PATH + NextSong);
                    if (File.Exists(moddedPath))
                    {
                        GameManager.Instance.Song = NextSong;
                        Dictionary<string, string> family = new Dictionary<string, string>();
                        List<string> fileList = new List<string>();
                        foreach (string familyName in Family)
                        {
                            string file = PathMod.ModPath(GraphicsManager.MUSIC_PATH + familyName);
                            fileList.Add(file);
                            family.Add(familyName, file);
                        }
                        GameManager.Instance.SongFamily = family;
                        onMusicChange(NextSong);
                        SoundManager.PlayBGM(moddedPath, fileList.ToArray());
                    }
                    else
                    {
                        GameManager.Instance.Song = "";
                        GameManager.Instance.SongFamily = new Dictionary<string, string>();
                        SoundManager.PlayBGM(GameManager.Instance.Song, new string[0]);
                    }
                    NextSong = null;

                    crossFadeFraction.Clear();
                    foreach (string songName in GameManager.Instance.SongFamily.Keys)
                    {
                        float defaultVol = (GameManager.Instance.Song == songName) ? 1f : 0f;
                        crossFadeFraction[GameManager.Instance.SongFamily[songName]] = defaultVol;
                    }
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

        public override void Update(FrameTick elapsedTime, ref float musicFadeFraction, Dictionary<string, float> crossFadeFraction)
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
