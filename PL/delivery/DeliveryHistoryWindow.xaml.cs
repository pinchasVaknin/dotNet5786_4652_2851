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

namespace PL.delivery;

/// <summary>
/// Interaction logic for DeliveryHistoryWindow.xaml
/// </summary>
public partial class DeliveryHistoryWindow : Window
{

    //==================== Fields ===================\\

    #region Fields

    // The entry point to the BL layer (Factory pattern).
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // Courier ID for observer registration
    private readonly int _courierId;

    // Selected Delivery from the data grid
    public BO.ClosedDeliveryInList? SelectedDelivery { get; set; } = null;

    // Stage 7: Mutex field for thread-safe observer updates
    private readonly ObserverMutex _deliveryListMutex = new();

    #endregion Fields

    //================ DeliveryList Property =================\\

    #region DeliveryList Property

    public IEnumerable<BO.ClosedDeliveryInList> DeliveryList
    {
        get { return (IEnumerable<BO.ClosedDeliveryInList>)GetValue(DeliveryListProperty); }
        set { SetValue(DeliveryListProperty, value); }
    }
    // Using a DependencyProperty as the backing store for DeliveryList.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DeliveryListProperty =
        DependencyProperty.Register("DeliveryList", typeof(IEnumerable<BO.ClosedDeliveryInList>), typeof(DeliveryHistoryWindow));

    public BO.Courier CurrentCourier
    {
        get { return (BO.Courier)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }
    // Using a DependencyProperty as the backing store for CurrentCourier.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(DeliveryHistoryWindow));

    #endregion DeliveryList Property

    //================= Constructor =================\\

    #region Constructor

    public DeliveryHistoryWindow(int courierId)
    {
        _courierId = courierId;
        CurrentCourier = s_bl.Courier.GetCourier(UserData.s_UserId, courierId);
        InitializeComponent();
    }

    #endregion Constructor

    //==================== Methods ===================\\

    #region Methods

    private void RefreshDeliveryList()
    {
        try
        {
            // Retrieve closed deliveries for the courier
            DeliveryList = s_bl.Order.GetClosedDeliveriesByCourier(UserData.s_UserId, _courierId) ??
                Enumerable.Empty<BO.ClosedDeliveryInList>();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            DeliveryList = Enumerable.Empty<BO.ClosedDeliveryInList>();
        }
    }

    #endregion Methods

    //==================== Observers ===================\\

    #region Observers

    private void DeliveryListObserver()
    {
   #region Stage 7 (for multithreading)
   if (_deliveryListMutex.CheckAndSetLoadInProgressOrRestartRequired())
   return;

    Dispatcher.BeginInvoke(async () =>
        {
      // The actual work to be done on the UI thread
        RefreshDeliveryList();

            // After completing the work, check if a restart was requested
     if (await _deliveryListMutex.UnsetLoadInProgressAndCheckRestartRequested())
         DeliveryListObserver();
        });
        #endregion Stage 7 (for multithreading)
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      s_bl.Order.AddObserver(DeliveryListObserver);
RefreshDeliveryList();
    }

    private void Window_Closed(object sender, EventArgs e)
     => s_bl.Order.RemoveObserver(DeliveryListObserver);

    #endregion Observers

    //==================== Event Handlers ===================\\

    #region Event Handlers

    private void DeliveryDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Show delivery history details
        if (SelectedDelivery is null) return;

        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            // Retrieve full order details including delivery history
            var currentOrder = s_bl.Order.GetDeliveryHistoryForCourier(UserData.s_UserId, _courierId, SelectedDelivery.OrderId);

            // If no delivery history is available, inform the user
            new DeliveryHistoryView(currentOrder).ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Error (Delivery History View)", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            // Restore default cursor
            Mouse.OverrideCursor = null;
        }
    }

    #endregion Event Handlers

}
