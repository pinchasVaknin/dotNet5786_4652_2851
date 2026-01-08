using PL.Order;
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

namespace PL.Order
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {

        //==================== Fields ===================\\

        #region Fields

        // The entry point to the BL layer (Factory pattern).
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        #endregion Fields

        //==================== Properties ===================\\

        #region Properties

        /// <summary>
        /// Gets or sets the current order associated with the application.
        /// </summary>
        public BO.Order CurrentOrder
        {
            get { return (BO.Order)GetValue(CurrentOrderProperty); }
            set { SetValue(CurrentOrderProperty, value); }
        }
        // registering the CurrentOrder dependency property
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
        // registering the ActionButtonText dependency property
        public static readonly DependencyProperty ActionButtonTextProperty =
            DependencyProperty.Register("ActionButtonText", typeof(string), typeof(OrderWindow), new PropertyMetadata("Add"));

        #endregion Properties

        //================== Constructors =================\\

        #region Constructors

        public OrderWindow()
        {
            InitializeComponent();
        }

        public OrderWindow(int orderId)
        {
            InitializeComponent();
        }

        #endregion Constructors
    }


}
