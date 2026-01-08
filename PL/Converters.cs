using BlApi;
using BO;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PL.Converters;

/// <summary>
/// Converts a VehicleType Enum to a specific Brush color for UI display.
/// </summary>
public class EnumToColorConverter : IValueConverter
{

    /// <summary>
    /// Converts various status Enums to corresponding Brush colors.
    /// </summary>
    /// <param name="value">The status Enum value to convert.</param>
    /// <param name="targetType">The target type of the conversion (expected to be Brush).</param>
    /// <param name="parameter">Optional parameter for conversion (not used).</param>
    /// <param name="culture">The culture info for the conversion (not used).</param>
    /// <returns>Brush color based on the status Enum provided.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

        // Return Transparent if value is null
        if (value == null) return DependencyProperty.UnsetValue;

        string text = value.ToString();
        object enumValue = null;

        if (Enum.TryParse(typeof(ScheduleStatus), text, out var s)) enumValue = s;
        else if (Enum.TryParse(typeof(OrderStatus), text, out var o)) enumValue = o;
        else if (Enum.TryParse(typeof(VehicleType), text, out var v)) enumValue = v;
        else if (Enum.TryParse(typeof(TypeOfOrder), text, out var t)) enumValue = t;

        // Use the parsed enum value if successful, otherwise use the original value
        object finalValue = enumValue ?? value;

        // Convert ScheduleStatus to corresponding Brush color

        #region ScheduleStatus

        if (finalValue is ScheduleStatus schedule)
        {
            switch (schedule)
            {
                case ScheduleStatus.OnTime: return Brushes.DarkGreen;
                case ScheduleStatus.Late: return Brushes.Red;
                case ScheduleStatus.InRisk: return Brushes.Orange;
            }
        }

        #endregion ScheduleStatus

        // Convert OrderStatus to corresponding Brush color

        #region OrderStatus

        if (finalValue is OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Open: return Brushes.DarkGreen;
                case OrderStatus.InProgress: return Brushes.Green;
                case OrderStatus.Supplied: return Brushes.Blue;
                case OrderStatus.Cancelled: return Brushes.Red;
                case OrderStatus.Refused: return Brushes.Gray;
            }
        }

        #endregion OrderStatus

        // Convert TypeOfOrder to corresponding Brush color

        #region TypeOfOrder

        if (finalValue is TypeOfOrder typeOfOrder)
        {
            switch (typeOfOrder)
            {
                case TypeOfOrder.TV: return Brushes.Blue;
                case TypeOfOrder.Smartphone: return Brushes.Green;
                case TypeOfOrder.Laptop: return Brushes.Orange;
                case TypeOfOrder.Tablet: return new SolidColorBrush(Color.FromRgb(59, 16, 76));
                case TypeOfOrder.Camera: return Brushes.Red;
                case TypeOfOrder.Audio: return Brushes.Brown;
                case TypeOfOrder.SmartHome: return Brushes.Cyan;
                case TypeOfOrder.GamingConsole: return Brushes.Magenta;
                case TypeOfOrder.Accessory: return Brushes.Gray;
            }
        }

        #endregion TypeOfOrder

        // Convert VehicleType to corresponding Brush color

        #region VehicleType

        if (finalValue is VehicleType vehicle)
        {
            switch (vehicle)
            {
                case VehicleType.Car: return Brushes.LightBlue;
                case VehicleType.Motorcycle: return Brushes.LightGreen;
                case VehicleType.Bicycle: return Brushes.LightSalmon;
                case VehicleType.Foot: return Brushes.SlateGray;
                default: return Brushes.LightGray;
            }
        }

        #endregion VehicleType

        return DependencyProperty.UnsetValue; ;
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

