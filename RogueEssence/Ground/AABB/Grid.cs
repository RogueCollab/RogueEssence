namespace AABB
{
	using System;
	using System.Collections.Generic;
    using System.Linq;
    using RogueElements;


	/// <summary>
	/// Basic spacial hashing of world's boxes.
	/// </summary>
	public class Grid
	{
		public class Cell
		{
            public Cell(int x, int y, int cellSize)
			{
				this.Bounds = new Rect(x * cellSize, y * cellSize, cellSize, cellSize);
			}

			public Rect Bounds { get; private set; }

			public IEnumerable<IObstacle> Children { get { return  this.children;}}

			private List<IObstacle> children = new List<IObstacle>();

			public void Add(IObstacle box)
			{
				this.children.Add(box);
			}

			public bool Contains(IObstacle box)
			{
				return this.children.Contains(box);
			}

			public bool Remove(IObstacle box)
			{
				return this.children.Remove(box);
			}

			public int Count()
			{
				return this.children.Count;
			}
		}

        public Grid(int width, int height, int cellSize)
		{
			this.Cells = new Cell[width, height];
			this.CellSize = cellSize;
		}

        public int CellSize { get; set; }

		#region Size

        public int Width { get { return this.Columns * CellSize; } }

        public int Height { get { return this.Rows * CellSize; } }

		public int Columns { get { return  this.Cells.GetLength(0);}}

        public int Rows { get { return this.Cells.GetLength(1); } }

		#endregion

		public Cell[,] Cells { get; private set; }

        public IEnumerable<Cell> QueryCells(int x, int y, int w, int h, bool wrap)
        {
            List<Cell> result = new List<Cell>();
            if (w == 0 && h == 0)
                return result;

			var minX = MathUtils.DivDown(x, this.CellSize);
			var minY = MathUtils.DivDown(y, this.CellSize);
			var maxX = MathUtils.DivUp(x + w, this.CellSize);
			var maxY = MathUtils.DivUp(y + h, this.CellSize);

			Loc size = new Loc(this.Columns, this.Rows);

			for (int ix = minX; ix < maxX; ix++)
			{
				for (int iy = minY; iy < maxY; iy++)
				{
					Loc testLoc = new Loc(ix, iy);
					if (wrap)
						testLoc = Loc.Wrap(testLoc, size);
					else if (!RogueElements.Collision.InBounds(this.Columns, this.Rows, testLoc))
						continue;

					var cell = Cells[testLoc.X, testLoc.Y];

					if (cell == null)
					{
						cell = new Cell(testLoc.X, testLoc.Y, CellSize);
						Cells[testLoc.X, testLoc.Y] = cell;
					}

					result.Add(cell);
				}
			}

			return result;

		}

        public IEnumerable<IObstacle> QueryBoxes(int x, int y, int w, int h, bool wrap)
		{
			var cells = this.QueryCells(x, y, w, h, wrap);

			return cells.SelectMany((cell) => cell.Children).Distinct();
		}

		public void Add(IObstacle box, bool wrap)
		{
			var cells = this.QueryCells(box.X, box.Y, box.Width, box.Height, wrap);

			foreach (var cell in cells)
			{
				if(!cell.Contains(box))
					cell.Add(box);
			}
		}

		public void Update(IObstacle box, Rect from, bool wrap)
		{
			var fromCells = this.QueryCells(from.X, from.Y, from.Width, from.Height, wrap);
			var removed = false;
			foreach (var cell in fromCells)
			{
				removed |= cell.Remove(box);
			}

			this.Add(box, wrap);
		}

		public bool Remove(IObstacle box, bool wrap)
		{
			var cells = this.QueryCells(box.X, box.Y, box.Width, box.Height, wrap);

			var removed = false;
			foreach (var cell in cells)
			{
				removed |= cell.Remove(box);
			}

			return removed;
		}

		public override string ToString()
		{
			return string.Format("[Grid: Width={0}, Height={1}, Columns={2}, Rows={3}]", Width, Height, Columns, Rows);
		}
	}
}

