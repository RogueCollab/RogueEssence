namespace RogueEssence.Dungeon
{
    public struct Multiplier
    {
        public int Numerator;
        public int Denominator;

        public Multiplier(int num, int den)
        {
            Numerator = num;
            Denominator = den;
        }


        public void AddMultiplier(int num, int den)
        {
            if (num == 0)
            {
                Numerator = 0;
                Denominator = 1;
            }
            else if (num < 0)
            {
                Numerator = -1;
                Denominator = 1;
            }
            else if (Numerator > 0)
            {
                Numerator *= num;
                Denominator *= den;
                for (int ii = 5; ii > 1; ii--)
                {
                    if (Numerator % ii == 0 && Denominator % ii == 0)
                    {
                        Numerator /= ii;
                        Denominator /= ii;
                    }
                }
            }
        }

        public bool IsNeutralized()
        {
            return Numerator <= 0;
        }
        public int Multiply(int baseAmt)
        {
            if (Numerator == -1)
                return -1;
            else
                return baseAmt * Numerator / Denominator;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Numerator, Denominator);
        }
    }

}
