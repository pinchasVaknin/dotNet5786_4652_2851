using System;
using System.Collections;
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
/// Interaction logic for LabeledComboBox.xaml
/// </summary>
public partial class LabeledComboBox : UserControl
{
    public LabeledComboBox()
    {
        InitializeComponent();

        try 
        { 
            InputStyle = (Style)Application.Current.FindResource("ConfigComboBoxStyle"); 
        } 
        catch 
        {
            // Ignore if the style is not found
        }
    }

    public string Label
    {
        get { return (string)GetValue(LabelProperty); }
        set { SetValue(LabelProperty, value); }
    }
    // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register("Label", typeof(string), typeof(LabeledComboBox), new PropertyMetadata(string.Empty));

    public IEnumerable ItemsSource
    {
        get { return (IEnumerable)GetValue(ItemsSourceProperty); }
        set { SetValue(ItemsSourceProperty, value); }
    }
    // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(LabeledComboBox), new PropertyMetadata(null));

    public object SelectedItem
    {
        get { return (object)GetValue(SelectedItemProperty); }
        set { SetValue(SelectedItemProperty, value); }
    }
    // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register("SelectedItem", typeof(object), typeof(LabeledComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public Style InputStyle
    {
        get { return (Style)GetValue(InputStyleProperty); }
        set { SetValue(InputStyleProperty, value); }
    }
    // Using a DependencyProperty as the backing store for InputStyle.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty InputStyleProperty =
        DependencyProperty.Register("InputStyle", typeof(Style), typeof(LabeledComboBox), new PropertyMetadata(null));

    public bool IsReadOnly
    {
        get { return (bool)GetValue(IsReadOnlyProperty); }
        set { SetValue(IsReadOnlyProperty, value); }
    }
    // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(LabeledComboBox),
            new PropertyMetadata(false, OnIsReadOnlyChanged));

    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // When IsReadOnly changes, update the IsEnabled property of the inner ComboBox
        var control = d as LabeledComboBox;
        // If IsReadOnly is true, disable the ComboBox; if false, enable it
        if (control != null)
            control.cmbInner.IsEnabled = !(bool)e.NewValue;
    }
}
