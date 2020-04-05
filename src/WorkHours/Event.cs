using System;
using System.Diagnostics;
using System.Globalization;

namespace WorkHours
{
    internal enum EventType
    {
        Unknown, FileCreated, Start, Stop
    }

    [DebuggerDisplay("{ToString()}")]
    internal class Event
    {
        public DateTime Time { get; set; }
        public EventType Type { get; set; }

        internal static Event Create(EventType type, DateTime? time = null)
        {
            return new Event
            {
                Time = time == null ? DateTime.Now : time.Value,
                Type = type
            };
        }

        internal static Event Parse(string src)
        {
            var fields = src.Split('\t');

            if (!DateTime.TryParse(fields[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                return null;

            if (!Enum.TryParse<EventType>(fields[1], out var type))
                type = EventType.Unknown;

            return new Event { Time = time, Type = type };
        }

        public override string ToString()
        {
            return $"{Time:yyyy-MM-dd HH:mm:ss}: {Type}";
        }
    }
}
