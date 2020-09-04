using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class TileLayer
    {
        public List<TileFrame> Frames;
        public int FrameLength;

        public override string ToString()
        {
            if (Frames.Count == 0)
                return String.Format("Empty Layer");
            else if (Frames.Count == 1)
                return Frames[0].ToString();
            else
                return String.Format("[{0}-Frame Layer]", Frames[0].ToString());
        }

        public TileLayer()
        {
            Frames = new List<TileFrame>();
            FrameLength = 60;
        }

        public TileLayer(int frameLength)
        {
            Frames = new List<TileFrame>();
            FrameLength = frameLength;
        }

        public TileLayer(TileLayer other)
        {
            Frames = new List<TileFrame>();
            for (int ii = 0; ii < other.Frames.Count; ii++)
                Frames.Add(other.Frames[ii]);
            FrameLength = other.FrameLength;
        }

        public TileLayer(Loc texture, string sheet)
            : this()
        {
            Frames.Add(new TileFrame(texture, sheet));
        }

        public void Draw(SpriteBatch spriteBatch, Loc pos, ulong totalTick)
        {
            Draw(spriteBatch, pos, totalTick, Color.White);
        }
        public void Draw(SpriteBatch spriteBatch, Loc pos, ulong totalTick, Color color)
        {
            if (Frames.Count > 0)
            {
                int currentFrame = (int)(totalTick / (ulong)FrameTick.FrameToTick(FrameLength) % (ulong)Frames.Count);
                Content.BaseSheet texture = GraphicsManager.GetTile(Frames[currentFrame]);
                texture.Draw(spriteBatch, pos.ToVector2(), null, color);
            }
        }

        public bool Equals(TileLayer other)
        {
            if (Object.ReferenceEquals(other, null))
                return false;

            if (FrameLength != other.FrameLength)
                return false;
            if (Frames.Count != other.Frames.Count)
                return false;

            for (int ii = 0; ii < other.Frames.Count; ii++)
            {
                if (!Frames[0].Equals(other.Frames[0]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return (obj != null) && Equals(obj as TileLayer);
        }

        public static bool operator ==(TileLayer value1, TileLayer value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(TileLayer value1, TileLayer value2)
        {
            return !(value1 == value2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
