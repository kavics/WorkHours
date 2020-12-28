using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace WorkHours
{
    public enum DayType { WorkDay, Holiday }

    public class Day
    {
        public DateTime Date { get; set; }
        public DayType Type { get; set; }
        public string Description { get; set; }

        internal static Day Parse(string src)
        {
            var fields = src.Split('\t');

            if (!DateTime.TryParse(fields[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                return null;

            var desc = fields[1];
            if (!Enum.TryParse<DayType>(desc, true, out var type))
                type = DayType.Holiday;

            return new Day { Date = time, Type = type, Description = desc };
        }

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd}: {Type}";
        }

    }
}
