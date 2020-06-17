namespace AABB
{
    using RogueElements;
	using System.Collections.Generic;
	using System.Linq;


	public class Movement : IMovement
	{
		public Movement()
		{
			this.Hits = new IHit[0];
		}

		public IEnumerable<IHit> Hits { get; set; }

		public bool HasCollided { get { return this.Hits.Any(); } }

		public Rect Origin { get; set; }

		public Rect Destination { get; set; }

		public Rect Goal { get; set; }
	}
}

