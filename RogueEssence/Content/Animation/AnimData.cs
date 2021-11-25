using System;
using RogueElements;
using RogueEssence.Dev;

namespace RogueEssence.Content
{
    public interface IPlaceableAnimData
    {
        GraphicsManager.AssetType AssetType { get; }
        string AnimIndex { get; set; }
        int FrameTime { get; set; }
        int StartFrame { get; set; }
        int EndFrame { get; set; }
        Dir8 AnimDir { get; set; }
        byte Alpha { get; set; }
        SpriteFlip AnimFlip { get; set; }

        int GetTotalFrames(int totalFrames);

        int GetCurrentFrame(FrameTick time, int totalFrames);

        int GetCurrentFrame(ulong time, int totalFrames);

        Dir8 GetDrawDir(Dir8 inputDir);

        IPlaceableAnimData Clone();
        void LoadFrom(IPlaceableAnimData other);
    }

    [Serializable]
    public class ItemAnimData : AnimDataBase, IPlaceableAnimData
    {
        public override GraphicsManager.AssetType AssetType { get { return GraphicsManager.AssetType.Item; } }
        [Anim(0, "Item/")]
        public override string AnimIndex { get; set; }

        public ItemAnimData()
            : this("", 1) { }
        public ItemAnimData(string animIndex, int frameTime)
            : this(animIndex, frameTime, -1, -1) { }
        public ItemAnimData(string animIndex, int frameTime, Dir8 dir)
            : this(animIndex, frameTime, -1, -1, 255, dir) { }
        public ItemAnimData(string animIndex, int frameTime, int startFrame, int endFrame)
            : this(animIndex, frameTime, startFrame, endFrame, 255) { }
        public ItemAnimData(string animIndex, int frameTime, int startFrame, int endFrame, byte alpha)
            : this(animIndex, frameTime, startFrame, endFrame, alpha, Dir8.Down) { }
        public ItemAnimData(string animIndex, int frameTime, int startFrame, int endFrame, byte alpha, Dir8 dir)
            : base(animIndex, frameTime, startFrame, endFrame, alpha, dir) { }
        public ItemAnimData(ItemAnimData other)
            : base(other) { }

        public override AnimDataBase Clone() { return new ItemAnimData(this); }

        IPlaceableAnimData IPlaceableAnimData.Clone() { return (IPlaceableAnimData)Clone(); }

        void IPlaceableAnimData.LoadFrom(IPlaceableAnimData other) { LoadFrom((AnimDataBase)other); }

        public override string ToString()
        {
            return AnimIndex;
        }
    }

    [Serializable]
    public class ObjAnimData : AnimDataBase, IPlaceableAnimData
    {
        public override GraphicsManager.AssetType AssetType { get { return GraphicsManager.AssetType.Object; } }
        [Anim(0, "Object/")]
        public override string AnimIndex { get; set; }

        public ObjAnimData()
            : this("", 1) { }
        public ObjAnimData(string animIndex, int frameTime)
            : this(animIndex, frameTime, -1, -1) { }
        public ObjAnimData(string animIndex, int frameTime, Dir8 dir)
            : this(animIndex, frameTime, -1, -1, 255, dir) { }
        public ObjAnimData(string animIndex, int frameTime, int startFrame, int endFrame)
            : this(animIndex, frameTime, startFrame, endFrame, 255) { }
        public ObjAnimData(string animIndex, int frameTime, int startFrame, int endFrame, byte alpha)
            : this(animIndex, frameTime, startFrame, endFrame, alpha, Dir8.Down) { }
        public ObjAnimData(string animIndex, int frameTime, int startFrame, int endFrame, byte alpha, Dir8 dir)
            : base(animIndex, frameTime, startFrame, endFrame, alpha, dir) { }
        public ObjAnimData(ObjAnimData other)
            : base(other) { }

        public override AnimDataBase Clone() { return new ObjAnimData(this); }

        IPlaceableAnimData IPlaceableAnimData.Clone() { return (IPlaceableAnimData)Clone(); }

        void IPlaceableAnimData.LoadFrom(IPlaceableAnimData other) { LoadFrom((AnimDataBase)other); }

        public override string ToString()
        {
            return AnimIndex;
        }
    }

