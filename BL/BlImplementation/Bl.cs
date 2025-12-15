namespace BlImplementation;
using BlApi;

//==================== Main BL Implementation ===================\\

/// <summary>
/// Main implementation of the Business Logic Layer (BL).
/// Acts as a centralized container/gateway that aggregates all specific BL service interfaces
/// (Admin, Courier, Order) into a single access point.
/// </summary>
internal class Bl : IBl
{
    //==================== Service Instances ===================\\

    #region Services

    /// <summary>
    /// Provides access to Admin-related operations (Clock, Config, DB).
    /// </summary>
    public IAdmin Admin { get; } = new AdminImplementation();

    /// <summary>
    /// Provides access to Courier-related operations (CRUD, Auth, Lists).
    /// </summary>
    public ICourier Courier { get; } = new CourierImplementation();

    /// <summary>
    /// Provides access to Order-related operations (CRUD, Actions, Lists).
    /// </summary>
    public IOrder Order { get; } = new OrderImplementation();

    #endregion Services

}