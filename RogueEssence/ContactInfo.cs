using System;
using System.Text;
using RogueEssence.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RogueEssence
{

    [Serializable]
    public class ContactData
    {
        public string TeamName;
        public int Rank;
        public int RankStars;
        public ProfilePic[] TeamProfile;


        public ContactData()
        {
            TeamName = "";
            Rank = -1;
            TeamProfile = new ProfilePic[0];
        }

        public string GetLocalRankStr()
        {
            if (Rank == -1)
                return "---";
            return /*Rank.ToLocal() + */new string('\uE10C', RankStars);
        }
    }

    [Serializable]
    public class ContactInfo
    {
        public string UUID;
        public string LastContact;

        public ContactData Data;

        public ContactInfo()
        {
            UUID = "";
            LastContact = "";
            Data = new ContactData();
        }
        public ContactInfo(string uuid)
        {
            UUID = uuid;
            LastContact = "---";
            Data = new ContactData();
        }

        public byte[] SerializeData()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.SerializeData(stream, Data);
                return stream.ToArray();
            }
        }
        public void DeserializeData(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                StringBuilder builder = new StringBuilder();
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                IFormatter formatter = new BinaryFormatter();
                Data = (ContactData)Serializer.DeserializeData(stream);
            }
        }
    }


    [Serializable]
    public class PeerInfo : ContactInfo
    {
        public string IP;
        public int Port;

        public PeerInfo() : base()
        {
            IP = "";
        }
        public PeerInfo(string ip, int port)
            : base("")
        {
            IP = ip;
            Port = port;
        }
    }

    [Serializable]
    public class ServerInfo
    {
        public string ServerName;
        public string IP;
        public int Port;

        public ServerInfo()
        {
            ServerName = "";
            IP = "";
        }
        public ServerInfo(string serverName, string ip, int port)
        {
            ServerName = serverName;
            IP = ip;
            Port = port;
        }
    }
}
