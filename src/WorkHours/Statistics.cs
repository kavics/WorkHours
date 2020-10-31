using System;
using System.Collections.Generic;
using System.Text;

namespace WorkHours
{
    public class Statistics
    {
        public static TimeSpan ExpectedWorkTimePerWorkDay { get; set; } = TimeSpan.FromHours(6.5d);
        public List<WorkDay> WorkDays { get; set; }
        public int WorkDayCount { get; private set; }

        public TimeSpan TotalExpectedWorkTime { get; private set; }
        public TimeSpan TotalWorkTime { get; private set; }
        public TimeSpan Diff { get; private set; }
        public double Rate { get; private set; }
        public TimeSpan Average => TimeSpan.FromTicks(TotalWorkTime.Ticks / WorkDayCount);

        public void Compute()
        {
            TimeSpan expected = TimeSpan.Zero;
            TimeSpan work = TimeSpan.Zero;
            foreach (var workDay in WorkDays)
            {
                if (!workDay.IsHoliday)
                {
                    expected += ExpectedWorkTimePerWorkDay;
                    WorkDayCount++;
                }
                work += workDay.WorkHours;
            }

            TotalExpectedWorkTime = expected;
            TotalWorkTime = work;
            Diff = work - expected;
            Rate = work.TotalHours / expected.TotalHours;
        }
    }
}
