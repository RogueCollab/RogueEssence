namespace AABB
{
    using System;
	
    using RogueElements;


    public struct Multiplier : IEquatable<Multiplier>
    {
        public int Numerator;
        public int Denominator;

        public Multiplier(int num, int den)
        {
            Numerator = num;
            Denominator = den;
        }

        public static Multiplier MinValue { get { return new Multiplier(0, 0); } }
        public static Multiplier MaxValue { get { return new Multiplier(1, 0); } }

        public override bool Equals(object obj)
        {
            if (obj is Multiplier)
            {
                return Equals((Multiplier)obj);
            }

            return false;
        }
        public bool Equals(Multiplier other)
        {
            return (this == other);
        }

        public static bool operator ==(Multiplier a, Multiplier b)
        {
            return (a.Numerator == b.Numerator) && (a.Denominator == b.Denominator);
        }

        public static bool operator !=(Multiplier a, Multiplier b)
        {
            return !(a == b);
        }

        public static bool operator <(Multiplier a, Multiplier b)
        {
            if (a == MinValue || b == MaxValue)
                return a != b;
            else if (a == MaxValue || b == MinValue)
                return false;

            if ((b.Denominator < 0) != (a.Denominator < 0))
                return (a.Numerator * b.Denominator > b.Numerator * a.Denominator);
            else
                return (a.Numerator * b.Denominator < b.Numerator * a.Denominator);
        }

        public static bool operator >(Multiplier a, Multiplier b)
        {
            if (a == MinValue || b == MaxValue)
                return false;
            else if (a == MaxValue || b == MinValue)
                return a != b;

            if ((b.Denominator < 0) != (a.Denominator < 0))
                return (a.Numerator * b.Denominator < b.Numerator * a.Denominator);
            else
                return (a.Numerator * b.Denominator > b.Numerator * a.Denominator);
        }

        public override int GetHashCode()
        {
            return Numerator.GetHashCode() ^ Numerator.GetHashCode();
        }

        public static Multiplier Min(Multiplier value1, Multiplier value2)
        {
            if (value1 < value2)
                return value1;
            else
                return value2;
        }

        public static Multiplier Max(Multiplier value1, Multiplier value2)
        {
            if (value1 > value2)
                return value1;
            else
                return value2;
        }
    }
	/// <summary>
	/// Represents a hit point out of a collision.
	/// </summary>
	public interface IHit
	{
		/// <summary>
		/// Gets the collided box.
		/// </summary>
		/// <value>The box.</value>
		IObstacle Box { get; }

		/// <summary>
		/// Gets the normal vector of the collided box side.
		/// </summary>
		/// <value>The normal.</value>
		Dir4 Normal { get;  }

		/// <summary>
		/// Gets the amount of movement needed from origin to get the impact position.
		/// </summary>
		/// <value>The amount.</value>
        Multiplier Amount { get; }

		/// <summary>
		/// Gets the impact position.
		/// </summary>
		/// <value>The position.</value>
		Loc Position { get;  }

		/// <summary>
		/// Indicates whether the hit point is nearer than an other from a given point. Warning: this should only be used
		/// for multiple calculation of the same box movement (amount is compared first, then distance).
		/// </summary>
		/// <returns><c>true</c>, if nearest was ised, <c>false</c> otherwise.</returns>
		/// <param name="than">Than.</param>
		/// <param name="from">From.</param>
		bool IsNearest(IHit than, Loc from);
	}
}

