namespace PL.Courier;

using BO;
using PL.Helpers;
using PL.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for CourierListWindow.xaml
/// </summary>
public partial class CourierListWindow : Window
{

    //==================== Fields ===================\\

    #region Fields

    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    public BO.CourierInListFilterBy CourierCategoryFilter { get; set; } = BO.CourierInListFilterBy.All;
    public BO.CourierInList? SelectedCourier { get; set; }
    private readonly ObserverMutex _courierListMutex = new();

    #endregion Fields

    //================== CourierList Property =================\\

    #region CourierList Property

    public IEnumerable<BO.CourierInList> CourierList
  {
        get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
        set { SetValue(CourierListProperty, value); }
    }
    public static readonly DependencyProperty CourierListProperty =
      DependencyProperty.Register("CourierList", typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

    #endregion CourierList Property

    //================== Constructor =================\\

#region Constructor

    public CourierListWindow()
    {
   InitializeComponent();
    }

    #endregion Constructor

 //==================== Methods ===================\\

    #region Methods

    private void CourierFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbFilterCategory.SelectedItem is BO.CourierInListFilterBy selectedCategory)
    {
   CourierCategoryFilter = selectedCategory;
     }

     if (CmbFilterValue != null)
        {
        CmbFilterValue.ItemsSource = null;
  CmbFilterValue.SelectedItem = null;
        }

        RefreshCourierList();
    }

    private void CmbFilterValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshCourierList();
    }

    private void RefreshCourierList()
    {
        try
   {
  if (CourierCategoryFilter == BO.CourierInListFilterBy.All)
          {
             CourierList = s_bl?.Courier.GetCouriers(UserData.s_UserId) ?? Enumerable.Empty<BO.CourierInList>();

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

  if (CmbFilterValue.ItemsSource == null)
    {
       switch (CourierCategoryFilter)
      {
     case BO.CourierInListFilterBy.CourierIsActive:
        CmbFilterValue.ItemsSource = new List<object> { true, false };
     break;
         case BO.CourierInListFilterBy.VehicleType:
   CmbFilterValue.ItemsSource = Enum.GetValues(typeof(BO.VehicleType));
  break;
      case BO.CourierInListFilterBy.OrderIdInHandle:
        CmbFilterValue.ItemsSource = new List<object> { true, false };
  break;
    }
       }

   if (CmbFilterValue.SelectedItem == null) return;

    switch (CourierCategoryFilter)
                {
           case BO.CourierInListFilterBy.CourierIsActive:
    var status = (bool)CmbFilterValue.SelectedItem;
           CourierList = s_bl?.Courier.GetCouriers(UserData.s_UserId, CourierCategoryFilter, status);
         break;
       case BO.CourierInListFilterBy.VehicleType:
     var type = (BO.VehicleType)CmbFilterValue.SelectedItem;
         CourierList = s_bl?.Courier.GetCouriers(UserData.s_UserId, CourierCategoryFilter, type);
        break;
      case BO.CourierInListFilterBy.OrderIdInHandle:
      var scheduleStatus = (bool)CmbFilterValue.SelectedItem;
          CourierList = s_bl?.Courier.GetCouriers(UserData.s_UserId, CourierCategoryFilter, scheduleStatus);
        break;
      }
 }
        }
     catch (Exception ex)
        {
          MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion Methods

    //==================== Observers ===================\\

    #region Observers

    private void CourierListObserver()
    {
        #region Stage 7 (for multithreading)
   if (_courierListMutex.CheckAndSetLoadInProgressOrRestartRequired())
          return;

  Dispatcher.BeginInvoke(async () =>
      {
  try
  {
    RefreshCourierList();
      }
         finally
        {
  if (await _courierListMutex.UnsetLoadInProgressAndCheckRestartRequested())
          CourierListObserver();
    }
   });
   #endregion Stage 7 (for multithreading)
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
     s_bl.Courier.AddObserver(CourierListObserver);
     RefreshCourierList();
    }

    private void Window_Closed(object sender, EventArgs e)
        => s_bl.Courier.RemoveObserver(CourierListObserver);

    #endregion Observers

    //================== Event Handlers =================\\

    #region Event Handlers

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        new CourierWindow().Show();
    }

    private void CourierDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
   if (SelectedCourier != null)
        {
     new CourierWindow(SelectedCourier.CourierId).Show();
      }
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
  if (sender is Button btn && btn.DataContext is BO.CourierInList courierToDelete)
    {
            MessageBoxResult result = MessageBox.Show(
      $"Are you sure you want to delete courier: {courierToDelete.CourierFullName}?",
   "Delete Confirmation",
     MessageBoxButton.YesNo,
         MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
                return;

          Mouse.OverrideCursor = Cursors.Wait;
   try
       {
           s_bl.Courier.DeleteCourier(UserData.s_UserId, courierToDelete.CourierId);
    MessageBox.Show("Deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }

    #endregion Event Handlers

}