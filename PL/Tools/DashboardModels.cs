using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL.Tools;

public class DashboardItem
{
    public BO.OrderStatus MainStatus { get; set; }
    public BO.ScheduleStatus TimeStatus { get; set; }
    public int Count { get; set; }


    public string DisplayTitle => $"{MainStatus}\n{TimeStatus}";


    public string StatusColor => MainStatus == BO.OrderStatus.Supplied ? "#27AE60" :    // Green
                                 MainStatus == BO.OrderStatus.Cancelled ? "#7F8C8D" :   // Gray
                                 TimeStatus == BO.ScheduleStatus.Late ? "#E74C3C" :     // Red
                                 "#3498DB";                                             // Blue
}
