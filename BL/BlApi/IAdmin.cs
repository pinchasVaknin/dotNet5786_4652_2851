namespace BlApi;
using System;

//==================== IAdmin Service Contract ===================\\

/// <summary>
/// Logical service contract for system administration operations.
/// Includes System Clock management, Configuration settings, and Database maintenance.
/// </summary>
public interface IAdmin
{
    //==================== System Clock ===================\\

    #region SystemClock

    /// <summary>
    /// Retrieves the current logical system clock.
    /// </summary>
    /// <returns>The current system time as a <see cref="DateTime"/>.</returns>
    DateTime GetClock();

    /// <summary>
    /// Advances the logical system clock by one unit of time.
    /// </summary>
    /// <param name="unit">
    /// The time unit by which to advance the clock
    /// (minute, hour, day, month, or year).
    /// </param>
    /// <remarks>
    /// The implementation should compute the new time based on the current clock
    /// and trigger any time-dependent periodic updates.
    /// </remarks>
    void ForwardClock(BO.TimeUnit unit);

    #endregion SystemClock

    //==================== Configuration ===================\\

    #region Configuration

    /// <summary>
    /// Retrieves all relevant configuration values for the presentation layer.
    /// </summary>
    /// <returns>
    /// A <see cref="BO.Config"/> object containing configuration values
    /// that are exposed upwards to the PL.
    /// </returns>
    BO.Config GetConfig();

    /// <summary>
    /// Updates configuration values based on the given logical config object.
    /// </summary>
    /// <param name="config">
    /// A <see cref="BO.Config"/> object containing the configuration values
    /// to be applied.
    /// </param>
    void SetConfig(BO.Config config);

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

}