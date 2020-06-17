using System;
using RogueElements;
using AABB;

namespace RogueEssence.Ground
{

    /// <summary>
    /// Represents a physical body in the world that can collide with others.
    /// </summary>
    [Serializable]
    public class GroundWall : IObstacle
    {
        #region Constructors 

        public GroundWall(int x, int y, int width, int height)
        {
            this.bounds = new Rect(x, y, width, height);
        }

        #endregion

        #region Fields
        
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
        

        public uint Tags { get; set; }
    }

}

