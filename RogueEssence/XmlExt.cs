using System.Xml;

namespace RogueEssence
{
    public static class XmlExt
    {
        public static XmlNode AppendInnerTextChild(this XmlNode parentNode, XmlDocument doc, string name, string text)
        {
            XmlNode node = doc.CreateElement(name);
            node.InnerText = text;
            parentNode.AppendChild(node);
            return node;
        }
    }
}
