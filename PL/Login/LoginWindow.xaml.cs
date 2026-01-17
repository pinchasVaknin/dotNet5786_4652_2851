using PL.Courier;
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

namespace PL.Login;

/// <summary>
/// Interaction logic for LoginWindow.xaml
/// Handles authentication and navigation.
/// </summary>
public partial class LoginWindow : Window
{

    //==================== Fields ===================\\

    #region Fields

    // Access to the Business Logic layer
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    #endregion Fields

    //==================== Constructor ===================\\

    #region Constructor

    public LoginWindow()
    {
        InitializeComponent();
    }

    #endregion Constructor

    //==================== Event Handlers ===================\\

    #region Event Handlers

    /// <summary>
    /// Validates input and attempts to log in via BL.
    /// </summary>
    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {

        ResetErrors();

        try
        {
            bool isValid = true;
            int userId = 0;

            // Show wait cursor
            Mouse.OverrideCursor = Cursors.Wait;

            // Input Validation
            if (string.IsNullOrWhiteSpace(txtUserId.Text))
            {
                ShowError(ErrId, "User ID is required.");
                isValid = false;
                return;
            }

            if (!int.TryParse(txtUserId.Text, out userId))
            {
                ShowError(ErrId, "User ID must be a number.");
                isValid = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                ShowError(ErrPass, "Password is required.");
                isValid = false;
                return;
            }

            string password = txtPassword.Password;

            // BL Login Call
            BO.UserRole role = s_bl.Courier.Login(userId, password);

            UserData.s_UserId = userId;

            // Navigation Switch
            switch (role)
            {
                case BO.UserRole.Admin:
                    new MainWindow().Show(); // Opens Admin Dashboard
                    break;

                case BO.UserRole.Courier:
                    new CourierDirectWindow(UserData.s_UserId).Show(); // Opens Courier Dashboard with ID
                    break;

                default:
                    throw new Exception("Unknown user role.");
            }

            // Close Login Window
            this.Close();
        }
        catch (Exception ex)
        {
            // Handles "User not found" or "Wrong password" from BL
            ShowError(ErrPass, ex.Message);
        }
        finally
        {
            // Restore default cursor
            Mouse.OverrideCursor = null;
        }
    }

    private void HandleEnterKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Mark event as handled to prevent default behavior
            e.Handled = true;

            // If Enter is pressed in the password box, trigger login
            if (sender == txtPassword)
            {
                BtnLogin_Click(sender, e);
            }
            else
            {
                // Move focus to the next control
                TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                (sender as UIElement)?.MoveFocus(request);
            }
        }
    }

    private void BtnExit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    #endregion Event Handlers

    //==================== Methods ===================\\

    #region Methods

    private void ShowError(TextBlock errBlock, string msg)
    {
        errBlock.Text = msg;
        errBlock.Visibility = Visibility.Visible;
    }

    private void ResetErrors()
    {
        ErrId.Visibility = Visibility.Collapsed;
        ErrPass.Visibility = Visibility.Collapsed;
    }

    private void ClearError_TextChanged(object sender, TextChangedEventArgs e)
    {
        ErrId.Visibility = Visibility.Collapsed;
    }

    private void ClearError_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ErrPass.Visibility = Visibility.Collapsed;
    }

    #endregion Methods

}