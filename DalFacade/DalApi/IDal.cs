namespace DalApi;

//==================== Main DAL Interface ===================\\

/// <summary>
/// Main interface for the Data Access Layer.
/// Provides access points to all entity-specific operations and system configuration.
/// </summary>
public interface IDal
{
    //==================== Entity Accessors ===================\\

    #region Entities

    /// <summary>
    /// Gets the accessor for Courier operations.
    /// </summary>
    ICourier Courier { get; }

    /// <summary>
    /// Gets the accessor for Order operations.
    /// </summary>
    IOrder Order { get; }

    /// <summary>
    /// Gets the accessor for Delivery operations.
    /// </summary>
    IDelivery Delivery { get; }

    #endregion Entities

    //==================== System & Config ===================\\

    #region SystemAndConfig

    /// <summary>
    /// Gets the accessor for system configuration and settings.
    /// </summary>
    IConfig Config { get; }

    /// <summary>
    /// Resets the entire database to its initial state (clears data and resets IDs).
    /// </summary>
    void ResetDB();

    #endregion SystemAndConfig

}