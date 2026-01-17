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
/// Interaction logic for LabeledTextBox.xaml
/// </summary>
public partial class LabeledTextBox : UserControl
{
    public LabeledTextBox()
    {
        InitializeComponent();

        try
        {
            InputStyle = (Style)Application.Current.FindResource("ConfigTextBoxStyle");
        }
        catch
        {
            // Ignore if the style is not found
        }
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
        DependencyProperty.Register("Label", typeof(string), typeof(LabeledTextBox), new PropertyMetadata(string.Empty));

    #endregion Label

    #region Text

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }
    // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(LabeledTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion Text

    #region InputStyle

    public Style InputStyle
    {
        get { return (Style)GetValue(InputStyleProperty); }
        set { SetValue(InputStyleProperty, value); }
    }
    // Using a DependencyProperty as the backing store for InputStyle.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty InputStyleProperty =
        DependencyProperty.Register("InputStyle", typeof(Style), typeof(LabeledTextBox), new PropertyMetadata(null));

    #endregion InputStyle

    #region TextForeground

    public Brush TextForeground
    {
        get { return (Brush)GetValue(TextForegroundProperty); }
        set { SetValue(TextForegroundProperty, value); }
    }
    // Using a DependencyProperty as the backing store for TextForeground.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TextForegroundProperty =
        DependencyProperty.Register("TextForeground", typeof(Brush), typeof(LabeledTextBox), new PropertyMetadata(Brushes.Black));

    #endregion TextForeground

    //=============== Data of the Control ===============\\

    #region IsReadOnly

    public bool IsReadOnly
    {
        get { return (bool)GetValue(IsReadOnlyProperty); }
        set { SetValue(IsReadOnlyProperty, value); }
    }
    // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(LabeledTextBox), new PropertyMetadata(false));

    #endregion IsReadOnly

}


