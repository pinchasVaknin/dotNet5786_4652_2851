namespace PL;

using PL.Courier;
using PL.Helpers;
using PL.Login;
using PL.Order;
using PL.Tools;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    //==================== Fields ===================\\

    #region Fields

    // The entry point to the BL layer (Factory pattern).
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // Stage 7: Mutex fields for thread-safe observer updates
    private readonly ObserverMutex _clockMutex = new();
    private readonly ObserverMutex _configMutex = new();
    private readonly ObserverMutex _dashboardMutex = new();

    #endregion Fields

    //==================== Dependency Properties ===================\\

    #region Dependency Properties

    public DateTime CurrentTime
    {
        get { return (DateTime)GetValue(CurrentTimeProperty); }
        set { SetValue(CurrentTimeProperty, value); }
    }
    // Using a DependencyProperty as the backing store for CurrentTime.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

    public BO.Config Configuration
    {
        get { return (BO.Config)GetValue(ConfigurationProperty); }
        set { SetValue(ConfigurationProperty, value); }
    }
    // Using a DependencyProperty as the backing store for Configuration.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ConfigurationProperty =
        DependencyProperty.Register("Configuration", typeof(BO.Config), typeof(MainWindow));

    public IEnumerable<DashboardItem> DashboardStats
    {
        get { return (IEnumerable<DashboardItem>)GetValue(DashboardStatsProperty); }
        set { SetValue(DashboardStatsProperty, value); }
    }
    // Using a DependencyProperty as the backing store for DashboardStats.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DashboardStatsProperty =
        DependencyProperty.Register("DashboardStats", typeof(IEnumerable<DashboardItem>), typeof(MainWindow));

    // Stage 7: Simulator control properties
    public bool IsSimulatorRunning
    {
        get { return (bool)GetValue(IsSimulatorRunningProperty); }
        set { SetValue(IsSimulatorRunningProperty, value); }
    }
    // Using a DependencyProperty as the backing store for IsSimulatorRunning.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsSimulatorRunningProperty =
        DependencyProperty.Register("IsSimulatorRunning", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

    public int Interval
    {
        get { return (int)GetValue(IntervalProperty); }
        set { SetValue(IntervalProperty, value); }
    }
    // Using a DependencyProperty as the backing store for Interval.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IntervalProperty =
        DependencyProperty.Register("Interval", typeof(int), typeof(MainWindow), new PropertyMetadata(1));

    #endregion Dependency Properties

    //==================== Constructor ===================\\

    #region Constructor

    public MainWindow()
    {
        InitializeComponent();
    }

    #endregion Constructor

    //==================== Button Click Handlers ===================\\

    #region Button Click Handlers

    private void BtnAddMinute_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Minute);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnAddHour_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Hour);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnAddDay_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Day);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnAddYear_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Year);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnDashboardItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is DashboardItem item)
        {
            // Open OrderListWindow with appropriate filters
            var win = new OrderListWindow();

            // Set filters based on the clicked dashboard item
            win.OrderCategoryFilter = (BO.OrderInListFilterBy)item.MainStatus;

            // If the main status is "All", do not set the schedule status filter
            win.ScheduleStatusFilter = item.TimeStatus;

            // Show the window
            win.Show();
        }
    }

    private void BtnInitDB_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Are you sure you want to initialize the Database? This might take a moment.", "Initialize Database", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            try
            {
                // Show wait cursor
                Mouse.OverrideCursor = Cursors.Wait;

                // Initialize the database
                s_bl.Admin.InitializeDB();

                // Refresh properties
                CurrentTime = s_bl.Admin.GetClock();
                Configuration = s_bl.Admin.GetConfig();

                // Show success message
                MessageBox.Show("Database initialized successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                // Restore default cursor
                Mouse.OverrideCursor = null;
            }
        }
    }

    private void BtnResetDB_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Are you sure you want to reset the Database? This will erase all data.", "Reset Database", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            try
            {
                // Show wait cursor
                Mouse.OverrideCursor = Cursors.Wait;

                // Reset the database
                s_bl.Admin.ResetDB();

                // Refresh properties
                CurrentTime = s_bl.Admin.GetClock();
                Configuration = s_bl.Admin.GetConfig();

                // Show success message
                MessageBox.Show("Database reset successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                // Restore default cursor
                Mouse.OverrideCursor = null;
            }
        }
    }

    private async void BtnUpdateConfig_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await s_bl.Admin.SetConfig(Configuration);
            MessageBox.Show("Configuration updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnShowCouriers_Click(object sender, RoutedEventArgs e)
    {
        // Check if CourierListWindow is already open
        CourierListWindow? courierListWindow = App.Current.Windows.OfType<CourierListWindow>().FirstOrDefault();

        // If not open, create and show it; otherwise, activate the existing window
        if (courierListWindow == null)
        {
            courierListWindow = new CourierListWindow();
            courierListWindow.Show();
        }
        else
        {
            // Bring the existing window to the front
            courierListWindow.Activate();
        }
    }

    private void BtnShowOrders_Click(object sender, RoutedEventArgs e)
    {
        // Check if OrderListWindow is already open
        OrderListWindow? orderListWindow = App.Current.Windows.OfType<OrderListWindow>().FirstOrDefault();

        // If not open, create and show it; otherwise, activate the existing window
        if (orderListWindow == null)
        {
            orderListWindow = new OrderListWindow();
            orderListWindow.Show();
        }
        else
        {
            // Bring the existing window to the front
            orderListWindow.Activate();
        }
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        // Stop simulator if running before logout
        if (IsSimulatorRunning)
        {
            s_bl.Admin.StopSimulator();
            IsSimulatorRunning = false;
        }

        new LoginWindow().Show();
        Close();
    }

    /// <summary>
    /// Toggles the simulator on/off based on current state.
    /// Stage 7: Simulator control
    /// </summary>
    private void BtnToggleSimulator_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsSimulatorRunning)
            {
                // Stop the simulator
                s_bl.Admin.StopSimulator();
                IsSimulatorRunning = false;
            }
            else
            {
                // Validate interval
                if (Interval <= 0)
                {
                    MessageBox.Show("Interval must be a positive number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Start the simulator with the specified interval
                s_bl.Admin.StartSimulator(Interval);
                IsSimulatorRunning = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    #endregion Button Click Handlers

    //==================== Dashboard Refresh ===================\\

    #region Dashboard Refresh

    private void RefreshDashboard()
    {
        #region Stage 7 (for multithreading)
        if (_dashboardMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        Dispatcher.BeginInvoke(async () =>
        {
            try
            {
                // Get the summary array from the business logic layer
                int[] summaryArray = s_bl.Order.GetOrderStatusSummary(UserData.s_UserId);

                // Prepare the list to hold dashboard items
                var statsList = new List<DashboardItem>();
                var orderStatuses = Enum.GetValues(typeof(BO.OrderStatus));
                var scheduleStatuses = Enum.GetValues(typeof(BO.ScheduleStatus));
                int scheduleCount = scheduleStatuses.Length;

                // Populate the dashboard items based on the summary array
                foreach (BO.OrderStatus os in orderStatuses)
                {
                    // For each schedule status
                    foreach (BO.ScheduleStatus ss in scheduleStatuses)
                    {
                        // Calculate the index in the summary array
                        int index = (int)os * scheduleCount + (int)ss;
                        int count = (index < summaryArray.Length) ? summaryArray[index] : 0;

                        // Only add items with a count greater than zero
                        if (count > 0)
                        {
                            // Add a new dashboard item
                            statsList.Add(new DashboardItem
                            {
                                MainStatus = os,
                                TimeStatus = ss,
                                Count = count
                            });
                        }
                    }
                }

                // Update the DashboardStats property
                DashboardStats = statsList;
            }
            catch { }
            finally
            {
                // After completing the work, check if a restart was requested
                if (await _dashboardMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    RefreshDashboard();
            }
        });
        #endregion Stage 7 (for multithreading)
    }

    #endregion Dashboard Refresh

    //==================== Observers ===================\\

    #region Observers

    private void ClockObserver()
    {
        #region Stage 7 (for multithreading)
        if (_clockMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        Dispatcher.BeginInvoke(async () =>
        {
            try
            {
                // The actual work to be done on the UI thread
                CurrentTime = s_bl.Admin.GetClock();
            }
            finally
            {
                // After completing the work, check if a restart was requested
                if (await _clockMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    ClockObserver();
            }
        });
        #endregion Stage 7 (for multithreading)
    }

    private void ConfigObserver()
    {
        #region Stage 7 (for multithreading)
        if (_configMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        Dispatcher.BeginInvoke(async () =>
        {
            try
            {
                // The actual work to be done on the UI thread
                Configuration = s_bl.Admin.GetConfig();
            }
            finally
            {
                // After completing the work, check if a restart was requested
                if (await _configMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    ConfigObserver();
            }
        });
        #endregion Stage 7 (for multithreading)
    }

    #endregion Observers

    //==================== Window Events ===================\\

    #region Window Events

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        CurrentTime = s_bl.Admin.GetClock();
        Configuration = s_bl.Admin.GetConfig();
        RefreshDashboard();

        s_bl.Admin.AddClockObserver(ClockObserver);
        s_bl.Admin.AddConfigObserver(ConfigObserver);
        s_bl.Order.AddObserver(RefreshDashboard);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        // Stage 7: Stop the simulator gracefully when window closes
        if (IsSimulatorRunning)
        {
            s_bl.Admin.StopSimulator();
            IsSimulatorRunning = false;
        }

        s_bl.Admin.RemoveClockObserver(ClockObserver);
        s_bl.Admin.RemoveConfigObserver(ConfigObserver);
        s_bl.Order.RemoveObserver(RefreshDashboard);
    }

    #endregion Window Events

}