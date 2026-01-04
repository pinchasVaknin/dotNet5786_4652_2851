namespace PL.Courier;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for CourierListWindow.xaml
/// </summary>
public partial class CourierListWindow : Window
{

    //==================== Fields ===================\\

    #region Fields

    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public BO.VehicleType VehicleFilter { get; set; } = BO.VehicleType.All;

    public BO.CourierInList? SelectedCourier { get; set; }

    #endregion Fields

    //================== CourierList Property =================\\

    #region CourierList Property

    /// <summary>
    /// Gets or sets the collection of couriers displayed in the list.
    /// </summary>
    public IEnumerable<BO.CourierInList> CourierList
    {
        get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
        set { SetValue(CourierListProperty, value); }
    }
    public static readonly DependencyProperty CourierListProperty =
        DependencyProperty.Register("CourierList", typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

    #endregion CourierList Property

    //================== Constructor =================\\

    #region Constructor

    public CourierListWindow()
    {
        InitializeComponent();
    }

    #endregion Constructor

    //==================== Methods ===================\\

    #region Methods

    private void CourierFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshCourierList();
    }

    private void RefreshCourierList()
    {
        // TODO: Replace with real user ID after Login implementation
        int adminId = 333333333;
        try
        {
            var oldList = s_bl?.Courier.GetCouriers(adminId);

            if (oldList == null) return;

            if (VehicleFilter == BO.VehicleType.All)
                CourierList = oldList;
            else
                CourierList = oldList.Where(c => c.VehicleType == VehicleFilter);
        }
        catch { }
    }

    private void CourierListObserver()
                    => RefreshCourierList();

    private void Window_Loaded(object sender, RoutedEventArgs e)
                    => s_bl.Courier.AddObserver(CourierListObserver);

    private void Window_Closed(object sender, EventArgs e)
                    => s_bl.Courier.RemoveObserver(CourierListObserver);

    #endregion Methods

    //================== Event Handlers =================\\

    #region Event Handlers

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        new CourierWindow().Show();
    }

    private void CourierDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SelectedCourier != null)
        {
            new CourierWindow(SelectedCourier.CourierId).Show();
        }
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {

        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        // Confirm deletion
        if (sender is Button btn && btn.DataContext is BO.CourierInList courierToDelete)
        {
            // Show confirmation dialog
            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete courier: {courierToDelete.CourierFullName}?",
                "Delete Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            // If user selects No, cancel deletion
            if (result == MessageBoxResult.No) return;

            try
            {
                
                int adminId = 333333333;

                // Perform deletion
                s_bl.Courier.DeleteCourier(adminId, courierToDelete.CourierId);

                // Notify user of successful deletion
                MessageBox.Show("Deleted successfully.");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Deletion failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Restore default cursor
                Mouse.OverrideCursor = null;
            }
        }
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        MainWindow? mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        if (mainWindow == null || !mainWindow.IsLoaded)
        {
            mainWindow = new MainWindow();
        }
        mainWindow.Show();
        Close();
    }

    #endregion Event Handlers

}