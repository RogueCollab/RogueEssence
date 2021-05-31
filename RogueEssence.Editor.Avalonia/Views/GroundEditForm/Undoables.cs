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
        public DrawBlockUndo(Dictionary<Loc, uint> brush) : base(brush)
        {
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
        private Rect coveredRect;

        public DrawTextureUndo(int layer, Dictionary<Loc, AutoTile> brush, Rect coveredRect) : base(brush)
        {
            this.layer = layer;
            this.coveredRect = coveredRect;
        }

        protected override AutoTile GetValue(Loc loc)
        {
            return ZoneManager.Instance.CurrentGround.Layers[layer].Tiles[loc.X][loc.Y];
        }
        protected override void SetValue(Loc loc, AutoTile val)
        {
            ZoneManager.Instance.CurrentGround.Layers[layer].Tiles[loc.X][loc.Y] = val;
        }
        protected override void ValuesFinished()
        {
            //now recompute all tiles within the multiselect rectangle + 1
            Rect bounds = coveredRect;
            bounds.Inflate(1, 1);
            ZoneManager.Instance.CurrentGround.Layers[layer].CalculateAutotiles(ZoneManager.Instance.CurrentGround.Rand.FirstSeed, bounds.Start, bounds.Size);
        }
    }

    public abstract class DrawUndo<T> : Undoable
    {
        private Dictionary<Loc, T> brush;
        private Dictionary<Loc, T> prevStates;
        public DrawUndo(Dictionary<Loc, T> brush)
        {
            this.brush = brush;
        }

        protected abstract T GetValue(Loc loc);
        protected abstract void SetValue(Loc loc, T val);
        protected virtual void ValuesFinished() { }

        public override void Apply()
        {
            prevStates = new Dictionary<Loc, T>();

            foreach (Loc loc in brush.Keys)
                prevStates[loc] = GetValue(loc);

            Redo();
        }

        public override void Redo()
        {
            foreach (Loc loc in brush.Keys)
                SetValue(loc, brush[loc]);
            ValuesFinished();
        }

        public override void Undo()
        {
            foreach (Loc loc in prevStates.Keys)
                SetValue(loc, prevStates[loc]);
            ValuesFinished();
        }
    }

}
