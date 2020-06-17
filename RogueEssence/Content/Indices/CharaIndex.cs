using System.Collections.Generic;
using System.IO;

namespace RogueEssence.Content
{

    public class CharaIndexNode
    {
        public long Position;
        public Dictionary<int, CharaIndexNode> Nodes;

        public CharaIndexNode()
        {
            Nodes = new Dictionary<int, CharaIndexNode>();
        }

        //save format:
        //write this element's position
        //for each sub-element
        //write their ID
        //write their element
        
        public static CharaIndexNode Load(BinaryReader reader)
        {
            CharaIndexNode node = new CharaIndexNode();
            node.Position = reader.ReadInt64();
            int count = reader.ReadInt32();
            for(int ii = 0; ii < count; ii++)
            {
                int id = reader.ReadInt32();
                CharaIndexNode subNode = CharaIndexNode.Load(reader);
                node.Nodes[id] = subNode;
            }
            return node;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Position);
            writer.Write(Nodes.Count);
            foreach(int key in Nodes.Keys)
            {
                writer.Write(key);
                Nodes[key].Save(writer);
            }
        }

        public void AddSubValue(long position, params int[] subIDs)
        {
            addSubValue(position, subIDs, 0);
        }
        private void addSubValue(long position, int[] subIDs, int subIDIndex)
        {
            if (subIDIndex < subIDs.Length)
            {
                int nodeIndex = subIDs[subIDIndex];
                if (nodeIndex == -1)
                    Position = position;
                else
                {
                    if (!Nodes.ContainsKey(nodeIndex))
                        Nodes[nodeIndex] = new CharaIndexNode();
                    Nodes[nodeIndex].addSubValue(position, subIDs, subIDIndex + 1);
                }
            }
            else
                Position = position;
        }

        public long GetPosition(params int[] subIDs)
        {
            return getPosition(subIDs, 0);
        }

        private long getPosition(int[] subIDs, int subIDIndex)
        {
            if (subIDIndex < subIDs.Length)
            {
                int nodeIndex = subIDs[subIDIndex];
                if (nodeIndex == -1)
                    return Position;
                else
                {
                    if (!Nodes.ContainsKey(nodeIndex))
                        return 0;
                    return Nodes[nodeIndex].getPosition(subIDs, subIDIndex + 1);
                }
            }
            else
                return Position;
        }
    }
}
