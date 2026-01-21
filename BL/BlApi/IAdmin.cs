namespace BlApi;
using System;

//==================== IAdmin Service Contract ===================\\

/// <summary>
/// Logical service contract for system administration operations.
/// Includes System Clock management, Configuration settings, and Database maintenance.
/// </summary>
public interface IAdmin
{

    //==================== Observers ===================\\

    #region Observers

    /// <summary>
    /// Registers an observer for configuration changes.
    /// </summary>
    /// <param name="configObserver"> The action to invoke on configuration changes.</param>
    void AddConfigObserver(Action configObserver);

    /// <summary>
    /// Unregisters an observer for configuration changes.
    /// </summary>
    /// <param name="configObserver"> The action to remove from configuration change notifications.</param>
    void RemoveConfigObserver(Action configObserver);

    /// <summary>
    /// Registers an observer for system clock changes.
    /// </summary>
    /// <param name="clockObserver"> The action to invoke on clock changes.</param>
    void AddClockObserver(Action clockObserver);

    /// <summary>
    /// Unregisters an observer for system clock changes.
    /// </summary>
    /// <param name="clockObserver"> The action to remove from clock change notifications.</param>
    void RemoveClockObserver(Action clockObserver);

    #endregion Observers

    //==================== System Clock ===================\\

    #region SystemClock

    /// <summary>
    /// Retrieves the current logical system clock.
    /// </summary>
    /// <returns>The current system time as a <see cref="DateTime"/>.</returns>
    DateTime GetClock();

    /// <summary>
    /// Advances the system clock by the specified time unit.
    /// </summary>
    /// <param name="unit"> The time unit by which to advance the clock.</param>
    void ForwardClock(BO.TimeUnit unit);

    #endregion SystemClock

    //==================== Configuration ===================\\

    #region Configuration

    /// <summary>
    /// Retrieves the current configuration settings.
    /// </summary>
    /// <returns> A <see cref="BO.Config"/> object representing the current configuration settings.</returns>
    BO.Config GetConfig();

    /// <summary>
    /// Sets new configuration settings.
    /// </summary>
    /// <param name="config"> The new configuration settings to apply.</param>
    /// <returns> A task representing the asynchronous operation.</returns>
    Task SetConfig(BO.Config config);

    #endregion Configuration

    //==================== Database Maintenance ===================\\

    #region DatabaseMaintenance

    /// <summary>
    /// Resets the entire database:
    /// - Resets all configuration values to their initial defaults.
    /// - Clears all data lists of all entities (Orders, Couriers, Deliveries).
    /// </summary>
    void ResetDB();

    /// <summary>
    /// Initializes the database with seed data:
    /// - First resets the database.
    /// - Then fills entities with initial sample values for testing/demo purposes.
    /// </summary>
    void InitializeDB();

    #endregion DatabaseMaintenance

    //==================== Simulator Control ===================\\

    #region SimulatorControl

    /// <summary>
    /// Starts the system simulator with the specified time interval.
    /// </summary>
    /// <param name="interval"> The time interval in milliseconds for simulator updates.</param>
    public void StartSimulator(int interval);

    /// <summary>
    /// Stops the system simulator.
    /// </summary>
    public void StopSimulator();

    #endregion SimulatorControl

}