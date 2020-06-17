namespace AABB
{
    using System;
    using RogueElements;
	


	/// <summary>
	/// Represents a physical body in the world that can collide with others.
	/// </summary>
	public interface IBox : IObstacle
	{
		#region Movements

		/// <summary>
		/// Tries to move the box to specified coordinates with collisition detection.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="filter">Filter.</param>
        IMovement Move(int x, int y, Func<ICollision, ICollisionResponse> filter);

		/// <summary>
		/// Simulate the move of the box to specified coordinates with collisition detection. The boxe's position isn't
		/// altered.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="filter">Filter.</param>
        IMovement Simulate(int x, int y, Func<ICollision, ICollisionResponse> filter);

		#endregion
        
	}


    /// <summary>
    /// Represents a body in the world that can be collided on.
    /// </summary>
    public interface IObstacle
    {
        #region Properties

        /// <summary>
        /// Gets the top left corner X coordinate of the box.
        /// </summary>
        /// <value>The x.</value>
        int X { get; }

        /// <summary>
        /// Gets the top left corner Y coordinate of the box.
        /// </summary>
        /// <value>The y.</value>
        int Y { get; }

        /// <summary>
        /// Gets the width of the box.
        /// </summary>
        /// <value>The width.</value>
        int Width { get; }

        /// <summary>
        /// Gets the height of the box.
        /// </summary>
        /// <value>The height.</value>
        int Height { get; }

        /// <summary>
        /// Gets the bounds of the box.
        /// </summary>
        /// <value>The bounds.</value>
        Rect Bounds { get; }

        #endregion

        uint Tags { get; }
    }
}

