using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace WorkHours
{
    [DebuggerDisplay("{ToString()}")]
    public class WorkDay
    {
        public DateTime Date { get; set; }
        public bool IsHoliday { get; set; }
        public TimeSpan WorkHours { get; set; }

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd} {WorkHours.TotalHours:##.00} {(IsHoliday ? "Holiday" : "")}";
        }
    }
}
