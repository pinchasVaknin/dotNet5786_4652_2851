namespace PL.Courier;

using BO;
using PL.Tools;
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

    public BO.CourierInListFilterBy CourierCategoryFilter { get; set; } = BO.CourierInListFilterBy.All;

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
        // Update the selected filter category based on user selection
        if (CmbFilterCategory.SelectedItem is BO.CourierInListFilterBy selectedCategory)
        {
            CourierCategoryFilter = selectedCategory;
        }

        // Reset the filter value ComboBox
        if (CmbFilterValue != null)
        {
            CmbFilterValue.ItemsSource = null;
            CmbFilterValue.SelectedItem = null;
        }

        //  Refresh the order list based on the new filter category
        RefreshCourierList();
    }

    private void CmbFilterValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshCourierList();
    }

    private void RefreshCourierList()
    {
        try
        {
            // Retrieve and filter couriers based on the selected filter criteria
            if (CourierCategoryFilter == BO.CourierInListFilterBy.All)
            {
                // No filter applied, get all couriers
                CourierList = s_bl?.Courier.GetCouriers(UserData.s_UserId) ?? Enumerable.Empty<BO.CourierInList>();

                if (CmbFilterValue != null)
                {
                    CmbFilterValue.ItemsSource = null;
                    CmbFilterValue.IsEnabled = false;
                }
            }
            else
            {
                if (CmbFilterValue == null) return;

                CmbFilterValue.IsEnabled = true;

                if (CmbFilterValue.ItemsSource == null)
                {
                    switch (CourierCategoryFilter)
                    {
                        case BO.CourierInListFilterBy.CourierIsActive:
                            CmbFilterValue.ItemsSource = new List<object> { true, false };
                            break;

                        case BO.CourierInListFilterBy.VehicleType:
                            CmbFilterValue.ItemsSource = Enum.GetValues(typeof(BO.VehicleType));
                            break;

                        case BO.CourierInListFilterBy.OrderIdInHandle:
                            CmbFilterValue.ItemsSource = new List<object> { true, false };
                            break;
                    }
                }

                if (CmbFilterValue.SelectedItem == null) return;

                switch (CourierCategoryFilter)
                {
                    case BO.CourierInListFilterBy.CourierIsActive:
                        var status = (bool)CmbFilterValue.SelectedItem;
                        CourierList = s_bl?.Courier.GetCouriers(UserData.s_UserId, CourierCategoryFilter, status);
                        break;
                    case BO.CourierInListFilterBy.VehicleType:
                        var type = (BO.VehicleType)CmbFilterValue.SelectedItem;
                        CourierList = s_bl?.Courier.GetCouriers(UserData.s_UserId, CourierCategoryFilter, type);
                        break;
                    case BO.CourierInListFilterBy.OrderIdInHandle:
                        var scheduleStatus = (bool)CmbFilterValue.SelectedItem;
                        CourierList = s_bl?.Courier.GetCouriers(UserData.s_UserId, CourierCategoryFilter, scheduleStatus);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion Methods

    //==================== Observers ===================\\

    #region Observers

    private void CourierListObserver()
                    => RefreshCourierList();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        s_bl.Courier.AddObserver(CourierListObserver);
        RefreshCourierList();
    }

    private void Window_Closed(object sender, EventArgs e)
                    => s_bl.Courier.RemoveObserver(CourierListObserver);

    #endregion Observers

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
            if (result == MessageBoxResult.No)
            {
                // Restore default cursor
                Mouse.OverrideCursor = null;
                return;
            }

            try
            {
                // Perform deletion
                s_bl.Courier.DeleteCourier(UserData.s_UserId, courierToDelete.CourierId);

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

    #endregion Event Handlers

}