namespace PL.Courier;

using BlApi;
using BO;
using PL.delivery;
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

    // Courier ID for observer registration
    private readonly int _courierId;

    #endregion Fields

    //================== Properties =================\\

    #region Properties

    // Dependency Property for Data Binding
    public BO.Courier CurrentCourier
    {
        get { return (BO.Courier)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }
    // Using a DependencyProperty as the backing store for CurrentCourier.  This enables animation, styling, binding, etc...
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

    // Provides a list of vehicle types excluding the 'All' option
    public IEnumerable<BO.VehicleType> VehicleTypesList
    {
        get
        {
            return App.GetEnumValues(BO.VehicleType.All);
        }
    }

    public IEnumerable<BO.DeliveryFinishType> DeliveryStatusList
    {
        get
        {
            return App.GetEnumValues(BO.DeliveryFinishType.Cancelled);
        }
    }

    #endregion Enumerables

    //=================== Methods ===================\\

    #region Methods

    private void RefreshData()
    {
        try
        {
            // Get Data
            CurrentCourier = s_bl.Courier.GetCourier(UserData.s_UserId, _courierId);
            // Logic: Has Order vs No Order
            if (CurrentCourier.OrderInProgress != null)
            {
                // BUSY
                pnlNoOrder.Visibility = Visibility.Collapsed;
                pnlHasOrder.Visibility = Visibility.Visible;

                cmbVehicle.IsReadOnly = true; // Disable vehicle change when busy

                txtStatus.Text = "BUSY";
                StatusBadge.Background = Brushes.OrangeRed;
            }
            else
            {
                // IDLE
                pnlNoOrder.Visibility = Visibility.Visible;
                pnlHasOrder.Visibility = Visibility.Collapsed;

                cmbVehicle.IsReadOnly = false; // Enable vehicle change when idle

                txtStatus.Text = "AVAILABLE";
                StatusBadge.Background = Brushes.SeaGreen;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void CanFinishOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Enable Finish Order button only if a status is selected
        btnFinishOrder.IsEnabled = cmbOrderStatus.SelectedItem != null;
    }

    #endregion Methods

    //================ Event Handlers ================\\

    #region Event Handlers

    private void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {

        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
            var config = s_bl.Admin.GetConfig();
            if (CurrentCourier.MaxCourierDistance > config.MaxAirDistance)
            {
                throw new Exception($"Error: Distance must be under or equal to {config.MaxAirDistance} ");
            }
            else
            {
                s_bl.Courier.UpdateCourier(CurrentCourier.CourierId, CurrentCourier);
                MessageBox.Show("Profile updated!");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            // Restore default cursor
            Mouse.OverrideCursor = null;
        }
    }

    private void BtnHistory_Click(object sender, RoutedEventArgs e)
    {
        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
            // Open history window
            new DeliveryHistoryWindow(CurrentCourier.CourierId).Show();
        }
        finally
        {
            // Restore default cursor
            Mouse.OverrideCursor = null;
        }
    }

    private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
    {
        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
            // Open window to pick order
            new OpenDeliveryListWindow(CurrentCourier).ShowDialog();
        }
        finally
        {
            // Restore default cursor
            Mouse.OverrideCursor = null;
        }
    }

    private void BtnFinishOrder_Click(object sender, RoutedEventArgs e)
    {
        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
            // Get selected finish status
            var selectedStatus = (BO.DeliveryFinishType)cmbOrderStatus.SelectedItem;

            // Complete order handling
            if (CurrentCourier.OrderInProgress != null)
            {
                var deliveryId = CurrentCourier.OrderInProgress.DeliveryId;

                s_bl.Order.CompleteOrderHandling(UserData.s_UserId, CurrentCourier.CourierId, deliveryId, selectedStatus);
                MessageBox.Show("Delivery Finished!");
            }
            else
            {
                MessageBox.Show("No order in progress to finish.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            // Restore default cursor
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
                    => RefreshData();

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
