namespace Helpers;

using System.Runtime.CompilerServices;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager //stage4
{

    #region Stage4-7

    //======== DAL & Sync Objects ========\\

    #region Data Access Layer Instance

    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; //stage4

    internal static event Action? ConfigUpdatedObservers; //stage5 - for config update observers
    internal static event Action? ClockUpdatedObservers; //stage5 - for clock update observers

    #endregion Data Access Layer Instance

    //======== Clock =========\\

    #region Clock TimeUnit Enum

    /// <summary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now
    {
        get
        {
            lock (BlMutex)
                return s_dal.Config.Clock; //stage4
        }
    }

    /// <summary>
    /// Method to update application's clock from any BL class as may be required
    /// </summary>
    /// <param name="newClock">updated clock value</param>
    internal static void UpdateClock(DateTime newClock) //stage4-7
    {
        // Validate input
        DateTime oldClock;

        // Update clock under lock
        lock (BlMutex) //stage7
        {
            oldClock = s_dal.Config.Clock; //stage4
            s_dal.Config.Clock = newClock; //stage4
        }

        // stage7: call periodic update asynchronously (do not block the simulator thread)
        _ = Task.Run(() =>
           {
               try
               {
                   CourierManager.PeriodicCouriersUpdates(oldClock, newClock);
                   OrderManager.PeriodicOrdersUpdates(oldClock, newClock);
               }
               catch
               {
                   // Ignore errors during periodic updates when stopping
               }
           });

        // Calling all the observers of clock update
        ClockUpdatedObservers?.Invoke(); //prepared for stage5

        // Notify order and courier list observers that time-dependent fields need recalculation
        OrderManager.Observers.NotifyListUpdated(); //stage7 - recalculate TimeLeftToFinish, ScheduleStatus
        CourierManager.Observers.NotifyListUpdated(); //stage7 - refresh courier list as well
    }

    internal static void ForwardClock(BO.TimeUnit unit) //stage4
    {
        // Validate input
        if (!Enum.IsDefined(typeof(BO.TimeUnit), unit))
            throw new BO.BlInvalidIntegerException("Invalid time unit.");

        // Advance clock based on specified time unit
        switch (unit) //stage4
        {
            case BO.TimeUnit.Minute:
                UpdateClock(Now.AddMinutes(1));
                break;
            case BO.TimeUnit.Hour:
                UpdateClock(Now.AddHours(1));
                break;
            case BO.TimeUnit.Day:
                UpdateClock(Now.AddDays(1));
                break;
            case BO.TimeUnit.Month:
                UpdateClock(Now.AddMonths(1));
                break;
            case BO.TimeUnit.Year:
                UpdateClock(Now.AddYears(1));
                break;
            default:
                throw new BO.BlUnknownTimeUnitException($"Unknown TimeUnit value: {unit}");
        }
    }

    #endregion Clock TimeUnit Enum

    //======== Configuration Variables =========\\

    #region Configuration Variables

    /// <summary>
    /// Method for providing current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage7
    internal static BO.Config GetConfig() //stage4
    {
        lock (BlMutex) //stage7
        {
            return new BO.Config()
            {
                Clock = s_dal.Config.Clock,
                AdminId = s_dal.Config.AdminId,
                AdminPassword = s_dal.Config.AdminPassword,
                CompanyAddress = s_dal.Config.CompanyAddress,
                Latitude = s_dal.Config.Latitude,
                Longitude = s_dal.Config.Longitude,
                MaxAirDistance = s_dal.Config.MaxAirDistance,
                AvgCarSpeed = s_dal.Config.AvgCarSpeed,
                AvgMotorcycleSpeed = s_dal.Config.AvgMotorcycleSpeed,
                AvgBicycleSpeed = s_dal.Config.AvgBicycleSpeed,
                AvgWalkSpeed = s_dal.Config.AvgWalkSpeed,
                MaxDelTimeRnge = s_dal.Config.MaxDelTimeRnge,
                RiskTimeRnge = s_dal.Config.RiskTimeRnge,
                UnactiveTimeRnge = s_dal.Config.UnactiveTimeRnge
            };
        }
    }

    /// <summary>
    /// Method for setting current configuration variables values for any BL class that may need it
    /// </summary>
    internal static async Task SetConfig(BO.Config configuration)
    {
        Tools.ValidateConfig(configuration); // Validate input

        // Read current address safely (DAL access under lock)
        string? oldAddress;
        lock (AdminManager.BlMutex)
            oldAddress = s_dal.Config.CompanyAddress;

        // Check if address changed
        bool addressChanged = configuration.CompanyAddress != oldAddress;

        // Network call outside lock
        (double Lat, double Lon)? newCoords = null;
        bool clearCoords = false;

        // If address changed, get new coordinates
        if (addressChanged)
        {
            // If new address is non-empty, get coordinates
            if (!string.IsNullOrWhiteSpace(configuration.CompanyAddress))
            {
                // Get coordinates from address
                var coords = await Tools.GetLocationFromAddressAsync(configuration.CompanyAddress);

                // If address is invalid, throw exception
                if (coords == null)
                    throw new BO.BlInvalidStringException(
                     $"Company address '{configuration.CompanyAddress}' is invalid.");

                // Set new coordinates
                newCoords = (coords.Value.Lat, coords.Value.Lon);
            }
            else
            {
                clearCoords = true; // Empty address -> clear coordinates
            }
        }

        bool configChanged = false;

        // Update DAL under lock (no await inside lock)
        lock (AdminManager.BlMutex)
        {
            if (addressChanged)
            {
                // Update address
                s_dal.Config.CompanyAddress = configuration.CompanyAddress;

                // Update coordinates
                if (clearCoords)
                {
                    s_dal.Config.Latitude = null;
                    s_dal.Config.Longitude = null;
                }
                else
                {
                    s_dal.Config.Latitude = newCoords!.Value.Lat;
                    s_dal.Config.Longitude = newCoords.Value.Lon;
                }

                configChanged = true;
            }

            // Update other configuration variables
            if (s_dal.Config.Clock != configuration.Clock)
            {
                s_dal.Config.Clock = configuration.Clock;
                configChanged = true;
            }

            // Update admin credentials
            if (s_dal.Config.AdminId != configuration.AdminId)
            {
                s_dal.Config.AdminId = configuration.AdminId;
                configChanged = true;
            }

            // Update admin password
            if (s_dal.Config.AdminPassword != configuration.AdminPassword)
            {
                s_dal.Config.AdminPassword = configuration.AdminPassword;
                configChanged = true;
            }

            // Update max air distance
            if (s_dal.Config.MaxAirDistance != configuration.MaxAirDistance)
            {
                s_dal.Config.MaxAirDistance = configuration.MaxAirDistance;
                configChanged = true;
            }

            // Update average speeds
            if (s_dal.Config.AvgCarSpeed != configuration.AvgCarSpeed)
            {
                s_dal.Config.AvgCarSpeed = configuration.AvgCarSpeed;
                configChanged = true;
            }

            // Update average motorcycle speed
            if (s_dal.Config.AvgMotorcycleSpeed != configuration.AvgMotorcycleSpeed)
            {
                s_dal.Config.AvgMotorcycleSpeed = configuration.AvgMotorcycleSpeed;
                configChanged = true;
            }

            // Update average bicycle speed
            if (s_dal.Config.AvgBicycleSpeed != configuration.AvgBicycleSpeed)
            {
                s_dal.Config.AvgBicycleSpeed = configuration.AvgBicycleSpeed;
                configChanged = true;
            }

            // Update average walk speed
            if (s_dal.Config.AvgWalkSpeed != configuration.AvgWalkSpeed)
            {
                s_dal.Config.AvgWalkSpeed = configuration.AvgWalkSpeed;
                configChanged = true;
            }

            // Update time ranges
            if (s_dal.Config.MaxDelTimeRnge != configuration.MaxDelTimeRnge)
            {
                s_dal.Config.MaxDelTimeRnge = configuration.MaxDelTimeRnge;
                configChanged = true;
            }

            // Update risk time range
            if (s_dal.Config.RiskTimeRnge != configuration.RiskTimeRnge)
            {
                s_dal.Config.RiskTimeRnge = configuration.RiskTimeRnge;
                configChanged = true;
            }

            // Update unactive time range
            if (s_dal.Config.UnactiveTimeRnge != configuration.UnactiveTimeRnge)
            {
                s_dal.Config.UnactiveTimeRnge = configuration.UnactiveTimeRnge;
                configChanged = true;
            }
        }

        // Notify observers outside lock
        if (configChanged)
            ConfigUpdatedObservers?.Invoke();
    }

    #endregion Configuration Variables

    //======== Database Initialization / Reset =========\\

    #region Database Initialization / Reset

    internal static void ResetDB() //stage4-7
    {
        lock (BlMutex) //stage7
        {
            s_dal.ResetDB(); //stage4
        }
        AdminManager.UpdateClock(AdminManager.Now); //stage5 - needed since we want the label on Pl to be updated
        ConfigUpdatedObservers?.Invoke(); //stage5 - needed to update PL 
    }

    internal static void InitializeDB() //stage4-7
    {
        lock (BlMutex) //stage7
        {
            DalTest.Initialization.Do(); //stage4
        }
        AdminManager.UpdateClock(AdminManager.Now); //stage5 - needed since we want the label on Pl to be updated 
        ConfigUpdatedObservers?.Invoke(); //stage5 - needed for update the PL
    }

    #endregion Database Initialization / Reset

    #endregion Stage4-7

    #region Stage7 base

    /// <summary> 
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object BlMutex = new();

    /// <summary>
    /// The thread of the simulator
    /// </summary>
    private static volatile Thread? s_thread;

    /// <summary>
    /// The Interval for clock updating
    /// in minutes by second (default value is 1, will be set on Start()) 
    /// </summary>
    private static int s_interval = 1;

    /// <summary>
    /// The flag that signs whether simulator is running
    /// </summary>
    private static volatile bool s_stop = false;

    [MethodImpl(MethodImplOptions.Synchronized)] //stage7 
    public static void ThrowOnSimulatorIsRunning()
    {
        // Throw exception if simulator is running
        if (s_thread is not null)
            throw new BO.BlTemporaryNotAvailableException("Cannot perform the operation since Simulator is running");
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage7 
    internal static void Start(int interval)
    {
        if (s_thread is null)
        {
            s_interval = interval;
            s_stop = false;
            s_thread = new(clockRunner) { Name = "ClockRunner" };
            s_thread.Start();
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage7 
    internal static void Stop()
    {
        if (s_thread is not null)
        {
            s_stop = true;
            s_thread.Interrupt(); // Awake a sleeping thread
            s_thread = null;
        }
    }

    private static void clockRunner()
    {
        while (!s_stop)
        {
            try
            {
                Thread.Sleep(1000);
            }
            catch (ThreadInterruptedException)
            {
                // Thread was interrupted by Stop() - exit gracefully
                break;
            }

            // Check again after sleep (in case Stop was called)
            if (s_stop)
                break;

            try
            {
                if (s_interval != 0)
                    UpdateClock(Now.AddMinutes(s_interval));

                // Simulate courier activity asynchronously (do not block the simulator thread)
                _ = Task.Run(async () =>
                  {
                      try
                      {
                          await CourierManager.SimulateCourierActivityAsync();
                      }
                      catch
                      {
                          // Ignore errors when simulator is stopping
                      }
                  });
            }
            catch
            {
                // Ignore any errors during clock update when stopping
                if (s_stop)
                    break;
            }
        }
    }

    #endregion Stage7 base

}

