using System;
using Microsoft.Xna.Framework.Audio;

namespace RogueEssence.Content
{
    public static class SoundManager
    {
        static float bgmVol;

        static float bgmBalance;
        static LoopedSong song;
        public static float BGMBalance
        {
            get { return bgmBalance; }
            set
            {
                bgmBalance = value;
                if (song != null)
                    song.Volume = bgmVol * BGMBalance;
            }
        }
        static float seBalance;
        public static float SEBalance
        {
            get { return seBalance; }
            set { seBalance = value; }
        }

        private static string[] playedSounds = new string[8];
        private static int soundIndex = 0;

        public static void InitStatic()
        {
            bgmBalance = 1f;
            seBalance = 1f;
        }

        public static void PlayBGM(string fileName, float volume = 1.0f)
        {
            if (song != null)
            {
                song.Stop();
                song = null;
            }
            if (!String.IsNullOrWhiteSpace(fileName))
            {
                song = new LoopedSong(fileName);
                song.Play();
                bgmVol = volume;
                song.Volume = bgmVol * BGMBalance;
            }
        }

        public static void SetBGMVolume(float volume)
        {
            bgmVol = volume;
            if (song != null)
                song.Volume = bgmVol * BGMBalance;
        }

        public static void NewFrame()
        {
            soundIndex = 0;
        }


        public static int PlaySound(string fileName, float volume = 1.0f)
        {
            if (volume * seBalance <= 0f)
                return 0;

            //don't play more than X sound effects in one frame
            if (soundIndex == playedSounds.Length)
                return 0;

            //don't play more than one instance of the same sound in one frame
            for (int ii = 0; ii < soundIndex; ii++)
            {
                if (fileName == playedSounds[ii])
                    return 0;
            }
            playedSounds[soundIndex] = fileName;
            soundIndex++;

            IntPtr stbVorbisData = FAudio.stb_vorbis_open_filename(fileName, out int error, IntPtr.Zero);
            FAudio.stb_vorbis_info fileInfo = FAudio.stb_vorbis_get_info(stbVorbisData);


            long total_samples = FAudio.stb_vorbis_stream_length_in_samples(stbVorbisData);
            long total_frames = total_samples * 60 / fileInfo.sample_rate;
            float[] chunk = new float[fileInfo.channels * total_samples];
            int framesRead = FAudio.stb_vorbis_get_samples_float_interleaved(stbVorbisData, fileInfo.channels, chunk, fileInfo.channels * (int)total_samples);
            FAudio.stb_vorbis_close(stbVorbisData);


            DynamicSoundEffectInstance soundStream = new DynamicSoundEffectInstance(
                (int)fileInfo.sample_rate,
                (fileInfo.channels == 1) ? AudioChannels.Mono : AudioChannels.Stereo
            );
            soundStream.Volume = volume * seBalance;
            soundStream.SubmitFloatBufferEXT(chunk, 0, framesRead * fileInfo.channels);
            soundStream.Play();


            return (int)total_frames;
        }
    }
}
