using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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

    /// <summary>
    /// Retrieves all values of the specified enumeration type, excluding any specified values.
    /// </summary>
    /// <remarks>This method returns all values of the enumeration type <typeparamref name="T"/> as defined in
    /// the enumeration,  except for those explicitly provided in the <paramref name="exclude"/> parameter.  The order
    /// of the returned values matches the order in which they are defined in the enumeration.</remarks>
    /// <typeparam name="T">The enumeration type to retrieve values from. Must be a valid enumeration.</typeparam>
    /// <param name="exclude">An optional array of enumeration values to exclude from the result.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all values of the enumeration type <typeparamref name="T"/>,
    /// excluding the specified values.</returns>
    public static IEnumerable<T> GetEnumValues<T>(params T[] exclude) where T : Enum
    {
        return Enum.GetValues(typeof(T))
                   .Cast<T>()
                   .Where(item => !exclude.Contains(item));
    }

    /// <summary>
    /// Handles the custom close button (X) click.
    /// </summary>
    public void BtnCloseProgram_Global_Click(object sender, RoutedEventArgs e)
    {
        // Closes the entire application
        Application.Current.Shutdown();
    }

    /// <summary>
    /// Handles the custom close button (X) click.
    /// </summary>
    public void BtnCloseWindow_Global_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            // Find the window that contains this button
            Window parentWindow = Window.GetWindow(btn);

            // Close that specific window safely
            parentWindow?.Close();
        }
    }


}
