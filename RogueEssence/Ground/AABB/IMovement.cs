namespace AABB
{
    using System.Collections.Generic;
    using RogueElements;


	public interface IMovement
	{
		IEnumerable<IHit> Hits { get; }

		bool HasCollided { get; }

		Rect Origin { get; }

		Rect Goal { get; }

		Rect Destination { get; }
	}
}