    [Serializable]
    public class BeamAnimData : AnimDataBase
    {
        public override GraphicsManager.AssetType AssetType { get { return GraphicsManager.AssetType.Beam; } }
        [Anim(0, "Beam/")]
        public override string AnimIndex { get; set; }

        public BeamAnimData()
            : this("", 1) { }
        public BeamAnimData(string animIndex, int frameTime)
            : this(animIndex, frameTime, -1, -1) { }
        public BeamAnimData(string animIndex, int frameTime, Dir8 dir)
            : this(animIndex, frameTime, -1, -1, 255, dir) { }
        public BeamAnimData(string animIndex, int frameTime, int startFrame, int endFrame)
            : this(animIndex, frameTime, startFrame, endFrame, 255) { }
        public BeamAnimData(string animIndex, int frameTime, int startFrame, int endFrame, byte alpha)
            : this(animIndex, frameTime, startFrame, endFrame, alpha, Dir8.None) { }
        public BeamAnimData(string animIndex, int frameTime, int startFrame, int endFrame, byte alpha, Dir8 dir)
            : base(animIndex, frameTime, startFrame, endFrame, alpha, dir) { }
        public BeamAnimData(BeamAnimData other)
            : base(other) { }

        public override AnimDataBase Clone() { return new BeamAnimData(this); }

        public override string ToString()
        {
            return AnimIndex;
        }
    }

    [Serializable]
    public class BGAnimData : AnimDataBase
    {
        public override GraphicsManager.AssetType AssetType { get { return GraphicsManager.AssetType.BG; } }
        [Anim(0, "BG/")]
        public override string AnimIndex { get; set; }

        public BGAnimData()
            : this("", 1) { }
        public BGAnimData(string animIndex, int frameTime)
            : this(animIndex, frameTime, -1, -1) { }
        public BGAnimData(string animIndex, int frameTime, Dir8 dir)
            : this(animIndex, frameTime, -1, -1, 255, dir) { }
        public BGAnimData(string animIndex, int frameTime, int startFrame, int endFrame)
            : this(animIndex, frameTime, startFrame, endFrame, 255) { }
        public BGAnimData(string animIndex, int frameTime, int startFrame, int endFrame, byte alpha)
            : this(animIndex, frameTime, startFrame, endFrame, alpha, Dir8.None) { }
        public BGAnimData(string animIndex, int frameTime, int startFrame, int endFrame, byte alpha, Dir8 dir)
            : base(animIndex, frameTime, startFrame, endFrame, alpha, dir) { }
        public BGAnimData(BGAnimData other)
            : base(other) { }

        public override AnimDataBase Clone() { return new BGAnimData(this); }

        public override string ToString()
        {
            return AnimIndex;
        }
    }

    [Serializable]
    public class AnimData : AnimDataBase
    {
        public override GraphicsManager.AssetType AssetType { get { return GraphicsManager.AssetType.Particle; } }
        [Anim(0, "Particle/")]
        public override string AnimIndex { get; set; }

        public AnimData()
            : this("", 1) { }
        public AnimData(string animIndex, int frameTime)
            : this(animIndex, frameTime, -1, -1) { }
        public AnimData(string animIndex, int frameTime, Dir8 dir)
            : this(animIndex, frameTime, -1, -1, 255, dir) { }
        public AnimData(string animIndex, int frameTime, int startFrame, int endFrame)
            : this(animIndex, frameTime, startFrame, endFrame, 255) { }
        public AnimData(string animIndex, int frameTime, int startFrame, int endFrame, byte alpha)
            : this(animIndex, frameTime, startFrame, endFrame, alpha, Dir8.None) { }
        public AnimData(string animIndex, int frameTime, int startFrame, int endFrame, byte alpha, Dir8 dir)
            : base(animIndex, frameTime, startFrame, endFrame, alpha, dir) { }
        public AnimData(AnimData other)
            : base(other) { }

        public override AnimDataBase Clone() { return new AnimData(this); }

        public override string ToString()
        {
            if (AnimIndex == "")
                return "---";
            return AnimIndex;
        }
    }

    [Flags]
    public enum SpriteFlip
    {
        None = 0,
        Horiz = 1,
        Vert = 2
    }

