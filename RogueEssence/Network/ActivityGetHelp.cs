using LiteNetLib;
using RogueEssence.Data;

namespace RogueEssence.Network
{
    public class ActivityGetHelp : OnlineActivity
    {

        public override ActivityType Activity { get { return ActivityType.GetHelp; } }
        public override ActivityType CompatibleActivity { get { return ActivityType.SendHelp; } }


        public AOKMail OfferedMail { get; private set; }
        public ExchangeRescueState CurrentState { get; private set; }
        public ActivityGetHelp(ServerInfo server, ContactInfo selfInfo, ContactInfo targetInfo)
            : base(server, selfInfo, targetInfo)
        {
            netPacketProcessor.SubscribeNetSerializable<ExchangeAOKState>((state) => OfferedMail = state.State);
            netPacketProcessor.SubscribeNetSerializable<ExchangeRescueReadyState>((state) => CurrentState = state.State);
            CurrentState = ExchangeRescueState.Communicating;
        }

        public override void NetworkReceived(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader);
        }




        public void OfferMail(SOSMail mail)
        {
            netPacketProcessor.SendNetSerializable(partner, new ExchangeSOSState { State = mail }, DeliveryMethod.ReliableOrdered);
        }

        public void SetReady(ExchangeRescueState state)
        {
            netPacketProcessor.SendNetSerializable(partner, new ExchangeRescueReadyState { State = state }, DeliveryMethod.ReliableOrdered);
        }
    }

    public class ExchangeSOSState : WrapperPacket<SOSMail> { }

    public class ExchangeAOKState : WrapperPacket<AOKMail> { }

    public class ExchangeRescueReadyState : WrapperPacket<ExchangeRescueState> { }

}
