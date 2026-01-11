using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL.delivery;

public class TimelineEvent
{
    public string Time { get; set; }
    public string Date { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
}
