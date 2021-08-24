using System;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RogueEssence.Dev
{
    public abstract class CanvasStroke<T>
    {
        public abstract T GetBrush(Loc loc);
        public abstract Rect CoveredRect { get; }

        public abstract void SetEnd(Loc loc);

        public abstract bool IncludesLoc(Loc loc);

        public abstract IEnumerable<Loc> GetLocs();



        public delegate CanvasStroke<T> StrokeCreator();
        public delegate void StrokeAction(CanvasStroke<T> stroke);
        public static void ProcessCanvasInput(InputManager input, Loc tileCoords, bool inWindow, StrokeCreator createStroke, StrokeCreator deleteStroke, StrokeAction strokeAction, ref CanvasStroke<T> pendingStroke)
        {
            if (input.JustPressed(FrameInput.InputType.LeftMouse) && inWindow)
                pendingStroke = createStroke();
            else if (pendingStroke != null && input[FrameInput.InputType.LeftMouse])
                pendingStroke.SetEnd(tileCoords);
            else if (pendingStroke != null && input.JustReleased(FrameInput.InputType.LeftMouse))
            {
                strokeAction(pendingStroke);
                pendingStroke = null;
            }
            else if (input.JustPressed(FrameInput.InputType.RightMouse) && inWindow)
                pendingStroke = deleteStroke();
            else if (pendingStroke != null && input[FrameInput.InputType.RightMouse])
                pendingStroke.SetEnd(tileCoords);
            else if (pendingStroke != null && input.JustReleased(FrameInput.InputType.RightMouse))
            {
                strokeAction(pendingStroke);
                pendingStroke = null;
            }
        }
    }
    public class RectStroke<T> : CanvasStroke<T>
    {
        private T brush;
        private Loc start;
        private Loc end;

        public override T GetBrush(Loc loc) { return brush; }

        private Rect coveredRect;
        public override Rect CoveredRect { get { return coveredRect; } }
        public RectStroke(Loc start, T brush)
        {
            this.brush = brush;
            this.start = start;
            SetEnd(start);
        }

        public override bool IncludesLoc(Loc loc)
        {
            return coveredRect.Contains(loc);
        }
        public override void SetEnd(Loc loc)
        {
            this.end = loc;
            Rect resultRect = Rect.FromPoints(start, end);
            if (resultRect.Size.X <= 0)
            {
                resultRect.Start = new Loc(resultRect.Start.X + resultRect.Size.X, resultRect.Start.Y);
                resultRect.Size = new Loc(-resultRect.Size.X + 1, resultRect.Size.Y);
            }
            else
                resultRect.Size = new Loc(resultRect.Size.X + 1, resultRect.Size.Y);

            if (resultRect.Size.Y <= 0)
            {
                resultRect.Start = new Loc(resultRect.Start.X, resultRect.Start.Y + resultRect.Size.Y);
                resultRect.Size = new Loc(resultRect.Size.X, -resultRect.Size.Y + 1);
            }
            else
                resultRect.Size = new Loc(resultRect.Size.X, resultRect.Size.Y + 1);
            coveredRect = resultRect;
        }

        public override IEnumerable<Loc> GetLocs()
        {
            for (int yy = coveredRect.Y; yy < coveredRect.Bottom; yy++)
            {
                for (int xx = coveredRect.X; xx < coveredRect.Right; xx++)
                    yield return new Loc(xx, yy);
            }
        }
    }

    public class DrawStroke<T> : CanvasStroke<T>
    {
        private T brush;
        private HashSet<Loc> locs;
        
        public override T GetBrush(Loc loc) { return brush; }

        private Rect coveredRect;
        public override Rect CoveredRect { get { return coveredRect; } }
        public DrawStroke(Loc start, T brush)
        {
            this.brush = brush;
            locs = new HashSet<Loc>();
            coveredRect = new Rect(start, Loc.Zero);
            SetEnd(start);
        }

        public override bool IncludesLoc(Loc loc)
        {
            return locs.Contains(loc);
        }
        public override void SetEnd(Loc loc)
        {
            locs.Add(loc);
            coveredRect = Rect.IncludeLoc(coveredRect, loc);
        }

        public override IEnumerable<Loc> GetLocs()
        {
            foreach (Loc loc in locs)
                yield return loc;
        }
    }


    public class ClusterStroke<T> : CanvasStroke<T>
    {
        private T[][] brush;
        private Loc loc;

        public override T GetBrush(Loc loc)
        {
            Loc checkLoc = loc - this.loc;
            return brush[checkLoc.X][checkLoc.Y];
        }

        private Rect coveredRect;
        public override Rect CoveredRect { get { return coveredRect; } }
        public ClusterStroke(Loc start, T[][] brush)
        {
            this.brush = brush;
            SetEnd(start);
        }

        public override bool IncludesLoc(Loc loc)
        {
            return coveredRect.Contains(loc);
        }
        public override void SetEnd(Loc loc)
        {
            this.loc = loc;
            coveredRect = new Rect(loc, new Loc(brush.Length, brush[0].Length));
        }

        public override IEnumerable<Loc> GetLocs()
        {
            for (int xx = 0; xx < brush.Length; xx++)
            {
                for (int yy = 0; yy < brush[0].Length; yy++)
                    yield return loc + new Loc(xx, yy);
            }
        }
    }



    public class FillStroke<T> : CanvasStroke<T>
    {
        private T brush;
        private Loc loc;

        public override T GetBrush(Loc loc)
        {
            return brush;
        }

        private Rect coveredRect;
        public override Rect CoveredRect { get { return coveredRect; } }
        public FillStroke(Loc start, T brush)
        {
            this.brush = brush;
            SetEnd(start);
        }

        public override bool IncludesLoc(Loc loc)
        {
            return coveredRect.Contains(loc);
        }
        public override void SetEnd(Loc loc)
        {
            this.loc = loc;
            coveredRect = new Rect(loc, Loc.One);
        }

        public override IEnumerable<Loc> GetLocs()
        {
            yield return loc;
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
