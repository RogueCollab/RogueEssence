using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class BackReference<T>
    {
        public T Element;
        public int BackRef;

        public BackReference() { BackRef = -1; }
        public BackReference(T element)
        {
            Element = element;
            BackRef = -1;
        }
        public BackReference(T element, int backRef)
        {
            Element = element;
            BackRef = backRef;
        }
    }

}
