using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace RogueEssence.Dev
{

    public class DrawBlockUndo : DrawUndo<uint>
    {
        private uint brush;

        public DrawBlockUndo(uint brush, List<Loc> locs) : base(locs)
        {
            this.brush = brush;
        }

        protected override uint GetBrush(int idx)
        {
            return brush;
        }
        protected override uint GetValue(Loc loc)
        {
            return ZoneManager.Instance.CurrentGround.GetObstacle(loc.X, loc.Y);
        }
        protected override void SetValue(Loc loc, uint val)
        {
            ZoneManager.Instance.CurrentGround.SetObstacle(loc.X, loc.Y, val);
        }
    }

    public class DrawTextureUndo : DrawUndo<AutoTile>
    {
        private int layer;
        private AutoTile brush;
        private Rect bounds;

        public DrawTextureUndo(int layer, AutoTile brush, List<Loc> locs, Rect bounds) : base(locs)
        {
            this.layer = layer;
            this.brush = brush;
            this.bounds = bounds;
        }

        protected override AutoTile GetBrush(int idx)
        {
            return brush;
        }
        protected override AutoTile GetValue(Loc loc)
        {
            return ZoneManager.Instance.CurrentGround.Layers[layer].Tiles[loc.X][loc.Y];
        }
        protected override void SetValue(Loc loc, AutoTile val)
        {
            ZoneManager.Instance.CurrentGround.Layers[layer].Tiles[loc.X][loc.Y] = brush.Copy();
        }
        protected override void ValuesFinished()
        {
            ZoneManager.Instance.CurrentGround.Layers[layer].CalculateAutotiles(ZoneManager.Instance.CurrentGround.Rand.FirstSeed, bounds.Start, bounds.Size);
        }
    }

    public abstract class DrawUndo<T> : Undoable
    {
        protected List<Loc> locs;
        protected List<T> prevStates;
        public DrawUndo(List<Loc> locs)
        {
            this.locs = locs;
        }

        protected abstract T GetBrush(int idx);
        protected abstract T GetValue(Loc loc);
        protected abstract void SetValue(Loc loc, T val);
        protected virtual void ValuesFinished() { }

        public override void Apply()
        {
            prevStates = new List<T>();

            for (int ii = 0; ii < locs.Count; ii++)
                prevStates.Add(GetValue(locs[ii]));

            Redo();
        }

        public override void Redo()
        {
            for (int ii = 0; ii < locs.Count; ii++)
                SetValue(locs[ii], GetBrush(ii));
            ValuesFinished();
        }

        public override void Undo()
        {
            for(int ii = 0; ii < locs.Count; ii++)
                SetValue(locs[ii], prevStates[ii]);
            ValuesFinished();
        }
    }

}
