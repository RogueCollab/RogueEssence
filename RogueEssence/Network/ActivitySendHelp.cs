using LiteNetLib;
using RogueEssence.Data;

namespace RogueEssence.Network
{
    public enum ExchangeRescueState
    {
        None,
        Communicating,
        SOSReady,
        SOSTrading,
        AOKReady,
        AOKTrading,
        Completed
    }
    public class ActivitySendHelp : OnlineActivity
    {

        public override ActivityType Activity { get { return ActivityType.SendHelp; } }
        public override ActivityType CompatibleActivity { get { return ActivityType.GetHelp; } }


        public SOSMail OfferedMail { get; private set; }
        public ExchangeRescueState CurrentState { get; private set; }

        public ActivitySendHelp(ServerInfo server, ContactInfo selfInfo, ContactInfo targetInfo)
            : base(server, selfInfo, targetInfo)
        {
            netPacketProcessor.SubscribeNetSerializable<ExchangeSOSState>((state) => OfferedMail = state.State);
            netPacketProcessor.SubscribeNetSerializable<ExchangeRescueReadyState>((state) => CurrentState = state.State);
            CurrentState = ExchangeRescueState.Communicating;
        }

        public override void NetworkReceived(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader);
        }



        public void OfferMail(AOKMail mail)
        {
            netPacketProcessor.SendNetSerializable(partner, new ExchangeAOKState { State = mail }, DeliveryMethod.ReliableOrdered);
        }

        public void SetReady(ExchangeRescueState state)
        {
            netPacketProcessor.SendNetSerializable(partner, new ExchangeRescueReadyState { State = state }, DeliveryMethod.ReliableOrdered);
        }

    }

}
