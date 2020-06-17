namespace AABB
{


    using RogueElements;


    public class SlideResponse : ICollisionResponse
	{
		public SlideResponse(ICollision collision)
		{
            var velocity = (collision.Goal.Start - collision.Origin.Start);
			var vert = collision.Hit.Normal.ToAxis() == Axis4.Vert;
            var endLoc = vert ? new Loc(collision.Goal.X, collision.Hit.Position.Y) : new Loc(collision.Hit.Position.X, collision.Goal.Y);

            this.Destination = new Rect(endLoc, collision.Goal.Size);
		}

		public Rect Destination { get; private set; }
	}
}

