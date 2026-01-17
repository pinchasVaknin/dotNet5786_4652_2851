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
/// Interaction logic for LabeledDatePicker.xaml
/// </summary>
public partial class LabeledDatePicker : UserControl
{

    public LabeledDatePicker()
    {
        InitializeComponent();
    }

    //=============== Style of the Control ===============\\

    #region Label
    public string Label
    {
        get { return (string)GetValue(LabelProperty); }
        set { SetValue(LabelProperty, value); }
    }
    // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register("Label", typeof(string), typeof(LabeledDatePicker), new PropertyMetadata(string.Empty));

    #endregion Label

    #region InputStyle

    public Style InputStyle
    {
        get { return (Style)GetValue(InputStyleProperty); }
        set { SetValue(InputStyleProperty, value); }
    }
    // Using a DependencyProperty as the backing store for InputStyle.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty InputStyleProperty =
        DependencyProperty.Register("InputStyle", typeof(Style), typeof(LabeledDatePicker), new PropertyMetadata(null));

    #endregion InputStyle

    //=============== Data of the Control ===============\\

    #region Date

    public DateTime? Date
    {
        get { return (DateTime?)GetValue(DateProperty); }
        set { SetValue(DateProperty, value); }
    }
    // Using a DependencyProperty as the backing store for Date.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DateProperty =
        DependencyProperty.Register("Date", typeof(DateTime?), typeof(LabeledDatePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion Date

    #region IsEnabled

    public bool IsEnabled
    {
        get { return (bool)GetValue(IsEnabledProperty); }
        set { SetValue(IsEnabledProperty, value); }
    }
    // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register("IsEnabled", typeof(bool), typeof(LabeledDatePicker), new PropertyMetadata(true));

    #endregion IsEnabled

}
