using LiteNetLib;

namespace RogueEssence.Network
{
    public class ActivityTradeMail : OnlineActivity
    {

        public override ActivityType Activity { get { return ActivityType.TradeMail; } }
        public override ActivityType CompatibleActivity { get { return ActivityType.TradeMail; } }

        public ExchangeState CurrentState { get; private set; }
        public ActivityTradeMail(ServerInfo server, ContactInfo selfInfo, ContactInfo targetInfo)
            : base(server, selfInfo, targetInfo)
        {
            CurrentState = ExchangeState.Selecting;
        }

        public override void NetworkReceived(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader);
        }

    }

}
