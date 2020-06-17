using System;

namespace RogueEssence.Content
{
    public class AnimMath
    {

        public static int GetArc(double maxHeight, double touchdownX, double currentX)
        {
            // = (-4 * m / (n ^ 2) ) * x ^ 2 + (4 * m / n) * x
            // m = height, n = total time, x = current time
            double height = -4 * maxHeight * Math.Pow(currentX / touchdownX, 2) + 4 * maxHeight * (currentX / touchdownX);
            return (int)Math.Round(height);
        }

        public static int Lerp(int int1, int int2, double point)
        {
            return (int)Math.Round(int1 * (1 - point) + int2 * point);
        }
    }
}
