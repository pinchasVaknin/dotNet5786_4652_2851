namespace PL.Courier;

using BlApi;
using BO;
using PL.delivery;
using PL.Helpers;
using PL.Tools;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public partial class CourierDirectWindow : Window
{

    //=================== Fields ==================\\

    #region Fields

    private static readonly IBl s_bl = Factory.Get();
    private readonly int _courierId;
    private readonly ObserverMutex _courierMutex = new();

#endregion Fields

    //================== Properties =================\\

    #region Properties

    public BO.Courier CurrentCourier
    {
        get { return (BO.Courier)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }
    public static readonly DependencyProperty CurrentCourierProperty =
     DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierDirectWindow));

    #endregion Properties

    //================== Constructors =================\\

    #region Constructors

    public CourierDirectWindow(int id)
    {
   InitializeComponent();
        _courierId = id;
    }

    #endregion Constructors

    //================== Enumerables =================\\

    #region Enumerables

    public IEnumerable<BO.VehicleType> VehicleTypesList
    {
        get { return App.GetEnumValues(BO.VehicleType.All); }
 }

    public IEnumerable<BO.DeliveryFinishType> DeliveryStatusList
    {
   get { return App.GetEnumValues(BO.DeliveryFinishType.Cancelled); }
    }

    #endregion Enumerables

    //=================== Methods ===================\\

    #region Methods

    private void RefreshData()
    {
        try
   {
     CurrentCourier = s_bl.Courier.GetCourier(UserData.s_UserId, _courierId);
    if (CurrentCourier.OrderInProgress != null)
            {
           pnlNoOrder.Visibility = Visibility.Collapsed;
     pnlHasOrder.Visibility = Visibility.Visible;
          cmbVehicle.IsReadOnly = true;
        txtStatus.Text = "BUSY";
      StatusBadge.Background = Brushes.OrangeRed;
            }
            else
   {
      pnlNoOrder.Visibility = Visibility.Visible;
     pnlHasOrder.Visibility = Visibility.Collapsed;
    cmbVehicle.IsReadOnly = false;
 txtStatus.Text = "AVAILABLE";
     StatusBadge.Background = Brushes.SeaGreen;
            }
        }
        catch (Exception ex)
  {
     MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CanFinishOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        btnFinishOrder.IsEnabled = cmbOrderStatus.SelectedItem != null;
    }

    #endregion Methods

    //================ Event Handlers ================\\

    #region Event Handlers

    private void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
      Mouse.OverrideCursor = Cursors.Wait;
        try
        {
     var config = s_bl.Admin.GetConfig();
   if (CurrentCourier.MaxCourierDistance > config.MaxAirDistance)
        {
    throw new Exception($"Error: Distance must be under or equal to {config.MaxAirDistance}");
            }
            s_bl.Courier.UpdateCourier(CurrentCourier.CourierId, CurrentCourier);
       MessageBox.Show("Profile updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
    MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    finally
        {
            Mouse.OverrideCursor = null;
    }
    }

    private void BtnHistory_Click(object sender, RoutedEventArgs e)
    {
 Mouse.OverrideCursor = Cursors.Wait;
        try
        {
     new DeliveryHistoryWindow(CurrentCourier.CourierId).Show();
        }
   finally
        {
         Mouse.OverrideCursor = null;
 }
 }

    private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
    {
        Mouse.OverrideCursor = Cursors.Wait;
        try
 {
         new OpenDeliveryListWindow(CurrentCourier).ShowDialog();
        }
    finally
        {
            Mouse.OverrideCursor = null;
    }
    }

    private void BtnFinishOrder_Click(object sender, RoutedEventArgs e)
    {
        Mouse.OverrideCursor = Cursors.Wait;
    try
      {
       var selectedStatus = (BO.DeliveryFinishType)cmbOrderStatus.SelectedItem;
    if (CurrentCourier.OrderInProgress != null)
 {
                var deliveryId = CurrentCourier.OrderInProgress.DeliveryId;
       s_bl.Order.CompleteOrderHandling(UserData.s_UserId, CurrentCourier.CourierId, deliveryId, selectedStatus);
        MessageBox.Show("Delivery Finished!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
    {
              MessageBox.Show("No order in progress to finish.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
      catch (Exception ex)
        {
      MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
         Mouse.OverrideCursor = null;
        }
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
    new Login.LoginWindow().Show();
        Close();
    }

    #endregion Event Handlers

    //==================== Observers ===================\\

    #region Observers

    private void CourierObserver()
    {
 #region Stage 7 (for multithreading)
        if (_courierMutex.CheckAndSetLoadInProgressOrRestartRequired())
         return;

        Dispatcher.BeginInvoke(async () =>
        {
            try
            {
        RefreshData();
       }
    finally
    {
    if (await _courierMutex.UnsetLoadInProgressAndCheckRestartRequested())
     CourierObserver();
        }
        });
        #endregion Stage 7 (for multithreading)
  }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
  s_bl.Courier.AddObserver(_courierId, CourierObserver);
        RefreshData();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        s_bl.Courier.RemoveObserver(_courierId, CourierObserver);
    }

    #endregion Observers

}
