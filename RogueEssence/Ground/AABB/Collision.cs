using RogueElements;

namespace AABB
{

	public class Collision : ICollision
	{
		public Collision()
		{
		}

		public IBox Box { get; set; }

		public IObstacle Other { get { return (this.Hit != null ? this.Hit.Box : null); } }

		public Rect Origin { get; set; }

		public Rect Goal { get; set; }

		public IHit Hit { get; set; }

        public bool HasCollided { get { return this.Hit != null; } }
	}
}

