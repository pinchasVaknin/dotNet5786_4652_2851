namespace PL.Courier;

using Tools;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for CourierWindow.xaml
/// </summary>
public partial class CourierWindow : Window
{
    //==================== Fields ===================\\

    #region Fields

    // The entry point to the BL layer (Factory pattern).
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    #endregion Fields

    //==================== Properties ===================\\

    #region Properties

    /// <summary>
    /// Gets or sets the current courier associated with the operation.
    /// </summary>
    public BO.Courier CurrentCourier
    {
        get { return (BO.Courier)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }
    // registering the CurrentCourier dependency property
    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the text displayed on the action button.
    /// </summary>
    public string ActionButtonText
    {
        get { return (string)GetValue(ActionButtonTextProperty); }
        set { SetValue(ActionButtonTextProperty, value); }
    }
    // registering the ActionButtonText dependency property
    public static readonly DependencyProperty ActionButtonTextProperty =
        DependencyProperty.Register("ActionButtonText", typeof(string), typeof(CourierWindow), new PropertyMetadata("Add"));

    #endregion Properties

    //================== Constructors =================\\

    #region Constructors

    public CourierWindow()
    {
        InitializeComponent();

        // Initialize a new courier for adding
        CurrentCourier = new BO.Courier()
        {
            StartWorkDate = DateTime.Now
        };

        // Set the action button text to "Add"
        ActionButtonText = "Add";
    }

    public CourierWindow(int courierId)
    {
        InitializeComponent();

        try
        {
            // Fetch the existing courier details for updating
            CurrentCourier = s_bl.Courier.GetCourier(UserData.s_UserId, courierId);

            // Set the action button text to "Update"
            ActionButtonText = "Update";

            // Disable editing of the Courier ID field when updating
            if (TxtId != null) TxtId.IsEnabled = false;
        }
        catch
        {
            Close();
        }
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

    private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
            // Basic validation checks
            if (CurrentCourier.CourierId <= 0)
            {
                MessageBox.Show("Invalid ID", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Ensure the name is not empty
            if (string.IsNullOrEmpty(CurrentCourier.CourierFullName))
            {
                MessageBox.Show("Name is required", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Ensure the phone number is not empty
            if (CurrentCourier.MaxCourierDistance <= 0)
            {
                MessageBox.Show("Max Distance must be a positive number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Validate the courier data before proceeding
            if (ActionButtonText == "Add")
            {
                s_bl.Courier.AddCourier(UserData.s_UserId, CurrentCourier);
                MessageBox.Show("Courier added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            // Update existing courier
            else
            {
                s_bl.Courier.UpdateCourier(UserData.s_UserId, CurrentCourier);
                MessageBox.Show("Courier updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            // Restore default cursor
            Mouse.OverrideCursor = null;
        }
    }

    #endregion Methods
    
}