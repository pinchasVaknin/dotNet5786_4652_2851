namespace BlImplementation;

using BlApi;
using Helpers;
using System;

//==================== Admin Implementation ===================\\

/// <summary>
/// Implementation of the IAdmin interface.
/// Acts as a facade/service layer for general system administration tasks
/// including Clock management, Configuration, and Database maintenance.
/// Delegates actual logic to the AdminManager helper.
/// </summary>
internal class AdminImplementation : IAdmin
{
    //==================== System Clock ===================\\

    #region SystemClock

    /// <summary>
    /// Retrieves the current logical time of the system (simulation clock).
    /// </summary>
    /// <returns>The current DateTime of the system.</returns>
    public DateTime GetClock()
    {
        return AdminManager.Now;
    }

    /// <summary>
    /// Advances the system clock by a specified time unit (Minute, Hour, Day, etc.).
    /// This may trigger periodic system updates (like closing timed-out orders).
    /// </summary>
    /// <param name="unit">The unit of time to advance.</param>
    public void ForwardClock(BO.TimeUnit unit)
    {
        AdminManager.ForwardClock(unit);
    }

    #endregion SystemClock

    //==================== Configuration ===================\\

    #region Configuration

    /// <summary>
    /// Retrieves the current system configuration settings.
    /// </summary>
    /// <returns>A <see cref="BO.Config"/> object containing system parameters.</returns>
    public BO.Config GetConfig()
    {
        return AdminManager.GetConfig();
    }

    /// <summary>
    /// Updates the system configuration settings.
    /// </summary>
    /// <param name="config">The new configuration object to apply.</param>
    public void SetConfig(BO.Config config)
    {
        AdminManager.SetConfig(config);
    }

    #endregion Configuration

    //==================== Database Management ===================\\

    #region DatabaseManagement

    /// <summary>
    /// Initializes the database with seed data (for testing or first run).
    /// </summary>
    public void InitializeDB()
    {
        AdminManager.InitializeDB();
    }

    /// <summary>
    /// Resets the database, clearing all operational data (Orders, Deliveries, Couriers).
    /// Keeps Admin credentials intact.
    /// </summary>
    public void ResetDB()
    {
        AdminManager.ResetDB();
    }

    #endregion DatabaseManagement

}