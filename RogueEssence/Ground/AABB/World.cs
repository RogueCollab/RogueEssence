namespace AABB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    
    using RogueElements;


    public class World : IWorld
    {
        public World(int width, int height)
        {
            Width = width;
            Height = height;

            boxes = new List<IBox>();
        }

        public Rect Bounds { get { return new Rect(0, 0, Width, Height); } }

        #region Boxes

        public int Width { get; private set; }

        public int Height { get; private set; }

        private List<IBox> boxes;

        public IBox Create(int x, int y, int width, int height)
        {
            var box = new Box(this, x, y, width, height);
            boxes.Add(box);
            return box;
        }

        public IEnumerable<IObstacle> FindPossible(int x, int y, int w, int h)
        {
            x = Math.Max(0, Math.Min(x, this.Bounds.Right - w));
            y = Math.Max(0, Math.Min(y, this.Bounds.Bottom - h));

            Rect testRect = new Rect(x, y, w, h);
            return boxes.Where((box) => testRect.Intersects(box.Bounds));
        }

        public IEnumerable<IObstacle> FindPossible(Rect area)
        {
            return this.FindPossible(area.X, area.Y, area.Width, area.Height);
        }

        public bool Remove(IBox box)
        {
            return boxes.Remove(box);
        }

        public void Update(IBox box, Rect from)
        {
            //no need to update any caches
        }

        #endregion

        #region Hits

        public IHit Hit(Loc point, IEnumerable<IObstacle> ignoring = null)
        {
            var boxes = this.FindPossible(point.X, point.Y, 0, 0);

            if (ignoring != null)
            {
                boxes = boxes.Except(ignoring);
            }

            foreach (var other in boxes)
            {
                var hit = AABB.Hit.Resolve(point, other);

                if (hit != null)
                {
                    return hit;
                }
            }

            return null;
        }

        public IHit Hit(Loc origin, Loc destination, IEnumerable<IObstacle> ignoring = null)
        {
            var min = Loc.Min(origin, destination);
            var max = Loc.Max(origin, destination);

            var wrap = new Rect(min, max - min);
            var boxes = this.FindPossible(wrap.X, wrap.Y, wrap.Width, wrap.Height);

            if (ignoring != null)
            {
                boxes = boxes.Except(ignoring);
            }

            IHit nearest = null;

            foreach (var other in boxes)
            {
                var hit = AABB.Hit.Resolve(origin, destination, other);

                if (hit != null && (nearest == null || hit.IsNearest(nearest, origin)))
                {
                    nearest = hit;
                }
            }

            return nearest;
        }

        public IHit Hit(Rect origin, Rect destination, IEnumerable<IObstacle> ignoring = null)
        {
            var wrap = new Rect(origin, destination);
            var boxes = this.FindPossible(wrap.X, wrap.Y, wrap.Width, wrap.Height);

            if (ignoring != null)
            {
                boxes = boxes.Except(ignoring);
            }

            IHit nearest = null;

            foreach (var other in boxes)
            {
                var hit = AABB.Hit.Resolve(origin, destination, other);

                if (hit != null && (nearest == null || hit.IsNearest(nearest, origin.Start)))
                {
                    nearest = hit;
                }
            }

            return nearest;
        }

        #endregion

        #region Movements

        public IMovement Simulate(IBox box, int x, int y, Func<ICollision, ICollisionResponse> filter)
        {
            var origin = box.Bounds;
            var destination = new Rect(x, y, box.Width, box.Height);

            var hits = new List<IHit>();

            var result = new Movement()
            {
                Origin = origin,
                Goal = destination,
                Destination = this.Simulate(hits, new List<IObstacle>() { box }, box, origin, destination, filter),
                Hits = hits,
            };

            return result;
        }

        private Rect Simulate(List<IHit> hits, List<IObstacle> ignoring, IBox box, Rect origin, Rect destination, Func<ICollision, ICollisionResponse> filter)
        {
            var nearest = this.Hit(origin, destination, ignoring);

            if (nearest != null)
            {
                hits.Add(nearest);

                var impact = new Rect(nearest.Position, origin.Size);
                var collision = new Collision() { Box = box, Hit = nearest, Goal = destination, Origin = origin };
                var response = filter(collision);

                if (response != null && destination != response.Destination)
                {
                    ignoring.Add(nearest.Box);
                    return this.Simulate(hits, ignoring, box, impact, response.Destination, filter);
                }
            }

            return destination;
        }

        #endregion

        #region Diagnostics

        public void DrawDebug(int x, int y, int w, int h, Action<int, int, int, int, float> drawCell, Action<IObstacle> drawBox, Action<string, int, int, float> drawString)
        {
            // Drawing boxes
            foreach (var box in boxes)
                drawBox(box);
        }

        #endregion
    }
}

