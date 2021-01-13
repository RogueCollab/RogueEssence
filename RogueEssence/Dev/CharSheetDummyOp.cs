using System;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using RogueEssence.Content;
using RogueEssence.Dev;

namespace RogueEssence.Dev
{
    [Serializable]
    public class CharSheetDummyOp : CharSheetOp
    {
        private string name;
        public override string Name { get { return name; } }
        public CharSheetDummyOp(string name)
        {
            this.name = name;
        }
        public override void Apply(CharSheet sheet) { }
    }

}
