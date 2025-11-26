using DO;

namespace BO
{
    /// <summary>
    /// 
    /// </summary>
    internal class ClosedDeliveryInList
    {
        public int DeliveryId {  get; init; }
        public int OrderId { get; init; }
        public TypeOfOrder TypeOfOrder { get; init; }
        public string OrderAddress { get; init; }
        public ShipmentType ShipmentType { get; init; }
        public double? AcctualDistance { get; init; }
        public DateTime TotalHandleTime { get; init; }
        public DeliveryFinishType? DeliveryFinishType { get; init; }

    }
}
