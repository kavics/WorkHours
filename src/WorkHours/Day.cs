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

        internal static Day Parse(string src)
        {
            var fields = src.Split('\t');

            if (!DateTime.TryParse(fields[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                return null;

            if (!Enum.TryParse<DayType>(fields[1], out var type))
                type = DayType.WorkDay;

            return new Day() { Date = time, Type = type };
        }

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd}: {Type}";
        }

    }
}
