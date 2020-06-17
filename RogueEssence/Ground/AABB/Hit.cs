namespace AABB
{
	using System;
    using RogueElements;


	public class Hit : IHit
	{
		public Hit()
		{
            this.Normal = Dir4.None;
			this.Amount = new Multiplier(1,1);
		}

		public IObstacle Box { get; set; }

        public Dir4 Normal { get; set; }

        public Multiplier Amount { get; set; }

		public Loc Position { get; set; }

		#region Public functions

		public static IHit Resolve(Rect origin, Rect destination, IObstacle other)
		{
			var result = Resolve(origin,destination, other.Bounds);
			if (result != null) result.Box = other;
			return result;
		}

        public static IHit Resolve(Loc origin, Loc destination, IObstacle other)
		{
			var result = Resolve(origin, destination, other.Bounds);
			if (result != null) result.Box = other;
			return result;
		}

		public static Hit Resolve(Rect origin, Rect destination, Rect other)
		{
			var broadphaseArea = Rect.Union(origin,destination);

			if (broadphaseArea.Intersects(other) || broadphaseArea.Contains(other))
			{
				return ResolveNarrow(origin, destination, other);
			}

			return null;
		}

        public static Hit Resolve(Loc origin, Loc destination, Rect other)
		{
            var min = Loc.Min(origin, destination);
            var size = Loc.Max(origin, destination) - min;

			var broadphaseArea = new Rect(min, size);

			if (broadphaseArea.Intersects(other) || broadphaseArea.Contains(other))
			{
				return ResolveNarrow(origin, destination, other);
			}

			return null;
		}

        public static IHit Resolve(Loc point, IObstacle other)
		{
			if (other.Bounds.Contains(point))
			{
				var outside = PushOutside(point, other.Bounds);
				return new Hit()
				{
                    Amount = new Multiplier(0, 1),
					Box = other,
					Position = outside.Item1,
					Normal = outside.Item2,
				};
			}

			return null;
		}

		#endregion

        private static Tuple<Loc, Dir4> PushOutside(Loc origin, Rect other)
		{
			var position = origin;
            var normal = Dir4.None;

			var top = origin.Y - other.Top;
			var bottom = other.Bottom - origin.Y;
			var left = origin.X - other.Left;
			var right = other.Right - origin.X;

			var min = Math.Min(top, Math.Min(bottom, Math.Min(right, left)));

			if ((min - top) == 0)
			{
                normal = Dir4.Up;
                position = new Loc(position.X, other.Top);
			}
			else if ((min - bottom) == 0)
			{
                normal = Dir4.Down;
                position = new Loc(position.X, other.Bottom);
			}
			else if ((min - left) == 0)
			{
                normal = Dir4.Left;
                position = new Loc(other.Left, position.Y);
			}
			else if ((min - right) == 0)
			{
                normal = Dir4.Right;
                position = new Loc(other.Right, position.Y);
			}

            return new Tuple<Loc, Dir4>(position, normal);
		}

        private static Tuple<Rect, Dir4> PushOutside(Rect origin, Rect other)
		{
			var position = origin;
            var normal = Dir4.None;

			var top = origin.Center.Y - other.Top;
			var bottom = other.Bottom - origin.Center.Y;
			var left = origin.Center.X - other.Left;
			var right = other.Right - origin.Center.X;

			var min = Math.Min(top, Math.Min(bottom, Math.Min(right, left)));

			if ((min - top) == 0)
			{
                normal = Dir4.Up;
                position.Start = new Loc(position.X, other.Top - position.Height);
			}
			else if ((min - bottom) == 0)
			{
                normal = Dir4.Down;
                position.Start = new Loc(position.X, other.Bottom);
			}
			else if ((min - left) == 0)
			{
                normal = Dir4.Left;
                position.Start = new Loc(other.Left - position.Width, position.Y);
			}
			else if ((min - right) == 0)
			{
                normal = Dir4.Right;
                position.Start = new Loc(other.Right, position.Y);
			}

            return new Tuple<Rect, Dir4>(position, normal);
		}

		private static Hit ResolveNarrow(Rect origin, Rect destination, Rect other)
		{
			// if starts inside, push it outside at the neareast place
			if (other.Contains(origin) || other.Intersects(origin))
			{
				var outside = PushOutside(origin, other);
				return new Hit()
				{
					Amount = new Multiplier(0, 1),
					Position = outside.Item1.Start,
					Normal = outside.Item2,
				};
			}
            return ResolveNarrow(origin.Start, origin.End, destination.Start, other);
		}

        private static Hit ResolveNarrow(Loc origin, Loc destination, Rect other)
		{
			// if starts inside, push it outside at the neareast place
			if (other.Contains(origin))
			{
				var outside = PushOutside(origin, other);
				return new Hit()
				{
                    Amount = new Multiplier(0, 1),
					Position = outside.Item1,
					Normal = outside.Item2,
				};
			}

            return ResolveNarrow(origin, origin, destination, other);
		}

        private static Hit ResolveNarrow(Loc origin1, Loc origin2, Loc destination, Rect other)
        {
            var velocity = (destination - origin1);

            Loc invEntry, invExit;
            Multiplier entryX, entryY, exitX, exitY;

            if (velocity.X > 0)
            {
                invEntry.X = other.Left - origin2.X;
                invExit.X = other.Right - origin1.X;
            }
            else
            {
                invEntry.X = other.Right - origin1.X;
                invExit.X = other.Left - origin2.X;
            }

            if (velocity.Y > 0)
            {
                invEntry.Y = other.Top - origin2.Y;
                invExit.Y = other.Bottom - origin1.Y;
            }
            else
            {
                invEntry.Y = other.Bottom - origin1.Y;
                invExit.Y = other.Top - origin2.Y;
            }

            if (velocity.X == 0)
            {
                entryX = Multiplier.MinValue;
                exitX = Multiplier.MaxValue;
            }
            else
            {
                entryX = new Multiplier(invEntry.X, velocity.X);
                exitX = new Multiplier(invExit.X, velocity.X);

                if (entryX > new Multiplier(1, 1))
                    entryX = Multiplier.MaxValue;
            }

            if (velocity.Y == 0)
            {
                entryY = Multiplier.MinValue;
                exitY = Multiplier.MaxValue;
            }
            else
            {
                entryY = new Multiplier(invEntry.Y, velocity.Y);
                exitY = new Multiplier(invExit.Y, velocity.Y);

                if (entryY > new Multiplier(1, 1))
                    entryY = Multiplier.MinValue;
            }

            var entryTime = Multiplier.Max(entryX, entryY);
            var exitTime = Multiplier.Min(exitX, exitY);

            if ((entryTime > exitTime || entryX == Multiplier.MinValue && entryY == Multiplier.MinValue) ||
                (entryX == Multiplier.MinValue && (origin2.X < other.Left || origin1.X > other.Right)) ||
                entryY == Multiplier.MinValue && (origin2.Y < other.Top || origin1.Y > other.Bottom))
                return null;


            var result = new Hit()
            {
                Amount = entryTime,
                Position = origin1 + velocity * entryTime.Numerator / entryTime.Denominator,
                Normal = GetNormal(invEntry, invExit, entryX, entryY),
            };

            return result;
        }

        private static Dir4 GetNormal(Loc invEntry, Loc invExit, Multiplier entryX, Multiplier entryY)
		{
            if (entryX > entryY)
                return (invEntry.X < 0) || (invEntry.X == 0 && invExit.X < 0) ? Dir4.Right : Dir4.Left;
            else if (entryX < entryY)
                return (invEntry.Y < 0 || (invEntry.Y == 0 && invExit.Y < 0)) ? Dir4.Down : Dir4.Up;
            else
                return Dir4.None;
		}


        public bool IsNearest(IHit than, Loc origin)
		{
            if (this.Amount < than.Amount)
                return true;
            else if (this.Amount > than.Amount)
                return false;
            return (this.Normal != Dir4.None && than.Normal == Dir4.None);
		}
	}
}

