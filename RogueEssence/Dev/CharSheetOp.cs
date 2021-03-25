using System;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using RogueEssence.Content;

namespace RogueEssence.Dev
{
    [Serializable]
    public abstract class CharSheetOp
    {
        public abstract int[] Anims { get; }
        public abstract string Name { get; }
        public abstract void Apply(CharSheet sheet, int anim);
    }

}
