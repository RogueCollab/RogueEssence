namespace AABB
{
    using RogueElements;


	public class TouchResponse : ICollisionResponse
	{
		public TouchResponse(ICollision collision)
		{
			this.Destination = new Rect(collision.Hit.Position, collision.Goal.Size);
		}

		public Rect Destination { get; private set; }
	}
}

