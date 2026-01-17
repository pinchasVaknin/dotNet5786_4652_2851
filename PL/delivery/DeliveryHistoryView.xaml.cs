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

namespace PL.delivery;

/// <summary>
/// Interaction logic for DeliveryHistoryView.xaml
/// </summary>
public partial class DeliveryHistoryView : Window
{

    //==================== Constructor ===================\\

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the window with the provided history list.
    /// </summary>
    /// <param name="historyList">The list of delivery steps from BO.</param>
    public DeliveryHistoryView(IEnumerable<BO.DeliveryPerOrderInList>? historyList)
    {

        // Restore default cursor
        Mouse.OverrideCursor = null;

        InitializeComponent();
        LoadTimeline(historyList);
    }

    #endregion Constructor

    //==================== Timeline Loading ===================\\

    #region Timeline Loading

    /// <summary>
    /// Converts the BO list into UI-friendly TimelineEvent objects and binds them.
    /// </summary>
    private void LoadTimeline(IEnumerable<BO.DeliveryPerOrderInList>? historyList)
    {
        // Convert BO list to TimelineEvent list
        List<TimelineEvent> timelineEvents = new List<TimelineEvent>();

        // Populate the timeline events
        if (historyList != null)
        {
            foreach (var item in historyList)
            {
                // Determine if the delivery is finished
                bool isFinished = item.DeliveryFinishType != null;

                // Add the timeline event
                timelineEvents.Add(new TimelineEvent
                {
                    // Format time and date
                    Time = item.StartDeliveryDate.ToString("HH:mm") ?? "00:00",
                    Date = item.StartDeliveryDate.ToString("dd/MM/yyyy") ?? "N/A",

                    // Set status based on delivery finish type
                    Status = item.DeliveryFinishType?.ToString() ?? "In Transit",

                    // Description with courier info
                    Description = $"Courier: {item.CourierFullName ?? "Assigned"}",

                    // Mark as completed if finished
                    IsCompleted = isFinished
                });
            }
        }
        else
        {
            // No history available
            timelineEvents.Add(new TimelineEvent
            {
                Status = "No History",
                Description = "No delivery records found.",
                IsCompleted = false
            });
        }

        // Bind the timeline events to the UI
        TimelineList.ItemsSource = timelineEvents;
    }

    #endregion Timeline Loading

    //==================== Methods ===================\\

    #region Methods

    /// <summary>
    /// Closes the current window.
    /// </summary>
    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    #endregion Methods

}
