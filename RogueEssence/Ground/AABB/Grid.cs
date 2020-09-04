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

        public IEnumerable<Cell> QueryCells(int x, int y, int w, int h)
        {
            List<Cell> result = new List<Cell>();
            if (w == 0 && h == 0)
                return result;

			var minX = (x / this.CellSize);
			var minY = (y / this.CellSize);
			var maxX = ((x + w - 1) / this.CellSize);
			var maxY = ((y + h - 1) / this.CellSize);

			minX = Math.Max(0, minX);
			minY = Math.Max(0, minY);
			maxX = Math.Min(this.Columns - 1, maxX);
			maxY = Math.Min(this.Rows - 1, maxY);

			for (int ix = minX; ix <= maxX; ix++)
			{
				for (int iy = minY; iy <= maxY; iy++)
				{
					var cell = Cells[ix, iy];

					if (cell == null)
					{
						cell = new Cell(ix,iy,CellSize);
						Cells[ix, iy] = cell;
					}

					result.Add(cell);
				}
			}

			return result;

		}

        public IEnumerable<IObstacle> QueryBoxes(int x, int y, int w, int h)
		{
			var cells = this.QueryCells(x, y, w, h);

			return cells.SelectMany((cell) => cell.Children).Distinct();
		}

		public void Add(IObstacle box)
		{
			var cells = this.QueryCells(box.X, box.Y, box.Width, box.Height);

			foreach (var cell in cells)
			{
				if(!cell.Contains(box))
					cell.Add(box);
			}
		}

		public void Update(IObstacle box, Rect from)
		{
			var fromCells = this.QueryCells(from.X, from.Y, from.Width, from.Height);
			var removed = false;
			foreach (var cell in fromCells)
			{
				removed |= cell.Remove(box);
			}

			if(removed)
				this.Add(box);
		}

		public bool Remove(IObstacle box)
		{
			var cells = this.QueryCells(box.X, box.Y, box.Width, box.Height);

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

