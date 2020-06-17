using System;
using System.Collections.Generic;
using System.IO;
using RogueElements;

namespace RogueEssence.Content
{
    public class TileIndexNode
    {
        public Dictionary<Loc, long> Positions;

        public TileIndexNode()
        {
            Positions = new Dictionary<Loc, long>();
        }

        public static TileIndexNode Load(BinaryReader reader)
        {
            TileIndexNode node = new TileIndexNode();
            int count = reader.ReadInt32();
            for (int ii = 0; ii < count; ii++)
            {
                Loc id = new Loc(reader.ReadInt32(), reader.ReadInt32());
                long position = reader.ReadInt64();
                node.Positions[id] = position;
            }
            return node;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Positions.Count);
            foreach (Loc key in Positions.Keys)
            {
                writer.Write(key.X);
                writer.Write(key.Y);
                writer.Write(Positions[key]);
            }
        }


        public long GetPosition(Loc tex)
        {
            long val;
            if (Positions.TryGetValue(tex, out val))
                return val;
            else
                return 0;
        }

        public Loc GetTileDims()
        {
            Loc loc = new Loc();
            foreach (Loc key in Positions.Keys)
            {
                loc.X = Math.Max(loc.X, key.X);
                loc.Y = Math.Max(loc.Y, key.Y);
            }
            return loc + new Loc(1);
        }
    }

    public class TileGuide
    {
        public Dictionary<string, TileIndexNode> Nodes;

        public TileGuide()
        {
            Nodes = new Dictionary<string, TileIndexNode>();
        }

        public long GetPosition(string sheet, Loc tex)
        {
            TileIndexNode node;
            if (Nodes.TryGetValue(sheet, out node))
                return node.GetPosition(tex);
            else
                return 0;

        }

        public Loc GetTileDims(string sheetNum)
        {
            TileIndexNode node;
            if (Nodes.TryGetValue(sheetNum, out node))
                return node.GetTileDims();
            else
                return new Loc();
        }


        public static TileGuide Load(BinaryReader reader)
        {
            TileGuide fullGuide = new TileGuide();
            int count = reader.ReadInt32();
            for (int ii = 0; ii < count; ii++)
            {
                string id = reader.ReadString();
                TileIndexNode node = TileIndexNode.Load(reader);
                fullGuide.Nodes[id] = node;
            }
            return fullGuide;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Nodes.Count);
            foreach (string key in Nodes.Keys)
            {
                writer.Write(key);
                Nodes[key].Save(writer);
            }
        }
    }

}
