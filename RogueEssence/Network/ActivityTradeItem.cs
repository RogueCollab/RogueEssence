using System.Collections.Generic;
using LiteNetLib;
using RogueEssence.Dungeon;

namespace RogueEssence.Network
{
    public class ActivityTradeItem : OnlineActivity
    {

        public override ActivityType Activity { get { return ActivityType.TradeItem; } }
        public override ActivityType CompatibleActivity { get { return ActivityType.TradeItem; } }

        public List<InvItem> OfferedItems { get; private set; }
        public ExchangeState CurrentState { get; private set; }
        public ActivityTradeItem(ServerInfo server, ContactInfo selfInfo, ContactInfo targetInfo)
            : base(server, selfInfo, targetInfo)
        {
            netPacketProcessor.SubscribeNetSerializable<ExchangeItemState>((state) => OfferedItems = state.State);
            netPacketProcessor.SubscribeNetSerializable<ExchangeReadyState>((state) => CurrentState = state.State);
            CurrentState = ExchangeState.Selecting;
        }

        public override void NetworkReceived(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader);
        }

        public void OfferItems(List<InvItem> offer)
        {
            netPacketProcessor.SendNetSerializable(partner, new ExchangeItemState { State = offer }, DeliveryMethod.ReliableOrdered);
        }

        public void SetReady(ExchangeState state)
        {
            netPacketProcessor.SendNetSerializable(partner, new ExchangeReadyState { State = state }, DeliveryMethod.ReliableOrdered);
        }

        private class ExchangeItemState : WrapperPacket<List<InvItem>> { }

        private class ExchangeReadyState : WrapperPacket<ExchangeState> { }
    }

}
