using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace RogueEssence.Content
{
    public class LoopedSong : IDisposable
    {
        #region Public Properties

        public string Name
        {
            get;
            private set;
        }

        public float Volume
        {
            get { return soundStream.Volume; }
            set { soundStream.Volume = value; }
        }

        public int Channels { get; private set; }
        public int SampleRate { get; private set; }
        private int chunkSize { get { return Channels * SampleRate * 2; } }

        public Dictionary<string, string> Tags { get; private set; }

        #endregion

        #region Private Variables

        private float[] chunk;

        private DynamicSoundEffectInstance soundStream;
        private IntPtr stbVorbisData;

        private long loopStart;
        private long loopEnd;
        private long pcmPosition;

        #endregion

        #region Constructors, Deconstructor, Dispose()

        public LoopedSong(string fileName)
        {
            stbVorbisData = FAudio.stb_vorbis_open_filename(fileName, out int error, IntPtr.Zero);
            FAudio.stb_vorbis_info fileInfo = FAudio.stb_vorbis_get_info(stbVorbisData);

            Channels = fileInfo.channels;
            SampleRate = (int)fileInfo.sample_rate;
            Name = Path.GetFileNameWithoutExtension(fileName);
            
            long total_samples = FAudio.stb_vorbis_stream_length_in_samples(stbVorbisData);


            FAudio.stb_vorbis_comment comments = FAudio.stb_vorbis_get_comment(stbVorbisData);

            loopStart = 0;
            loopEnd = (int)total_samples;
            int loopLength = 0;

            Tags = new Dictionary<string, string>();
            for (int ii = 0; ii < comments.comment_list_length; ii++)
            {
                IntPtr ptr = new IntPtr(comments.comment_list.ToInt64() + IntPtr.Size * ii);
                IntPtr strPtr = (IntPtr)Marshal.PtrToStructure(ptr, typeof(IntPtr));
                string comment = Marshal.PtrToStringUTF8(strPtr);

                string[] split = comment.Split('=', 2);
                string label = split.Length > 0 ? split[0] : "";
                string val = split.Length > 1 ? split[1] : "";

                if (label != "")
                    Tags.Add(label, val);

                if (label == "LOOPSTART")
                    loopStart = Convert.ToInt32(val);
                else if (label == "LOOPLENGTH")
                    loopLength = Convert.ToInt32(val);

            }
            if (loopStart > -1)
            {
                if (loopLength > 0)
                    loopEnd = loopStart + loopLength;
            }


            pcmPosition = 0;

            soundStream = new DynamicSoundEffectInstance(
                SampleRate,
                (Channels == 1) ? AudioChannels.Mono : AudioChannels.Stereo
            );

            chunk = new float[chunkSize];
        }

        public void Dispose()
        {
            if (stbVorbisData != IntPtr.Zero)
            {
                FAudio.stb_vorbis_close(stbVorbisData);
                stbVorbisData = IntPtr.Zero;
            }
        }

        #endregion

        #region Internal Playback Methods

        internal void Play()
        {
            FAudio.stb_vorbis_seek_start(stbVorbisData);

            queueBuffer();

            soundStream.BufferNeeded += BufferNeeded;
            soundStream.Play();
        }

        internal void Resume()
        {
            soundStream.Resume();
        }

        internal void Pause()
        {
            soundStream.Pause();
        }

        internal void Stop()
        {
            soundStream.Stop();
            soundStream.BufferNeeded -= BufferNeeded;
            FAudio.stb_vorbis_seek_start(stbVorbisData);
        }

        #endregion

        internal void BufferNeeded(object sender, EventArgs args)
        {
            queueBuffer();
        }

        private void queueBuffer()
        {
            int framesRead = FAudio.stb_vorbis_get_samples_float_interleaved(stbVorbisData, Channels, chunk, chunkSize);
            framesRead = (int)Math.Min(framesRead, loopEnd-pcmPosition);

            if (framesRead != 0)
            {
                soundStream.SubmitFloatBufferEXT(chunk, 0, framesRead * Channels);
                pcmPosition += framesRead;
            }

            if (loopEnd == pcmPosition)
            {
                FAudio.stb_vorbis_seek_frame(stbVorbisData, (uint)loopStart);
                pcmPosition = loopStart;
                queueBuffer();
            }
        }

        #region Public Comparison Methods/Operators

        public bool Equals(LoopedSong song)
        {
            return (((object)song) != null) && (Name == song.Name);
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return Equals(obj as LoopedSong);
        }

        public static bool operator ==(LoopedSong song1, LoopedSong song2)
        {
            if (((object)song1) == null)
            {
                return ((object)song2) == null;
            }
            return song1.Equals(song2);
        }

        public static bool operator !=(LoopedSong song1, LoopedSong song2)
        {
            return !(song1 == song2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

    }
}
