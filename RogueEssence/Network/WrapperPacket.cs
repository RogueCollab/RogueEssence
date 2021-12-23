using System.Text;
using LiteNetLib.Utils;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using RogueEssence.Data;

namespace RogueEssence.Network
{

    public abstract class WrapperPacket<T> : INetSerializable
    {
        public T State { get; set; }

        void INetSerializable.Deserialize(NetDataReader reader)
        {
            byte[] arr = reader.GetBytesWithLength();
            if (arr.Length == 0)
            {
                State = default(T);
                return;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                StringBuilder builder = new StringBuilder();
                stream.Write(arr, 0, arr.Length);
                stream.Position = 0;
                State = (T)Serializer.Deserialize(stream, typeof(T));
            }
        }

        void INetSerializable.Serialize(NetDataWriter netWriter)
        {
            if (State == null)
            {
                byte[] bytes = new byte[0];
                netWriter.PutBytesWithLength(bytes);
                return;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize(stream, State);
                netWriter.PutBytesWithLength(stream.ToArray());
            }
        }
    }
}
