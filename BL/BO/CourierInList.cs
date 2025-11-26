namespace BO;

/// <summary>
/// Represents a courier in the list with details about their deliveries and status.
/// </summary>
/// <remarks>
/// This class provides information about a courier, including their identification,  full name, active
/// status, Vehicle type, start work date, and delivery statistics.
/// </remarks>
public class CourierInList
{
    public int CourierId { get; init; }
    public string CourierFullName { get; set; }
    public bool CourierIsActive { get; set; }
    public VehicleType VehicleType { get; init; }
    public DateTime? StartWorkDate { get; init; }
    public int DeliveriesInTime { get; init; }//סך המשלוחים הקיימים עבור אותו שליח עם סוג סיום סופק וזמן סיום משלוח קטן/שווה מהזמן המחושב כזמן אספקה מירבי
    public int DeliveriesOverTime { get; init; }//סך המשלוחים הקיימים עבור אותו שליח עם סוג סיום סופק וזמן סיום משלוח גדול מהזמן המחושב כזמן אספקה מירבי
    public int OrderIdInHandle { get; init; }//האם קיימת ברשימת ישויות המשלוח ישות עבור השליח שזמן סיום המשלוח הוא עדיין null
}
