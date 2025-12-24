using BlApi;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
namespace PL;

/// <summary>
/// Converts a VehicleType Enum to a specific Brush color for UI display.
/// </summary>
public class ConvertVehicleTypeToColor : IValueConverter
{
    /// <summary>
    /// Converts the value from the source (VehicleType) to the target (Brush).
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A Brush color corresponding to the vehicle type.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BO.VehicleType type)
        {
            switch (type)
            {
                case BO.VehicleType.Motorcycle:
                    return Brushes.LightGreen;
                case BO.VehicleType.Car:
                    return Brushes.LightBlue;
                case BO.VehicleType.Bicycle:
                    return Brushes.LightSalmon;
                default:
                    return Brushes.LightGray;
            }
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts Courier state to a Brush color for the Delete button.
/// If the courier has history or active orders, returns Gray (disabled look).
/// Otherwise, returns Red.
/// </summary>
public class ConvertDeleteToColor : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        ConvertDeleteToEnabled check = new ConvertDeleteToEnabled();

        bool isEnabled = (bool)check.Convert(value, targetType, parameter, culture);

        return isEnabled ? Brushes.Red : Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts Courier object to boolean.
/// Returns FALSE if courier has history/active orders (disable button).
/// Returns TRUE otherwise (enable button).
/// </summary>
public class ConvertDeleteToEnabled : IValueConverter
{
    private static readonly IBl s_bl = Factory.Get();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BO.CourierInList courier)
        {
            try
            {
                return s_bl.Courier.IsCourierDeletable(courier.CourierId);
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
