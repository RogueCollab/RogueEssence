namespace AABB
{
    using RogueElements;

    public class BounceResponse : ICollisionResponse
	{
		public BounceResponse(ICollision collision)
		{
            var velocity = (collision.Goal.Start - collision.Origin.Start);
            var vert = collision.Hit.Normal.ToAxis() == Axis4.Vert;
            var diff = velocity * collision.Hit.Amount.Numerator / collision.Hit.Amount.Denominator;
            var bouncePos = collision.Origin.Start + diff * 2 - velocity;
            var endLoc = vert ? new Loc(collision.Goal.X, bouncePos.Y) : new Loc(bouncePos.X, collision.Goal.Y);

            this.Destination = new Rect(endLoc, collision.Goal.Size);
		}

		public Rect Destination { get; private set; }
	}
}

