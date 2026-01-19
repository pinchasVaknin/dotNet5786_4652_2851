using PL.delivery;
using PL.Order;
using PL.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Order;

/// <summary>
/// Interaction logic for OrderWindow.xaml
/// </summary>
public partial class OrderWindow : Window
{
    //==================== Fields ===================\\

    #region Fields

    // The entry point to the BL layer (Factory pattern).
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // The order ID associated with this window (used for observers)
    private readonly int _orderId;

    // Flag to indicate if loading the order failed
    private bool _LoadFailed = false;

    #endregion Fields

    //==================== Properties ===================\\

    #region Properties

    // Collection for the DataGrid
    public ObservableCollection<OrderItem> ItemsCollection { get; set; } = new();

    /// <summary>
    /// Gets or sets the current order associated with the application.
    /// </summary>
    public BO.Order CurrentOrder
    {
        get { return (BO.Order)GetValue(CurrentOrderProperty); }
        set { SetValue(CurrentOrderProperty, value); }
    }
    // Using a DependencyProperty as the backing store for CurrentOrder.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CurrentOrderProperty =
        DependencyProperty.Register("CurrentOrder", typeof(BO.Order), typeof(OrderWindow), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the text displayed on the action button.
    /// </summary>
    public string ActionButtonText
    {
        get { return (string)GetValue(ActionButtonTextProperty); }
        set { SetValue(ActionButtonTextProperty, value); }
    }
    // Using a DependencyProperty as the backing store for ActionButtonText.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ActionButtonTextProperty =
        DependencyProperty.Register("ActionButtonText", typeof(string), typeof(OrderWindow), new PropertyMetadata("Add Order"));

    /// <summary>
    /// Logic flag to determine if the window is in Update mode or Add mode.
    /// Used by XAML converters to hide/show UI elements.
    /// </summary>
    public bool IsUpdateMode
    {
        get { return (bool)GetValue(IsUpdateModeProperty); }
        set { SetValue(IsUpdateModeProperty, value); }
    }
    // Using a DependencyProperty as the backing store for IsUpdateMode.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsUpdateModeProperty =
        DependencyProperty.Register("IsUpdateMode", typeof(bool), typeof(OrderWindow), new PropertyMetadata(false));

    public bool IsEditable
    {
        get { return (bool)GetValue(IsEditableProperty); }
        set { SetValue(IsEditableProperty, value); }
    }
    // Using a DependencyProperty as the backing store for IsEditable.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsEditableProperty =
        DependencyProperty.Register("IsEditable", typeof(bool), typeof(OrderWindow), new PropertyMetadata(true));

    #endregion Properties

    //================== Constructors =================\\

    #region Constructors

    /// <summary>
    /// Constructor for Adding a new Order
    /// </summary>
    public OrderWindow()
    {
        InitializeComponent();

        IsUpdateMode = false;
        ActionButtonText = "Add Order";

        var timeNow = s_bl.Admin.GetClock();
        var maxTime = s_bl.Admin.GetConfig().MaxDelTimeRnge;

        // Initialize a new Order with default values
        CurrentOrder = new BO.Order()
        {
            OrderOpenTime = timeNow,
            MaxDeliveryTime = timeNow + maxTime,
            TimeRemaining = maxTime,
            OrderStatus = BO.OrderStatus.Open,
            TypeOfOrder = BO.TypeOfOrder.TV,
        };

        InitializeUI();
    }

    /// <summary>
    /// Constructor for Updating an existing Order
    /// </summary>
    public OrderWindow(int orderId)
    {

        InitializeComponent();

        _orderId = orderId;
        IsUpdateMode = true;
        ActionButtonText = "Update Order";

        // Load the existing order details
        RefreshOrder();

        // If loading failed, close the window
        if (_LoadFailed)
        {
            Close();
            return;
        }

        InitializeUI();
    }

    #endregion Constructors

    //==================== Methods ===================\\

    #region Methods

    /// <summary>
    /// Initializes the user interface components for managing orders and products.
    /// </summary>
    /// <remarks>This method sets up the data bindings for the product and order category ComboBoxes. If the
    /// application is in update mode, the order category ComboBox is pre-populated  with the current order's type and
    /// is disabled to prevent changes. The product list  is refreshed based on the selected order type.</remarks>
    private void InitializeUI()
    {
        // Populate Order Category ComboBox
        CmbFilterCategory.ItemsSource = Enum.GetValues(typeof(BO.TypeOfOrder));

        // Populate Product ComboBox
        if (IsUpdateMode)
        {
            // In Update mode, lock the order type to the existing order's type
            CmbFilterCategory.SelectedItem = CurrentOrder.TypeOfOrder;
            CmbFilterCategory.IsEnabled = false;
            RefreshProductList(CurrentOrder.TypeOfOrder);
        }
    }

    /// <summary>
    /// Refreshes the current order details from the business logic layer.
    /// </summary>
    private void RefreshOrder()
    {
        try
        {
            // Fetch the latest order details
            CurrentOrder = s_bl.Order.GetOrder(UserData.s_UserId, _orderId);

            // Clear existing items
            ItemsCollection.Clear();

            // Parse the order details and populate the items collection
            foreach (var (model, qty) in ParseOrderDetail(CurrentOrder.OrderDetail))
            {
                double price = s_bl.Order.GetProductPrice(model);
                ItemsCollection.Add(new OrderItem { Model = model, Quantity = qty, Price = price });
            }
            // Update the editability based on order status
            IsEditable = (CurrentOrder.OrderStatus == BO.OrderStatus.Open ||
                          CurrentOrder.OrderStatus == BO.OrderStatus.InProgress);
        }
        catch
        {
            MessageBox.Show($"Failed to load order: {_orderId}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            _LoadFailed = true;
        }
    }

    /// <summary>
    /// Updates the enabled state of the order type ComboBox based on the current mode and cart contents.
    /// </summary>
    /// <remarks>The ComboBox is disabled if the application is in update mode or if there are items in the
    /// cart. Otherwise, it remains enabled.</remarks>
    private void UpdateOrderTypeLock()
    {
        // Lock the order type ComboBox if in Update mode or if there are items in the cart
        if (!IsUpdateMode)
        {
            CmbFilterCategory.IsEnabled = ItemsCollection.Count == 0;
        }
    }

    /// <summary>
    /// Handles the selection change event for the order category ComboBox.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CmbOrderCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Update the selected filter category based on user selection
        if (CmbFilterCategory.SelectedItem is BO.TypeOfOrder type)
        {
            // Update the current order's type
            CurrentOrder.TypeOfOrder = type;

            // Refresh the product list based on the selected type
            RefreshProductList(type);
        }
    }

    /// <summary>
    /// Refreshes the product list based on the selected order type.
    /// </summary>
    /// <param name="type">The selected type of order.</param>
    private void RefreshProductList(BO.TypeOfOrder type)
    {
        // Clear existing items
        IEnumerable<string> products = new List<string>();

        // Populate products based on selected type
        switch (type)
        {
            case BO.TypeOfOrder.Smartphone:
                products = Enum.GetNames(typeof(BO.Catalog.SmartphoneDetails));
                break;

            case BO.TypeOfOrder.Laptop:
                products = Enum.GetNames(typeof(BO.Catalog.LaptopDetails));
                break;

            case BO.TypeOfOrder.Tablet:
                products = Enum.GetNames(typeof(BO.Catalog.TabletDetails));
                break;

            case BO.TypeOfOrder.TV:
                products = Enum.GetNames(typeof(BO.Catalog.TVDetails));
                break;

            case BO.TypeOfOrder.Camera:
                products = Enum.GetNames(typeof(BO.Catalog.CameraDetails));
                break;

            case BO.TypeOfOrder.Audio:
                products = Enum.GetNames(typeof(BO.Catalog.AudioDetails));
                break;

            case BO.TypeOfOrder.SmartHome:
                products = Enum.GetNames(typeof(BO.Catalog.SmartHomeDetails));
                break;

            case BO.TypeOfOrder.GamingConsole:
                products = Enum.GetNames(typeof(BO.Catalog.GamingConsoleDetails));
                break;

            case BO.TypeOfOrder.Accessory:
                products = Enum.GetNames(typeof(BO.Catalog.AccessoryDetails));
                break;

            default:
                // If no valid type is selected, clear the product list
                CmbProductModel.ItemsSource = null;
                return;
        }

        // Sort and set the items source
        CmbProductModel.ItemsSource = products.OrderBy(x => x);
    }

    /// <summary>
    /// Parses the order detail string into a collection of model and quantity tuples.
    /// </summary>
    /// <param name="detail"> The order detail string to parse.</param>
    /// <returns> An enumerable of tuples containing model names and their corresponding quantities.</returns>
    private static IEnumerable<(string Model, int Qty)> ParseOrderDetail(string? detail)
    {
        if (string.IsNullOrWhiteSpace(detail))
            yield break;

        // "TV => Sony{3}, Samsung{7}"
        var parts = detail.Split(new[] { "=>" }, 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2) yield break;

        foreach (var token in parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            int open = token.LastIndexOf('{');
            int close = token.LastIndexOf('}');

            if (open <= 0 || close <= open) continue;

            string model = token.Substring(0, open).Trim();
            string qtyStr = token.Substring(open + 1, close - open - 1).Trim();

            if (!string.IsNullOrWhiteSpace(model) && int.TryParse(qtyStr, out int qty) && qty > 0)
                yield return (model, qty);
        }
    }

    #endregion Methods

    //==================== Event Handlers ===================\\

    #region Event Handlers

    /// <summary>
    /// Adds an item to the local list (UI only)
    /// </summary>
    private void BtnAddItem_Click(object sender, RoutedEventArgs e)
    {

        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        // Validate that a category is selected
        if (CmbFilterCategory.SelectedItem == null)
        {
            MessageBox.Show("Please select a valid Category first.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);

            // Restore default cursor
            Mouse.OverrideCursor = null;
            return;
        }

        // Validate and add the item
        if (CmbProductModel.SelectedItem is string modelName &&
            int.TryParse(TxtQuantity.Text, out int qty) && qty > 0)
        {
            var existingItem = ItemsCollection.FirstOrDefault(i => i.Model == modelName);

            // If item exists, update quantity; otherwise, add new item
            if (existingItem != null)
            {
                existingItem.Quantity += qty;
            }
            else
            {
                // Get the price from BL
                double price = s_bl.Order.GetProductPrice(modelName);
                ItemsCollection.Add(new OrderItem
                {
                    Model = modelName,
                    Quantity = qty,
                    Price = price
                });
            }

            TxtQuantity.Text = "1";

            // Lock the order type if needed
            UpdateOrderTypeLock();
        }
        else
        {
            MessageBox.Show("Please select a valid model and quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        // Restore default cursor
        Mouse.OverrideCursor = null;
    }

    /// <summary>
    /// Removes an item from the local list (UI only)
    /// </summary>
    /// <param name="sender"> The source of the event.</param>
    /// <param name="e"> The event data.</param>
    private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
    {
        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        // Remove the item associated with the clicked button
        if (sender is Button btn && btn.DataContext is OrderItem item)
            ItemsCollection.Remove(item);

        // Update the order type lock state
        UpdateOrderTypeLock();

        // Restore default cursor
        Mouse.OverrideCursor = null;
    }

    /// <summary>
    /// Removes one quantity of an item from the local list (UI only)
    /// </summary>
    /// <param name="sender"> The source of the event.</param>
    /// <param name="e"> The event data.</param>
    private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
    {
        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        // Remove one quantity of the item associated with the clicked button
        if (sender is Button btn && btn.DataContext is OrderItem item)
        {
            // Decrease quantity or remove item
            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
            else
            {
                // Quantity is 1, so remove the row entirely
                ItemsCollection.Remove(item);
            }
        }

        // Update the order type lock state
        UpdateOrderTypeLock();

        // Restore default cursor
        Mouse.OverrideCursor = null;
    }

    /// <summary>
    /// Handles the final Save (Add or Update) operation
    /// </summary>
    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
            var itemsList = ItemsCollection.Select(i => (i.Model, i.Quantity)).ToList();
            s_bl.Order.UpdateOrderDetails(CurrentOrder, itemsList);

            if (!IsUpdateMode)
                await s_bl.Order.AddOrder(UserData.s_UserId, CurrentOrder);
            else
                await s_bl.Order.UpdateOrder(UserData.s_UserId, CurrentOrder);

            MessageBox.Show(IsUpdateMode ? "Updated successfully" : "Added successfully", "Success");
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    /// <summary>
    /// Opens the delivery history window (Only visible in Update mode)
    /// </summary>
    private void BtnViewDeliveries_Click(object sender, RoutedEventArgs e)
    {
        // Show wait cursor
        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
            if (CurrentOrder.DeliveryPerOrderInList == null || CurrentOrder.DeliveryPerOrderInList.Count == 0)
            {
                MessageBox.Show("No history available for this order.", "Info");

                // Restore default cursor
                Mouse.OverrideCursor = null;
                return;
            }

            // Create and configure the delivery history window
            var trackingWindow = new DeliveryHistoryView(CurrentOrder.DeliveryPerOrderInList);
            trackingWindow.Owner = this;

            // Show the delivery history window as a dialog
            trackingWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            // Restore default cursor
            Mouse.OverrideCursor = null;
        }

    }

    #endregion Event Handlers

    //==================== Observers ===================\\

    #region Observers

    private void OrderObserver()
                    =>RefreshOrder();


    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (!IsUpdateMode) return;
        s_bl.Order.AddObserver(_orderId, OrderObserver);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (!IsUpdateMode) return;
        s_bl.Order.RemoveObserver(_orderId, OrderObserver);
    }

    #endregion Observers

}