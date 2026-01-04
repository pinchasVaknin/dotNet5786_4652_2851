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

    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public BO.OrderInListFilterBy OrderFilter { get; set; } = BO.OrderInListFilterBy.All;

    //================ OrderList Property =================\\

    public IEnumerable<BO.OrderInList> OrderList
    {
        get { return (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty); }
        set { SetValue(OrderListProperty, value); }
    }

    // Using a DependencyProperty as the backing store for OrderList.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register("OrderList", typeof(IEnumerable<BO.OrderInList>), typeof(OrderListWindow), new PropertyMetadata(null));




    //================== Constructor =================\\

    public OrderListWindow()
    {
        InitializeComponent();
    }

    private void OrderFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // TODO: Replace with real user ID after Login implementation
        int adminId = 333333333;

        OrderList = (OrderFilter == BO.OrderInListFilterBy.All) ?
            s_bl.Order.GetOrders(adminId) :
            s_bl.Order.GetOrders(adminId, OrderFilter);
    }
}
