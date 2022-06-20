using Microsoft.Xna.Framework;
using RogueElements;

namespace RogueEssence
{
    public static class XNAExt
    {

        public static Vector2 ToVector2(this Loc loc)
        {
            return new Vector2(loc.X, loc.Y);
        }
        public static Loc ToLoc(this Vector2 loc)
        {
            return new Loc((int)loc.X, (int)loc.Y);
        }
    }
}
