using System;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class BGLayer
    {
        [Dev.SubGroup]
        public MapBG BG;
    }

    [Serializable]
    public class LayeredBG : IBackgroundSprite
    {
        public List<BGLayer> Layers;

        public Loc MapLoc { get { return Loc.Zero; } }
        public int LocHeight { get { return 0; } }

        public LayeredBG()
        {
            Layers = new List<BGLayer>();
        }
        public LayeredBG(LayeredBG other)
        {
            foreach (BGLayer layer in other.Layers)
            {
                BGLayer newLayer = new BGLayer();
                newLayer.BG = new MapBG(layer.BG);
                Layers.Add(newLayer);
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch, Loc offset) { }
        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            foreach (BGLayer layer in Layers)
                layer.BG.Draw(spriteBatch, offset);
        }


        public Loc GetDrawLoc(Loc offset)
        {
            return offset;
        }
        public Loc GetSheetOffset() { return Loc.Zero; }

        public Loc GetDrawSize()
        {
            return Loc.Zero;
        }
    }
}
