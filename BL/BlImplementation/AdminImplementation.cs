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

    //==================== Observers Management ===================\\

    #region ObserversManagement

    public void AddClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers += clockObserver;
    public void RemoveClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers -= clockObserver;
    public void AddConfigObserver(Action configObserver) =>
   AdminManager.ConfigUpdatedObservers += configObserver;
    public void RemoveConfigObserver(Action configObserver) =>
    AdminManager.ConfigUpdatedObservers -= configObserver;

    #endregion ObserversManagement

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
        // Only allow clock advancement when the simulator is not running
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
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
    public async Task SetConfig(BO.Config config)
    {
        // Only allow config updates when the simulator is not running
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        await AdminManager.SetConfig(config);
    }

    #endregion Configuration

    //==================== Database Management ===================\\

    #region DatabaseManagement

    /// <summary>
    /// Initializes the database with seed data (for testing or first run).
    /// </summary>
    public void InitializeDB()
    {
        // Only allow DB initialization when the simulator is not running
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        AdminManager.InitializeDB();
    }

    /// <summary>
    /// Resets the database, clearing all operational data (Orders, Deliveries, Couriers).
    /// Keeps Admin credentials intact.
    /// </summary>
    public void ResetDB()
    {
        // Only allow DB reset when the simulator is not running
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        AdminManager.ResetDB();
    }

    #endregion DatabaseManagement

    //==================== Simulator Control ===================\\

    #region SimulatorControl

    public void StartSimulator(int interval)  //stage 7
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        AdminManager.Start(interval); //stage 7
    }

    public void StopSimulator()
    => AdminManager.Stop(); //stage 7

    #endregion SimulatorControl

}