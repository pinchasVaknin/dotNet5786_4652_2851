namespace BlApi;

//==================== Main BL Interface ===================\\

/// <summary>
/// Main interface for the Business Logic Layer (BL).
/// Acts as a centralized gateway that aggregates all specific service interfaces
/// (Admin, Courier, Order) into a single access point.
/// </summary>
public interface IBl
{
    //==================== Services ===================\\

    /// <summary>
    /// Provides access to Admin-related operations (Clock, Config, DB).
    /// </summary>
    IAdmin Admin { get; }

    /// <summary>
    /// Provides access to Courier-related operations (CRUD, Auth, Lists).
    /// </summary>
    ICourier Courier { get; }

    /// <summary>
    /// Provides access to Order-related operations (CRUD, Actions, Lists).
    /// </summary>
    IOrder Order { get; }
}