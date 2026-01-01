using BO;
using PL;
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

namespace PL.Login;

/// <summary>
/// Interaction logic for LoginWindow.xaml
/// </summary>
public partial class LoginWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    //public BO.Config CurrentConfig
    //{
    //    get { return (BO.Config)GetValue(CurrentConfigProperty); }
    //    set { SetValue(CurrentConfigProperty, value); }
    //}
    //// registering the CurrentConfig dependency property
    //public static readonly DependencyProperty CurrentConfigProperty =
    //    DependencyProperty.Register("CurrentConfig", typeof(BO.Config), typeof(LoginWindow), new PropertyMetadata(null));













    public LoginWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        int userId = int.Parse (UserId.Text);
        string password = Password.Text;

        try
        {
            UserRole userRole = s_bl.Courier.Login(userId, password);
            if (userRole == UserRole.Admin)
            {
                new MainWindow().Show();
                Close();
            }
            else if (userRole == UserRole.Courier)
            {
                new MainWindow().Show();
                Close();
            }
            else
            {
                throw new Exception("bad");
            }
        }
        catch (Exception ex) 
        { 
            MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