    [Serializable]
    public abstract class AnimDataBase
    {
        public abstract GraphicsManager.AssetType AssetType { get; }
        public abstract string AnimIndex { get; set; }
        /// <summary>
        /// Time spent on each frame of animation, in frames (time unit)
        /// </summary>
        public int FrameTime { get; set; }
        public int StartFrame { get; set; }
        [SharedRow]
        public int EndFrame { get; set; }
        public Dir8 AnimDir { get; set; }
        public byte Alpha { get; set; }
        public SpriteFlip AnimFlip { get; set; }

        public AnimDataBase()
            : this("", 1) { }
        public AnimDataBase(string animIndex, int frameTime)
            : this(animIndex, frameTime, -1, -1) { }
        public AnimDataBase(string animIndex, int frameTime, Dir8 dir)
            : this(animIndex, frameTime, -1, -1, 255, dir) { }
        public AnimDataBase(string animIndex, int frameTime, int startFrame, int endFrame)
            : this(animIndex, frameTime, startFrame, endFrame, 255) { }
        public AnimDataBase(string animIndex, int frameTime, int startFrame, int endFrame, byte alpha)
            : this(animIndex, frameTime, startFrame, endFrame, alpha, Dir8.None) { }
        public AnimDataBase(string animIndex, int frameTime, int startFrame, int endFrame, byte alpha, Dir8 dir)
        {
            AnimIndex = animIndex;
            FrameTime = frameTime;
            Alpha = alpha;
            StartFrame = startFrame;
            EndFrame = endFrame;
            AnimDir = dir;
        }
        public AnimDataBase(AnimDataBase other)
        {
            LoadFrom(other);
        }

        public abstract AnimDataBase Clone();

        public virtual void LoadFrom(AnimDataBase other)
        {
            AnimIndex = other.AnimIndex;
            FrameTime = other.FrameTime;
            Alpha = other.Alpha;
            StartFrame = other.StartFrame;
            EndFrame = other.EndFrame;
            AnimDir = other.AnimDir;
            AnimFlip = other.AnimFlip;
        }

        public int GetTotalFrames(int totalFrames)
        {
            int frameStart = (StartFrame > -1) ? StartFrame : 0;
            int frameEnd = (EndFrame > -1) ? EndFrame + 1 : totalFrames;
            int actualTotal = frameEnd - frameStart;
            return Math.Max(1, actualTotal);
        }

        public int GetCurrentFrame(FrameTick time, int totalFrames)
        {
            int frameStart = (StartFrame > -1) ? StartFrame : 0;
            int frameEnd = (EndFrame > -1) ? EndFrame + 1 : totalFrames;
            int actualTotal = frameEnd - frameStart;
            if (actualTotal <= 1)
                return frameStart;

            return (int)(time.ToFrames() / FrameTime % actualTotal) + frameStart;
        }

        public int GetCurrentFrame(ulong time, int totalFrames)
        {
            int frameStart = (StartFrame > -1) ? StartFrame : 0;
            int frameEnd = (EndFrame > -1) ? EndFrame + 1 : totalFrames;
            int actualTotal = frameEnd - frameStart;
            if (actualTotal <= 1)
                return frameStart;

            return (int)(FrameTick.TickToFrames(time) / (ulong)FrameTime % (ulong)actualTotal) + frameStart;
        }

        public Dir8 GetDrawDir(Dir8 inputDir)
        {
            if (AnimDir != Dir8.None)
                return AnimDir;
            if (inputDir != Dir8.None)
                return inputDir;
            return Dir8.Down;
        }

        public override string ToString()
        {
            return "[" + (AnimIndex == "" ? "\"---\"" : "\""+AnimIndex+"\"") + " Frames: " + FrameTime + " ]";
        }

        public bool Equals(AnimDataBase other)
        {
            if (other == null)
                return false;

            if (AnimIndex != other.AnimIndex)
                return false;
            if (FrameTime == other.FrameTime)
                return false;
            if (Alpha == other.Alpha)
                return false;
            if (StartFrame == other.StartFrame)
                return false;
            if (EndFrame == other.EndFrame)
                return false;
            if (AnimDir == other.AnimDir)
                return false;
            if (AnimFlip == other.AnimFlip)
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            return (obj != null) && Equals(obj as AnimDataBase);
        }

        public override int GetHashCode()
        {
            return AnimIndex.GetHashCode() ^ FrameTime.GetHashCode();
        }
    }
}
