using LiteNetLib;
using LiteNetLib.Utils;

namespace RogueEssence.Network
{
    public enum ActivityType
    {
        None,
        TradeTeam,
        TradeItem,
        TradeMail,
        SendHelp,
        GetHelp
    }
    public abstract class OnlineActivity
    {
        public ServerInfo Server { get; private set; }
        public ContactInfo SelfInfo { get; private set; }
        public ContactInfo TargetInfo { get; private set; }

        public abstract ActivityType Activity { get; }
        public abstract ActivityType CompatibleActivity { get; }

        protected readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        protected NetPeer partner;

        public OnlineActivity(ServerInfo server, ContactInfo selfInfo, ContactInfo targetInfo)
        {
            Server = server;
            SelfInfo = selfInfo;
            TargetInfo = targetInfo;
        }

        public void SetPeer(NetPeer peer)
        {
            partner = peer;
        }

        public abstract void NetworkReceived(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod);
    }
}
