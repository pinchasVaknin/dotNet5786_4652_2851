using BO;
using PL.Helpers;
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

namespace PL.Order;

/// <summary>
/// Interaction logic for OrderListWindow.xaml
/// </summary>
public partial class OrderListWindow : Window
{

    //==================== Fields ===================\\

 #region Fields

    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    public BO.OrderInListFilterBy OrderCategoryFilter { get; set; } = BO.OrderInListFilterBy.All;
    public BO.OrderInList? SelectedOrder { get; set; }
    public BO.ScheduleStatus? ScheduleStatusFilter { get; set; } = null;

    // Stage 7: Mutex field for thread-safe observer updates
    private readonly ObserverMutex _orderListMutex = new();

    #endregion Fields

 //================ OrderList Property =================\\

    #region OrderList Property

    public IEnumerable<BO.OrderInList> OrderList
    {
        get { return (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty); }
        set { SetValue(OrderListProperty, value); }
    }

    // Using a DependencyProperty as the backing store for OrderList.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register("OrderList", typeof(IEnumerable<BO.OrderInList>), typeof(OrderListWindow), new PropertyMetadata(null));

    #endregion OrderList Property

    //================== Constructor =================\\

    #region Constructor

    public OrderListWindow()
    {
 InitializeComponent();
    }

    #endregion Constructor

    //==================== Methods ===================\\

    #region Methods

 /// <summary>
    /// Handles the selection change event for the order category ComboBox.
    /// </summary>
 /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
  private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Update the selected filter category based on user selection
   if (CmbFilterCategory.SelectedItem is BO.OrderInListFilterBy selectedCategory)
        {
     OrderCategoryFilter = selectedCategory;
        }

   // Reset the filter value ComboBox
        if (CmbFilterValue != null)
   {
        CmbFilterValue.ItemsSource = null;
       CmbFilterValue.SelectedItem = null;
      }

        //  Refresh the order list based on the new filter category
        RefreshOrderList();
    }

    /// <summary>
    /// Handles the selection change event for the order filter value ComboBox.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CmbFilterValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
   RefreshOrderList();
    }

    /// <summary>
    /// Refreshes the list of orders based on the selected filter criteria.
    /// </summary>
    private void RefreshOrderList()
    {
        try
    {
        // If a ScheduleStatus filter is applied, filter orders by that status
          if (ScheduleStatusFilter != null)
  {
      // Disable filter controls
      if (CmbFilterCategory != null) CmbFilterCategory.IsEnabled = false;
    if (CmbFilterValue != null) CmbFilterValue.IsEnabled = false;

      // Get all orders for the user
    var allOrders = s_bl?.Order.GetOrders(UserData.s_UserId) ?? Enumerable.Empty<BO.OrderInList>();

       // Filter orders by the selected OrderStatus and ScheduleStatus
        var targetStatus = (BO.OrderStatus)OrderCategoryFilter;
      OrderList = allOrders.Where(o => o.OrderStatus == targetStatus && o.ScheduleStatus == ScheduleStatusFilter);

                // Exit the method early
           return;
            }

         // Enable filter controls
          if (CmbFilterCategory != null) CmbFilterCategory.IsEnabled = true;

            // If no specific filter is selected, retrieve all orders
        if (OrderCategoryFilter == BO.OrderInListFilterBy.All)
            {
// Get all orders for the user
       OrderList = s_bl?.Order.GetOrders(UserData.s_UserId) ?? Enumerable.Empty<BO.OrderInList>();

            // Reset filter value ComboBox
         if (CmbFilterValue != null)
       {
          CmbFilterValue.ItemsSource = null;
         CmbFilterValue.IsEnabled = false;
 }
    }
  else
            {
             // A specific filter is selected
 if (CmbFilterValue == null) return;

  // Enable the filter value ComboBox
      CmbFilterValue.IsEnabled = true;

           // Populate the filter value ComboBox if not already populated
   if (CmbFilterValue.ItemsSource == null)
             {
   // Populate based on the selected filter category
  switch (OrderCategoryFilter)
    {
 // Populate OrderStatus values
      case BO.OrderInListFilterBy.OrderStatus:
           CmbFilterValue.ItemsSource = Enum.GetValues(typeof(BO.OrderStatus));
               break;

  // Populate TypeOfOrder values
 case BO.OrderInListFilterBy.TypeOfOrder:
          CmbFilterValue.ItemsSource = Enum.GetValues(typeof(BO.TypeOfOrder));
      break;

  // Populate ScheduleStatus values
   case BO.OrderInListFilterBy.ScheduleStatus:
            CmbFilterValue.ItemsSource = Enum.GetValues(typeof(BO.ScheduleStatus));
           break;
        }
        }

 // Ensure both filter category and value are selected
             if (CmbFilterCategory.SelectedItem == null || CmbFilterValue.SelectedItem == null) return;

  // Retrieve orders based on the selected filter criteria
   var category = (BO.OrderInListFilterBy)CmbFilterCategory.SelectedItem;

       // Switch based on the selected filter category
         switch (category)
       {
      // Filter by OrderStatus
 case BO.OrderInListFilterBy.OrderStatus:
           var status = (BO.OrderStatus)CmbFilterValue.SelectedItem;
    OrderList = s_bl?.Order.GetOrders(UserData.s_UserId, category, status);
          break;

         // Filter by TypeOfOrder
  case BO.OrderInListFilterBy.TypeOfOrder:
  var type = (BO.TypeOfOrder)CmbFilterValue.SelectedItem;
    OrderList = s_bl?.Order.GetOrders(UserData.s_UserId, category, type);
  break;

      // Filter by ScheduleStatus
           case BO.OrderInListFilterBy.ScheduleStatus:
         var scheduleStatus = (BO.ScheduleStatus)CmbFilterValue.SelectedItem;
  OrderList = s_bl?.Order.GetOrders(UserData.s_UserId, category, scheduleStatus);
             break;
  }
        }
     }
        catch (Exception ex)
      {
       MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the click event for the Cancel button to cancel an order.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        // Confirm deletion
        if (sender is Button btn && btn.DataContext is BO.OrderInList orderToCancel)
        {
            // Show confirmation dialog
       MessageBoxResult result = MessageBox.Show(
       $"Are you sure you want to Cancel order: {orderToCancel.OrderId}?",
       "Cancel Confirmation",
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
     
       s_bl.Order.CancelOrder(UserData.s_UserId, orderToCancel.OrderId);

            // Notify user of successful deletion
  MessageBox.Show("Canceld successfully.");

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

    /// <summary>
    /// Handles the mouse double-click event on the order data grid to open the order details window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OrderDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (OrderDataGrid.SelectedItem is BO.OrderInList selectedOrder)
    {
            new OrderWindow(selectedOrder.OrderId).Show();
        }
    }

    /// <summary>
    /// Handles the click event for the Add button to open a new order window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
/// <param name="e">The event data.</param>
    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        new OrderWindow().Show();
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
    RefreshOrderList();

    // After completing the work, check if a restart was requested
       if (await _orderListMutex.UnsetLoadInProgressAndCheckRestartRequested())
         OrderListObserver();
        });
        #endregion Stage 7 (for multithreading)
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
   s_bl.Order.AddObserver(OrderListObserver);
        RefreshOrderList();
    }

    private void Window_Closed(object sender, EventArgs e)
     => s_bl.Order.RemoveObserver(OrderListObserver);

    #endregion Observers

}
