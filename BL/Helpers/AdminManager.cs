namespace Helpers;

using System.Runtime.CompilerServices;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager //stage 4
{

    #region Stage 4-7

    //======== DAL & Sync Objects ========\\

    #region Data Access Layer Instance

    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; //stage 4

    internal static event Action? ConfigUpdatedObservers; //stage 5 - for config update observers
    internal static event Action? ClockUpdatedObservers; //stage 5 - for clock update observers

    private static Task? _periodicTask = null; //stage 7

    #endregion Data Access Layer Instance

    //======== Clock =========\\

    #region Clock TimeUnit Enum

    /// <summary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now { get => s_dal.Config.Clock; } //stage 4

    /// <summary>
    /// Method to update application's clock from any BL class as may be required
    /// </summary>
    /// <param name="newClock">updated clock value</param>
    internal static void UpdateClock(DateTime newClock) //stage 4-7
    {
        var oldClock = s_dal.Config.Clock; //stage 4
        s_dal.Config.Clock = newClock; //stage 4

        //Add calls here to any logic method that should be called periodically,
        //after each clock update
        //for example, Periodic students' updates:
        // - Go through all students to update properties that are affected by the clock update
        // - (students become not active after 5 years etc.)

        //TO_DO: //stage 4
        CourierManager.PeriodicCouriersUpdates(oldClock, newClock); //stage 4. to be removed in stage 7 and replaced as below
        OrderManager.PeriodicOrdersUpdates(oldClock, newClock); //stage 5
        //...

        //TO_DO: //stage 7
        //if (_periodicTask is null || _periodicTask.IsCompleted) //stage 7
        //    _periodicTask = Task.Run(() => StudentManager.PeriodicStudentsUpdates(oldClock, newClock));
        //...

        //Calling all the observers of clock update
        ClockUpdatedObservers?.Invoke(); //prepared for stage 5
    }

    internal static void ForwardClock(BO.TimeUnit unit) //stage 4
    {

        if (!Enum.IsDefined(typeof(BO.TimeUnit), unit))
            throw new BO.BlInvalidIntegerException("Invalid time unit.");

        switch (unit) //stage 4
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
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static BO.Config GetConfig() //stage 4
    => new BO.Config()
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

    /// <summary>
    /// Method for setting current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static void SetConfig(BO.Config configuration) //stage 4
    {

        Tools.ValidateConfig(configuration);

        bool configChanged = false; // stage 5

        if (configuration.CompanyAddress != s_dal.Config.CompanyAddress)
        {
            if (!string.IsNullOrWhiteSpace(configuration.CompanyAddress))
            {
                var coords = Tools.GetLocationFromAddress(configuration.CompanyAddress);

                if (coords == null)
                    throw new BO.BlInvalidStringException($"Company address '{configuration.CompanyAddress}' is invalid.");

                s_dal.Config.Latitude = coords.Value.Lat;
                s_dal.Config.Longitude = coords.Value.Lon;
            }
            else
            {
                s_dal.Config.Latitude = null;
                s_dal.Config.Longitude = null;
            }
            configChanged = true;
        } 

        if (s_dal.Config.Clock != configuration.Clock) //stage 4
        {
            s_dal.Config.Clock = configuration.Clock;
            configChanged = true;
        }

        if (s_dal.Config.AdminId != configuration.AdminId) //stage 4
        {
            s_dal.Config.AdminId = configuration.AdminId;
            configChanged = true;
        }

        if (s_dal.Config.AdminPassword != configuration.AdminPassword) //stage 4
        {
            s_dal.Config.AdminPassword = configuration.AdminPassword;
            configChanged = true;
        }

        if (s_dal.Config.CompanyAddress != configuration.CompanyAddress) //stage 4
        {
            s_dal.Config.CompanyAddress = configuration.CompanyAddress;
            configChanged = true;
        }

        if (s_dal.Config.MaxAirDistance != configuration.MaxAirDistance) //stage 4
        {
            s_dal.Config.MaxAirDistance = configuration.MaxAirDistance;
            configChanged = true;
        }

        if (s_dal.Config.AvgCarSpeed != configuration.AvgCarSpeed) //stage 4
        {
            s_dal.Config.AvgCarSpeed = configuration.AvgCarSpeed;
            configChanged = true;
        }

        if (s_dal.Config.AvgMotorcycleSpeed != configuration.AvgMotorcycleSpeed) //stage 4
        {
            s_dal.Config.AvgMotorcycleSpeed = configuration.AvgMotorcycleSpeed;
            configChanged = true;
        }

        if (s_dal.Config.AvgBicycleSpeed != configuration.AvgBicycleSpeed) //stage 4
        {
            s_dal.Config.AvgBicycleSpeed = configuration.AvgBicycleSpeed;
            configChanged = true;
        }

        if (s_dal.Config.AvgWalkSpeed != configuration.AvgWalkSpeed) //stage 4
        {
            s_dal.Config.AvgWalkSpeed = configuration.AvgWalkSpeed;
            configChanged = true;
        }

        if (s_dal.Config.MaxDelTimeRnge != configuration.MaxDelTimeRnge) //stage 4
        {
            s_dal.Config.MaxDelTimeRnge = configuration.MaxDelTimeRnge;
            configChanged = true;
        }

        if (s_dal.Config.RiskTimeRnge != configuration.RiskTimeRnge) //stage 4
        {
            s_dal.Config.RiskTimeRnge = configuration.RiskTimeRnge;
            configChanged = true;
        }

        if (s_dal.Config.UnactiveTimeRnge != configuration.UnactiveTimeRnge) //stage 4
        {
            s_dal.Config.UnactiveTimeRnge = configuration.UnactiveTimeRnge;
            configChanged = true;
        }

        //Calling all the observers of configuration update
        if (configChanged) // stage 5
            ConfigUpdatedObservers?.Invoke(); // stage 5
    }

    #endregion Configuration Variables

    //======== Database Initialization / Reset =========\\

    #region Database Initialization / Reset

    internal static void ResetDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            s_dal.ResetDB(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now); //stage 5 - needed since we want the label on Pl to be updated
            ConfigUpdatedObservers?.Invoke(); //stage 5 - needed to update PL 
        }
    }

    internal static void InitializeDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            DalTest.Initialization.Do(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now);  //stage 5 - needed since we want the label on Pl to be updated           
            ConfigUpdatedObservers?.Invoke(); //stage 5 - needed for update the PL
        }
    }

    #endregion Database Initialization / Reset

    #endregion Stage 4-7

    #region Stage 7 base

    /// <summary>    
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object BlMutex = new(); // BlMutex = s_dal; // This field is actually the same as s_dal - it is defined for readability of locks

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
    /// 
    private static volatile bool s_stop = false;

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread is not null)
            throw new BO.BlTemporaryNotAvailableException("Cannot perform the operation since Simulator is running");
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
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

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Stop()
    {
        if (s_thread is not null)
        {
            s_stop = true;
            s_thread.Interrupt(); //awake a sleeping thread
            s_thread.Name = "ClockRunner stopped";
            s_thread = null;
        }
    }

    private static Task? _simulateTask = null;

    private static void clockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            //TO_DO: //stage 7
            //Add calls here to any logic simulation that was required in stage 7
            //for example: course registration simulation


            //if (_simulateTask is null || _simulateTask.IsCompleted)//stage 7
            //    _simulateTask = Task.Run(() => StudentManager.SimulateCourseRegistrationAndGrade());


            //etc...

            try
            {
                Thread.Sleep(1000); // 1 second
            }
            catch (ThreadInterruptedException) { }
        }
    }

    #endregion Stage 7 base

}



