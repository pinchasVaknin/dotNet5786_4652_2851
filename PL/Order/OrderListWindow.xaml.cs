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
using PL.Tools;

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

    private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbFilterValue != null)
        {
            CmbFilterValue.ItemsSource = null;
        }
        RefreshOrderList();
    }

    private void CmbFilterValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshOrderList();
    }

    private void RefreshOrderList()
    {
        try
        {
            // If no filter is applied, retrieve all orders
            if (OrderCategoryFilter == BO.OrderInListFilterBy.All)
            {
                // Retrieve all orders for the user
                OrderList = s_bl?.Order.GetOrders(UserData.s_UserId) ?? Enumerable.Empty<BO.OrderInList>();

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
                CmbFilterValue.ItemsSource = null;

                switch (OrderCategoryFilter)
                {
                    case BO.OrderInListFilterBy.OrderStatus:
                        CmbFilterValue.ItemsSource = Enum.GetValues(typeof(BO.OrderStatus));
                        break;

                    case BO.OrderInListFilterBy.TypeOfOrder:
                        CmbFilterValue.ItemsSource = Enum.GetValues(typeof(BO.TypeOfOrder));
                        break;

                    case BO.OrderInListFilterBy.ScheduleStatus:
                        CmbFilterValue.ItemsSource = Enum.GetValues(typeof(BO.ScheduleStatus));
                        break;
                }

                if (CmbFilterCategory.SelectedItem == null || CmbFilterValue.SelectedItem == null) return;
                var category = (BO.OrderInListFilterBy)CmbFilterCategory.SelectedItem;

                switch (category)
                {
                    case BO.OrderInListFilterBy.OrderStatus:
                        var status = (BO.OrderStatus)CmbFilterValue.SelectedItem;
                        OrderList = s_bl?.Order.GetOrders(UserData.s_UserId, category, status);
                        break;
                    case BO.OrderInListFilterBy.TypeOfOrder:
                        var type = (BO.TypeOfOrder)CmbFilterValue.SelectedItem;
                        OrderList = s_bl?.Order.GetOrders(UserData.s_UserId, category, type);
                        break;
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

    private void OrderDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Get the selected order from the DataContext of the DataGrid
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        // Open a new window to add a new order
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        // Get the selected order from the DataContext of the button
    }

    #endregion Methods

    //==================== Observers ===================\\

    #region Observers

    private void OrderListObserver()
                    => RefreshOrderList();

    private void Window_Loaded(object sender, RoutedEventArgs e)
                    => s_bl.Order.AddObserver(OrderListObserver);

    private void Window_Closed(object sender, EventArgs e)
                    => s_bl.Order.RemoveObserver(OrderListObserver);

    #endregion Observers

}
