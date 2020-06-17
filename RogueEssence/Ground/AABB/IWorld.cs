namespace AABB
{
	using System;
	using System.Collections.Generic;
	
	
    using RogueElements;


	/// <summary>
	/// Represents a physical world that contains AABB box colliding bodies.
	/// </summary>
	public interface IWorld
	{
		#region Boxes

		///// <summary>
		///// Create a new box in the world.
		///// </summary>
		///// <param name="x">The x coordinate.</param>
		///// <param name="y">The y coordinate.</param>
		///// <param name="width">The width.</param>
		///// <param name="height">The height.</param>
  //      IBox Create(int x, int y, int width, int height);

		///// <summary>
		///// Remove the specified box from the world.
		///// </summary>
		///// <param name="box">Box.</param>
		//bool Remove(IBox box);

		/// <summary>
		/// Update the specified box in the world (needed to be called tu update spacial hash).
		/// </summary>
		/// <param name="box">Box.</param>
		/// <param name="from">From.</param>
		void Update(IBox box, Rect from);

		#endregion

		#region Queries

		/// <summary>
		/// Find the boxes contained in the given area of the world.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="w">The width.</param>
		/// <param name="h">The height.</param>
        IEnumerable<IObstacle> FindPossible(int x, int y, int w, int h);

		/// <summary>
		/// Find the boxes contained in the given area of the world.
		/// </summary>
		/// <param name="area">Area.</param>
		IEnumerable<IObstacle> FindPossible(Rect area);

		#endregion

		#region Hits

		/// <summary>
		/// Queries the world to find the nearest colliding point from a given position.
		/// </summary>
		/// <param name="point">Point.</param>
		/// <param name="ignoring">A collection of boxes that will be ignored during hit test (optionnal).</param>
		IHit Hit(Loc point, IEnumerable<IObstacle> ignoring = null);

		/// <summary>
		/// Queries the world to find the nearest colliding position from an oriented segment.
		/// </summary>
		/// <param name="origin">Origin.</param>
		/// <param name="destination">Destination.</param>
		/// <param name="ignoring">A collection of boxes that will be ignored during hit test (optionnal).</param>
        IHit Hit(Loc origin, Loc destination, IEnumerable<IObstacle> ignoring = null);

		/// <summary>
		/// Queries the world to find the nearest colliding position from a moving rectangle.
		/// </summary>
		/// <param name="origin">Origin.</param>
		/// <param name="destination">Destination.</param>
		/// <param name="ignoring">Ignoring.</param>
		IHit Hit(Rect origin, Rect destination, IEnumerable<IObstacle> ignoring = null);

		#endregion

		#region Movements

		/// <summary>
		/// Simulate the specified box movement without moving it.
		/// </summary>
		/// <param name="box">Box.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="filter">Filter.</param>
        IMovement Simulate(IBox box, int x, int y, Func<ICollision, ICollisionResponse> filter);

		#endregion

		#region Diagnostics

		/// <summary>
		/// Draws the debug layer.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="w">The width.</param>
		/// <param name="h">The height.</param>
		/// <param name="drawCell">Draw cell.</param>
		/// <param name="drawBox">Draw box.</param>
		/// <param name="drawString">Draw string.</param>
		void DrawDebug(int x, int y, int w, int h, Action<int, int, int, int, float> drawCell, Action<IObstacle> drawBox, Action<string, int, int, float> drawString);

		#endregion
	}
}

