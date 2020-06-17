namespace AABB
{
    using RogueElements;

	public class CrossResponse : ICollisionResponse
	{
		public CrossResponse(ICollision collision)
		{
			this.Destination = collision.Goal;
		}


		public Rect Destination { get; private set; }
	}
}

