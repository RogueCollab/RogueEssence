using System;
using RogueElements;
using AABB;
using RogueEssence.Dungeon;
using System.Collections.Generic;
using RogueEssence.Content;

namespace RogueEssence.Ground
{

    [Serializable]
    public class AnimLayer : IMapLayer
    {
        public string Name { get; set; }
        public DrawLayer Layer { get; set; }
        public bool Visible { get; set; }

        public List<GroundAnim> Anims;

        public AnimLayer(string name)
        {
            Name = name;
            Visible = true;
            Anims = new List<GroundAnim>();
        }

        protected AnimLayer(AnimLayer other)
        {
            Name = other.Name;
            Layer = other.Layer;
            Visible = other.Visible;

            Anims = new List<GroundAnim>();
            foreach (GroundAnim anim in other.Anims)
                Anims.Add(new GroundAnim(anim));
        }

        public IMapLayer Clone() { return new AnimLayer(this); }

        public void Merge(IMapLayer other)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return (Layer == DrawLayer.Top ? "[Top] " : "") + Name;
        }



        /// <summary>
        /// Convenience method for locating an anim on that map at the
        /// position specified.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public List<GroundAnim> FindAnimsAtPosition(Loc pos)
        {
            List<GroundAnim> found = new List<GroundAnim>();

            foreach (GroundAnim c in Anims)
            {
                Loc drawSize = c.GetDrawSize();
                if (RogueElements.Collision.InBounds(c.MapLoc, new Loc(Math.Max(drawSize.X, GraphicsManager.TEX_SIZE), Math.Max(drawSize.Y, GraphicsManager.TEX_SIZE)), pos))
                    found.Add(c);
            }

            return found;
        }
    }
}

