using DO;

namespace BO
{
    /// <summary>
    /// 
    /// </summary>
    internal class DeliveryPerOrderInList
    {
        public int OrderId { get; init; }
        public int? CourierId { get; init; }
        public string CourierFullName { get; init; }
        public ShipmentType ShipmentType { get; init; }
        public DateTime StartDeliveryDate { get; init; }
        public DeliveryFinishType DeliveryFinishType { get; init; }
        public DateTime FinishDeliveryTime { get; init; }


    }
}
