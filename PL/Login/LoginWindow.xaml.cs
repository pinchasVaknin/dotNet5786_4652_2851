using PL.Courier;
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
    // Access to the Business Logic layer
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public LoginWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Validates input and attempts to log in via BL.
    /// </summary>
    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        try
        {

            // Show wait cursor
            Mouse.OverrideCursor = Cursors.Wait;

            // Input Validation
            if (string.IsNullOrWhiteSpace(txtUserId.Text))
            {
                MessageBox.Show("Please enter User ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtUserId.Text, out int userId))
            {
                MessageBox.Show("User ID must be a number.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string password = txtPassword.Password;

            // BL Login Call
            BO.UserRole role = s_bl.Courier.Login(userId, password);

            // Navigation Switch
            switch (role)
            {
                case BO.UserRole.Admin:
                    new MainWindow().Show(); // Opens Admin Dashboard
                    break;

                case BO.UserRole.Courier:
                    new CourierDirectWindow(userId).Show(); // Opens Courier Dashboard with ID
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
            MessageBox.Show($"Login Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            // Restore default cursor
            Mouse.OverrideCursor = null;
        }
    }
}

/*
 <Window x:Class="PL.Login.LoginWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Delivery System - Login" 
        Height="450" Width="800"
        WindowStartupLocation="CenterScreen" 
        ResizeMode="NoResize"
        WindowStyle="None"
        AllowsTransparency="True"
        Background="Transparent">

    <Border CornerRadius="20" Background="White">
        <Border.Effect>
            <DropShadowEffect BlurRadius="20" ShadowDepth="0" Opacity="0.4" Color="Black"/>
        </Border.Effect>
        <Border.Clip>
            <RectangleGeometry Rect="0,0,800,450" RadiusX="20" RadiusY="20"/>
        </Border.Clip>

        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="300"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>

            <Border Grid.Column="0">
                <Border.Background>
                    <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                        <GradientStop Color="#2C3E50" Offset="0.0"/>
                        <GradientStop Color="#34495E" Offset="1.0"/>
                    </LinearGradientBrush>
                </Border.Background>

                <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
                    <Path Data="M12,4A4,4 0 0,1 16,8A4,4 0 0,1 12,12A4,4 0 0,1 8,8A4,4 0 0,1 12,4M12,14C16.42,14 20,15.79 20,18V20H4V18C4,15.79 7.58,14 12,14Z" 
                          Fill="White" Stretch="Uniform" Width="80" Height="80" Margin="0,0,0,20">
                        <Path.Effect>
                            <DropShadowEffect BlurRadius="10" Color="Black" Opacity="0.3" ShadowDepth="3"/>
                        </Path.Effect>
                    </Path>

                    <Label Content="Delivery System" Foreground="White" 
                           FontSize="28" FontWeight="Bold" FontFamily="Segoe UI"
                           HorizontalAlignment="Center"/>

                    <TextBlock Text="Management Console" Foreground="#BDC3C7" 
                               FontSize="16" HorizontalAlignment="Center" Margin="0,5,0,0" 
                               FontStyle="Italic"/>
                </StackPanel>
            </Border>

            <Grid Grid.Column="1" Background="White">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>

                <Button Content="✕" HorizontalAlignment="Right" Margin="15" 
                        Width="30" Height="30" Background="Transparent" BorderThickness="0" 
                        FontSize="16" Foreground="#95A5A6" Cursor="Hand"
                        Click="BtnClose_Click">
                    <Button.Style>
                        <Style TargetType="Button">
                            <Setter Property="Template">
                                <Setter.Value>
                                    <ControlTemplate TargetType="Button">
                                        <Border Background="{TemplateBinding Background}" CornerRadius="15">
                                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                        </Border>
                                    </ControlTemplate>
                                </Setter.Value>
                            </Setter>
                            <Style.Triggers>
                                <Trigger Property="IsMouseOver" Value="True">
                                    <Setter Property="Background" Value="#ECF0F1"/>
                                    <Setter Property="Foreground" Value="#E74C3C"/>
                                </Trigger>
                            </Style.Triggers>
                        </Style>
                    </Button.Style>
                </Button>

                <StackPanel Grid.Row="1" VerticalAlignment="Center" Margin="60,0">

                    <TextBlock Text="Welcome" FontSize="36" FontWeight="ExtraBold" 
                               Foreground="{StaticResource DarkBlueBrush}" 
                               FontFamily="Elephant" Margin="0,0,0,10">
                        <TextBlock.Effect>
                            <DropShadowEffect BlurRadius="2" Color="LightGray" ShadowDepth="2" Opacity="0.5"/>
                        </TextBlock.Effect>
                    </TextBlock>

                    <TextBlock Text="Please sign in to continue" Foreground="Gray" FontSize="14" Margin="0,0,0,30"/>

                    <Label Content="USER ID" FontSize="12" FontWeight="Bold" Foreground="#7F8C8D"/>
                    <TextBox x:Name="txtUserId" Height="40" FontSize="16" VerticalContentAlignment="Center" 
                             Style="{StaticResource NumericStyle}" BorderBrush="#BDC3C7" BorderThickness="0,0,0,2" Background="Transparent" Padding="2"/>

                    <Label Content="PASSWORD" FontSize="12" FontWeight="Bold" Foreground="#7F8C8D" Margin="0,20,0,0"/>
                    <PasswordBox x:Name="txtPassword" Height="40" FontSize="16" VerticalContentAlignment="Center" 
                                 BorderBrush="#BDC3C7" BorderThickness="0,0,0,2" Background="Transparent" Padding="2"/>

                    <Button x:Name="btnLogin" Content="LOGIN" Height="50" Margin="0,40,0,0" 
                            FontSize="18" FontWeight="Bold" Click="BtnLogin_Click"
                            Style="{StaticResource ActionButtonStyle}">
                        <Button.Effect>
                            <DropShadowEffect BlurRadius="10" Color="#27AE60" Opacity="0.4" ShadowDepth="3"/>
                        </Button.Effect>
                    </Button>

                </StackPanel>
            </Grid>
        </Grid>
    </Border>
</Window>
 */
