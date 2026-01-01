using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PL;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    //==================== Methods ===================\\
    /// <summary>
    /// Validates whether the input text consists only of numeric characters.
    /// </summary>
    /// <remarks>This method uses a regular expression to determine if the input text contains non-numeric
    /// characters. If the input is not numeric, the event is marked as handled, preventing further
    /// processing.</remarks>
    /// <param name="sender">The source of the event. Typically, the control that raised the event.</param>
    /// <param name="e">The <see cref="TextCompositionEventArgs"/> containing the text input to validate.</param>
    public void CheckIfNumber(object sender, TextCompositionEventArgs e)
    {
        // Regular expression to match any non-numeric characters
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }

    public static IEnumerable<T> GetEnumValues<T>(params T[] exclude) where T : Enum
    {
        return Enum.GetValues(typeof(T))
                   .Cast<T>()
                   .Where(item => !exclude.Contains(item));
    }
}
