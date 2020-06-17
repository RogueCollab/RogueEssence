using LiteNetLib;
using RogueEssence.Dungeon;

namespace RogueEssence.Network
{
    public enum ExchangeState
    {
        None,
        Selecting,
        Viewing,
        Ready,
        Exchange,
        PostTradeWait
    }

    public class ActivityTradeTeam : OnlineActivity
    {

        public override ActivityType Activity { get { return ActivityType.TradeTeam; } }
        public override ActivityType CompatibleActivity { get { return ActivityType.TradeTeam; } }

        public CharData OfferedChar { get; private set; }
        public ExchangeState CurrentState { get; private set; }
        public ActivityTradeTeam(ServerInfo server, ContactInfo selfInfo, ContactInfo targetInfo)
            : base(server, selfInfo, targetInfo)
        {
            netPacketProcessor.SubscribeNetSerializable<ExchangeCharState>((state) => OfferedChar = state.State);
            netPacketProcessor.SubscribeNetSerializable<ExchangeReadyState>((state) => CurrentState = state.State);
            CurrentState = ExchangeState.Selecting;
        }

        public override void NetworkReceived(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader);
        }

        public void OfferChar(CharData chara)
        {
            netPacketProcessor.SendNetSerializable(partner, new ExchangeCharState { State = chara }, DeliveryMethod.ReliableOrdered);
        }

        public void SetReady(ExchangeState state)
        {
            netPacketProcessor.SendNetSerializable(partner, new ExchangeReadyState { State = state }, DeliveryMethod.ReliableOrdered);
        }

        private class ExchangeCharState : WrapperPacket<CharData> { }

        private class ExchangeReadyState : WrapperPacket<ExchangeState> { }
    }
}
