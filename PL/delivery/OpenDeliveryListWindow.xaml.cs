using PL.Courier;
using PL.Helpers;
using PL.Order;
using PL.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL.delivery;

/// <summary>
/// Interaction logic for OpenDeliveryListWindow.xaml
/// </summary>
public partial class OpenDeliveryListWindow : Window
{
    //==================== Fields ===================\\

    #region Fields

    // The entry point to the BL layer (Factory pattern).
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // Selected order from the data grid
    public BO.OpenOrderInList? SelectedOrder { get; set; } = null;

    // Stage 7: Mutex field for thread-safe observer updates
    private readonly ObserverMutex _orderListMutex = new();

    #endregion Fields

    //================ OrderList Property =================\\

    #region OrderList Property

    public IEnumerable<BO.OpenOrderInList> OrderList
    {
        get { return (IEnumerable<BO.OpenOrderInList>)GetValue(OrderListProperty); }
        set { SetValue(OrderListProperty, value); }
    }
    // Using a DependencyProperty as the backing store for OrderList.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register("OrderList", typeof(IEnumerable<BO.OpenOrderInList>), typeof(OpenDeliveryListWindow), new PropertyMetadata(null));

    public BO.Courier CurrentCourier
    {
        get { return (BO.Courier)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }
    // Using a DependencyProperty as the backing store for CurrentCourier.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(OpenDeliveryListWindow));

    #endregion OrderList Property

    //================= Constructor =================\\

    #region Constructor

    public OpenDeliveryListWindow(BO.Courier courier)
    {
        CurrentCourier = courier;
        InitializeComponent();
    }

    #endregion Constructor

    //==================== Methods ===================\\

    #region Methods

    private async Task RefreshOrderListAsync()
    {
        try
        {
            // Show wait cursor
            Mouse.OverrideCursor = Cursors.Wait;

            // Fetch the open orders from the BL layer
            OrderList = await s_bl.Order.GetOpenOrdersForCourier(
                UserData.s_UserId,
                CurrentCourier.CourierId,
                null,
                BO.OpenOrderSortBy.AirDistance);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            // Always restore cursor
            Mouse.OverrideCursor = null;
        }
    }

    /// <summary>
    /// Event fired when user selects a row in the grid.
    /// Enables/Disables the button.
    /// </summary>
    private void ItemsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // If an item is selected, enable the button. Otherwise, disable it.
        if (ItemsDataGrid.SelectedItem != null)
            BtnAssign.IsEnabled = true;

        // If no item is selected, disable the button.
        else BtnAssign.IsEnabled = false;
    }

    #endregion Methods

    //==================== Observers ===================\\

    #region Observers

    private void OrderListObserver()
    {
        #region Stage 7 (for multithreading)
        if (_orderListMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        Dispatcher.BeginInvoke(async () =>
            {
                // The actual work to be done on the UI thread
                await RefreshOrderListAsync();

                // After completing the work, check if a restart was requested
                if (await _orderListMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    OrderListObserver();
            });
        #endregion Stage 7 (for multithreading)
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Subscribe observer once (make sure you also remove it on Window_Closed)
            s_bl.Order.AddObserver(OrderListObserver);

            await RefreshOrderListAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Window_Closed(object sender, EventArgs e)
        => s_bl.Order.RemoveObserver(OrderListObserver);

    #endregion Observers

    //==================== Events ===================\\

    #region Events
    
    private async void OpenNewDeliveryButton_Click(object sender, RoutedEventArgs e)
    {
        // Assign the selected order to the courier
        if (SelectedOrder == null) return;

        // Close the window
        Close();

        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        // Call BL to assign order
        await s_bl.Order.AssignOrderToCourier(UserData.s_UserId, CurrentCourier.CourierId, SelectedOrder.OrderId, SelectedOrder.ActualDistance);

        // Restore default cursor
        Mouse.OverrideCursor = null;
    }

    #endregion Events

}

