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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PL.Controls;

/// <summary>
/// Interaction logic for TopBarControl.xaml
/// </summary>
public partial class TopBarControl : UserControl
{

    //==================== Constructor ===================\\

    #region Constructor

    public TopBarControl()
    {
        InitializeComponent();
    }

    #endregion Constructor

    //==================== Dependency Properties ===================\\

    #region Dependency Properties

    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }
    // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(TopBarControl), new PropertyMetadata(""));

    public bool IsExitProgram
    {
        get { return (bool)GetValue(IsExitProgramProperty); }
        set { SetValue(IsExitProgramProperty, value); }
    }
    // Using a DependencyProperty as the backing store for IsExitProgram.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsExitProgramProperty =
        DependencyProperty.Register("IsExitProgram", typeof(bool), typeof(TopBarControl), new PropertyMetadata(false));

    public Visibility BackButtonVisibility
    {
        get { return (Visibility)GetValue(BackButtonVisibilityProperty); }
        set { SetValue(BackButtonVisibilityProperty, value); }
    }
    // Using a DependencyProperty as the backing store for BackButtonVisibility.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BackButtonVisibilityProperty =
        DependencyProperty.Register("BackButtonVisibility", typeof(Visibility), typeof(TopBarControl), new PropertyMetadata(Visibility.Visible));

    #endregion Dependency Properties

    //==================== Methods ===================\\

    #region Methods

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        if (IsExitProgram)
        {
            Application.Current.Shutdown();
        }
        else
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        MainWindow? mainWindow = App.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        if (mainWindow == null || !mainWindow.IsLoaded)
        {
            mainWindow = new MainWindow();
        }
        else
        {
            if (mainWindow.WindowState == WindowState.Minimized)
                mainWindow.WindowState = WindowState.Normal;
        }

        mainWindow.Show();
        mainWindow.Activate();

        Window parentWindow = Window.GetWindow(this);
        if (parentWindow != mainWindow)
        {
            parentWindow?.Close();
        }
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Window parentWindow = Window.GetWindow(this);

            parentWindow?.DragMove();
        }
    }

    #endregion Methods

}

