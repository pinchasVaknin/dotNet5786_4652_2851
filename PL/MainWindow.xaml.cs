using PL.Courier;
using PL.Order;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));


        public BO.Config Configuration
        {
            get { return (BO.Config)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }
        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(BO.Config), typeof(MainWindow));

        private void ClockObserver()
        {
            CurrentTime = s_bl.Admin.GetClock();
        }

        private void ConfigObserver()
        {
            Configuration = s_bl.Admin.GetConfig();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            CurrentTime = s_bl.Admin.GetClock();

            Configuration = s_bl.Admin.GetConfig();

            s_bl.Admin.AddClockObserver(ClockObserver);

            s_bl.Admin.AddConfigObserver(ConfigObserver);
        }

        private void Window_Closed(object sender, EventArgs e)
        {             
            s_bl.Admin.RemoveClockObserver(ClockObserver);
            s_bl.Admin.RemoveConfigObserver(ConfigObserver);
        }


        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnAddMinute_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Minute);
        }

        private void BtnAddHour_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Hour);
        }

        private void BtnAddDay_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Day);
        }

        private void BtnAddYear_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Year);
        }

        private void BtnInitDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to initialize the Database ? This might take a moment.", "Initialize Database", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // Show wait cursor
                    Mouse.OverrideCursor = Cursors.Wait;

                    // Initialize the database
                    s_bl.Admin.InitializeDB();

                    // Refresh properties
                    CurrentTime = s_bl.Admin.GetClock();
                    Configuration = s_bl.Admin.GetConfig();

                    // Show success message
                    MessageBox.Show("Database initialized successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error initializing DB: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // Restore default cursor
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void BtnResetDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset the Database ? This will erase all data.", "Reset Database", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // Show wait cursor
                    Mouse.OverrideCursor = Cursors.Wait;

                    // Reset the database
                    s_bl.Admin.ResetDB();

                    // Refresh properties
                    CurrentTime = s_bl.Admin.GetClock();
                    Configuration = s_bl.Admin.GetConfig();

                    // Show success message
                    MessageBox.Show("Database reset successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error resetting DB: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // Restore default cursor
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void BtnUpdateConfig_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.SetConfig(Configuration);
        }

        private void BtnShowCouriers_Click(object sender, RoutedEventArgs e)
        {
            new CourierListWindow().Show(); 
        }

        private void BtnShowOrders_Click(object sender, RoutedEventArgs e)
        {
            new OrderListWindow().Show();
        }
    }
}