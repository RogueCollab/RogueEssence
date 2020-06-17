namespace AABB
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	
    using RogueElements;


	public class GridBlockWorld : IWorld
	{
        public GridBlockWorld(int width, int height, int cellDivs, int divSize)
		{
            this.divSize = divSize;
            int cellSize = cellDivs * divSize;
			var iwidth = (width - 1) / cellSize + 1;
			var iheight = (height - 1) / cellSize + 1;

			this.grid = new Grid(iwidth, iheight, cellSize);

            obstacles = new Obstacle[iwidth * cellDivs][];
            for (int ii = 0; ii < obstacles.Length; ii++)
            {
                obstacles[ii] = new Obstacle[iheight * cellDivs];
                for (int jj = 0; jj < obstacles[ii].Length; jj++)
                    obstacles[ii][jj] = new Obstacle(ii * divSize, jj * divSize, divSize, divSize);
            }
		}

        public Rect Bounds { get { return new Rect(0, 0, this.grid.Width, this.grid.Height); } }

		#region Boxes

		private Grid grid;
        private int divSize;
        private Obstacle[][] obstacles;

        public uint GetObstacle(int x, int y)
        {
            return obstacles[x][y].Tags;
        }
        public void SetObstacle(int x, int y, uint tags)
        {
            obstacles[x][y].Tags = obstacles[x][y].Tags;
        }

        public IBox Create(int x, int y, int width, int height)
		{
			var box = new Box(this, x, y, width, height);
			this.grid.Add(box);
			return box;
		}

        public IEnumerable<IObstacle> FindPossible(int x, int y, int w, int h)
		{
			x = Math.Max(0, Math.Min(x, this.Bounds.Right - w));
			y = Math.Max(0, Math.Min(y, this.Bounds.Bottom - h));


            foreach (IObstacle obstacle in this.grid.QueryBoxes(x, y, w, h))
                yield return obstacle;

            var minX = (x / divSize);
            var minY = (y / divSize);
            var maxX = ((x + w) / divSize);
            var maxY = ((y + h) / divSize);

            for (int ii = minX; ii <= maxX; ii++)
            {
                for (int jj = minY; jj <= maxY; jj++)
                {
                    if (obstacles[ii][jj].Tags > 0)
                        yield return obstacles[ii][jj];
                }
            }
        }

		public IEnumerable<IObstacle> FindPossible(Rect area)
		{
			return this.FindPossible(area.X, area.Y, area.Width, area.Height);
		}

		public bool Remove(IBox box)
		{
			return this.grid.Remove(box);
		}

		public void Update(IBox box, Rect from)
		{
			this.grid.Update(box, from);
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

				if (hit != null && (nearest == null || hit.IsNearest(nearest,origin)))
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

		public void DrawDebug(int x, int y, int w, int h, Action<int,int,int,int,float> drawCell, Action<IObstacle> drawBox, Action<string,int,int, float> drawString)
		{
			// Drawing boxes
			var boxes = this.grid.QueryBoxes(x, y, w, h);
			foreach (var box in boxes)
			{
				drawBox(box);
			}

			// Drawing cells
			var cells = this.grid.QueryCells(x, y, w, h);
			foreach (var cell in cells)
			{
				var count = cell.Count();
				var alpha = count > 0 ? 1f : 0.4f;
				drawCell((int)cell.Bounds.X, (int)cell.Bounds.Y, (int)cell.Bounds.Width, (int)cell.Bounds.Height, alpha);
				drawString(count.ToString(), (int)cell.Bounds.Center.X, (int)cell.Bounds.Center.Y,alpha);
			}
		}

		#endregion
	}


    /// <summary>
    /// Represents a physical body in the world that can collide with others.
    /// </summary>
    [Serializable]
    public class Obstacle : IObstacle
    {
        #region Constructors 

        public Obstacle(int x, int y, int width, int height)
        {
            this.bounds = new Rect(x, y, width, height);
        }

        #endregion

        #region Fields

        private Rect bounds;

        #endregion

        #region Properties

        public Rect Bounds
        {
            get { return bounds; }
        }

        public int Height { get { return Bounds.Height; } }

        public int Width { get { return Bounds.Width; } }

        public int X { get { return Bounds.X; } }

        public int Y { get { return Bounds.Y; } }

        #endregion


        public uint Tags { get; set; }
    }

}

