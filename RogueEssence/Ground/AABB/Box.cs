namespace AABB
{
    using System;
    using RogueElements;



    public class Box : IBox
	{
		#region Constructors 

        public Box(IWorld world, int x, int y, int width, int height)
		{
			this.world = world;
			this.bounds = new Rect(x, y, width, height);
		}

		#endregion

		#region Fields

		private IWorld world;

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

		#region Movements

        public IMovement Simulate(int x, int y, Func<ICollision, ICollisionResponse> filter)
		{
			return world.Simulate(this, x, y, filter);
		}

        public IMovement Move(int x, int y, Func<ICollision, ICollisionResponse> filter)
		{
			var movement = this.Simulate(x, y, filter);
			this.bounds.X = movement.Destination.X;
			this.bounds.Y = movement.Destination.Y;
			this.world.Update(this, movement.Origin);
			return movement;
		}

		#endregion

        public uint Tags { get; set; }
	}
}

