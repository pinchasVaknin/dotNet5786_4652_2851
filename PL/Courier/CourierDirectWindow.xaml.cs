namespace PL.Courier;

using BlApi;
using BO;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

public partial class CourierDirectWindow : Window
{

    //=================== Fields ==================\\

    #region Fields

    private static readonly IBl s_bl = Factory.Get();
    private int _courierId;

    #endregion Fields

    //================== Properties =================\\

    #region Properties

    // Dependency Property for Data Binding
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
        DataContext = this;
        RefreshData();
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

    #endregion Enumerables

    //=================== Methods ===================\\

    #region Methods


    private void RefreshData()
    {
        try
        {
            // Get Data
            CurrentCourier = s_bl.Courier.GetCourier(_courierId, _courierId);
            // Logic: Has Order vs No Order
            if (CurrentCourier.OrderInProgress != null)
            {
                // BUSY
                pnlNoOrder.Visibility = Visibility.Collapsed;
                pnlHasOrder.Visibility = Visibility.Visible;

                cmbVehicle.IsEnabled = false; // Cannot change vehicle during mission

                txtStatus.Text = "BUSY";
                StatusBadge.Background = Brushes.OrangeRed;
            }
            else
            {
                // IDLE
                pnlNoOrder.Visibility = Visibility.Visible;
                pnlHasOrder.Visibility = Visibility.Collapsed;

                cmbVehicle.IsEnabled = true;

                txtStatus.Text = "AVAILABLE";
                StatusBadge.Background = Brushes.SeaGreen;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
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
            s_bl.Courier.UpdateCourier(CurrentCourier.CourierId, CurrentCourier);
            MessageBox.Show("Profile updated!");
            RefreshData();
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

    private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
    {
        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        // Open window to pick order
        MessageBox.Show("Opening Order Selector...");
        // new OrderSelectionWindow(_courierId).ShowDialog();
        RefreshData();

        // Restore default cursor
        Mouse.OverrideCursor = null;
    }

    private void BtnFinishOrder_Click(object sender, RoutedEventArgs e)
    {
        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
            if (CurrentCourier.OrderInProgress != null)
            {
                // Call BL to finish
                //s_bl.Order.UpdateDelivery(CurrentCourier.OrderInProgress.OrderId, _courierId);
                MessageBox.Show("Delivery Finished!");
                RefreshData();
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

}