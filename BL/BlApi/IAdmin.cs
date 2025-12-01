namespace BlApi
{
    /// <summary>
    /// Logical service contract for system administration operations.
    /// </summary>
    public interface IAdmin
    {
        /// <summary>
        /// Resets the entire database:
        /// - Resets all configuration values to their initial defaults.
        /// - Clears all data lists of all entities.
        /// </summary>
        void ResetDB();

        /// <summary>
        /// Initializes the database with initial data:
        /// - First resets the database.
        /// - Then fills all entities with initial values according to the project requirements.
        /// </summary>
        void InitializeDB();

        /// <summary>
        /// Retrieves the current logical system clock.
        /// </summary>
        /// <returns>The current system time as a <see cref="System.DateTime"/>.</returns>
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
        /// and then call the appropriate helper method, e.g.:
        /// AdminManager.UpdateClock(AdminManager.Now.AddMinutes(1));
        /// </remarks>
        void ForwardClock(BO.TimeUnit unit);

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
        /// Only configuration values that are exposed to the PL should be updated.
        /// </summary>
        /// <param name="config">
        /// A <see cref="BO.Config"/> object containing the configuration values
        /// to be applied.
        /// </param>
        void SetConfig(BO.Config config);
    }
}
