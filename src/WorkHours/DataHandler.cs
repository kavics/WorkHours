using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WorkHours
{
    internal class DataHandler
    {
        /// <summary>
        /// Returns last "start" time that is not followed by "stop" time.
        /// Returns DateTime.MinValue if the last time is "stop".
        /// </summary>
        public DateTime GetWorkStart()
        {
            var events = GetEvents();

            var lastStart = events.LastOrDefault(x => x.Type == EventType.Start);
            if (lastStart == null)
                return DateTime.MinValue;

            var lastStop = events.LastOrDefault(x => x.Type == EventType.Stop);
            if (lastStop == null)
                return lastStart.Time;

            if(lastStart.Time > lastStop.Time)
                return lastStart.Time;

            return DateTime.MinValue;
        }

        /// <summary>
        /// Returns the daily work time.
        /// </summary>
        public TimeSpan GetWorkHours()
        {
            var events = GetEvents();

            var now = DateTime.Now;
            var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

            var todayEvents = events.Where(x => x.Time >= today).ToArray();

            var time = TimeSpan.Zero;

            var pairs = new List<Tuple<Event, Event>>();
            DateTime currentStartTime = DateTime.MinValue;
            foreach (var e in todayEvents)
            {
                if(e.Type == EventType.Start)
                {
                    if (currentStartTime == DateTime.MinValue)
                        currentStartTime = e.Time;
                }
                else if(e.Type == EventType.Stop)
                {
                    if (currentStartTime != DateTime.MinValue)
                    {
                        time += e.Time - currentStartTime;
                        currentStartTime = DateTime.MinValue;
                    }
                }
            }

            if (currentStartTime != DateTime.MinValue)
                time += now - currentStartTime;

            return time;
        }

        /// <summary>
        /// Writes day transition events
        /// </summary>
        public void LogDayTransition(DateTime stopTime, DateTime startTime)
        {
            AppendEvents(
                Event.Create(EventType.Stop, stopTime),
                Event.Create(EventType.Start, startTime)
            );
        }

        /// <summary>
        /// Writes a "start" record
        /// </summary>
        public void LogStart(DateTime? time = null)
        {
            AppendEvents(Event.Create(EventType.Start, time));
        }

        /// <summary>
        /// Writes a "stop" record
        /// </summary>
        public void LogStop(DateTime? time = null)
        {
            AppendEvents(Event.Create(EventType.Stop, time));
        }

        public Statistics GetStatistics()
        {
            var today = DateTime.Now.Date;
            var days = new List<WorkDay>();
            WorkDay day = null;
            var start = DateTime.MinValue;
            var allEvents = GetEvents();
            foreach (var @event in allEvents)
            {
                var eventDate = @event.Time.Date;
                //if (eventDate == today)
                //    break;

                if (day == null || eventDate > day.Date)
                    days.Add(day = new WorkDay {Date = eventDate, IsHoliday = IsHoliday(@event.Time)});

                if (@event.Type == EventType.Start)
                    start = @event.Time;
                if (@event.Type == EventType.Stop)
                    day.WorkHours+= (@event.Time-start);
            }

            // Today time correction if working.
            var lastEvent = allEvents[^1];
            if (lastEvent.Type == EventType.Start)
            {
                var lastDay = days[^1];
                lastDay.WorkHours += DateTime.Now - lastEvent.Time;
            }

            var statistics = new Statistics {WorkDays = days};
            statistics.Compute();
            return statistics;
        }

        /* ========================================================================= READERS AND WRITERS */

        private void AppendEvents(params Event[] events)
        {
            WriteToFile(writer =>
            {
                foreach (var @event in events)
                    AppendEvent(writer, @event);
            });
        }

        private void AppendEvent(TextWriter writer, Event @event)
        {
            writer.WriteLine($"{@event.Time:yyyy-MM-dd HH:mm:ss.fffff}\t{@event.Type}");
            AddEvent(@event);
        }

        private List<Event> __events;
        private  List<Event> GetEvents()
        {
            List<Event> LoadEvents()
            {
                var result = new List<Event>();

                LoadFromFile(GetLogFilePath(), reader =>
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                        result.Add(Event.Parse(line));
                });

                return result;
            }

            if (__events == null)
                __events = LoadEvents();
            return __events;
        }
        private void AddEvent(Event @event)
        {
            if (__events != null)
                GetEvents().Add(@event);
        }

        /* ================================================================================ HOLIDAYS & DAY TYPE */

        public DayType GetDefaultDayType(DateTime? dateTime = null)
        {
            var date = dateTime ?? DateTime.Now;
            var isWorkday = date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
            return isWorkday ? DayType.WorkDay : DayType.Holiday;
        }

        private List<Day> __days;
        private List<Day> GetDays()
        {
            List<Day> LoadDays()
            {
                var result = new List<Day>();

                LoadFromFile(GetHolidayFilePath(), reader =>
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                        result.Add(Day.Parse(line));
                });

                return result;
            }

            if (__days == null)
                __days = LoadDays();
            return __days;
        }

        public bool IsHoliday(DateTime? dateTime = null)
        {
            var date = (dateTime ?? DateTime.Now).Date;
            var type = GetDays().FirstOrDefault(x=>x.Date == date)?.Type ?? GetDefaultDayType(date);
            return type == DayType.Holiday;
        }

        /* ================================================================================ FILE HANDLER */

        private void LoadFromFile(string path, Action<TextReader> readerCallback)
        {
            using (var writer = new StreamReader(path, true))
            {
                readerCallback(writer);
            }
        }

        private void WriteToFile(Action<TextWriter> writeCallback)
        {
            using(var writer = new StreamWriter(GetLogFilePath(), true))
            {
                writeCallback(writer);
            }
        }

        private string _workDirectoryPath;

        private string GetWorkDirectoryPath()
        {
            if (_workDirectoryPath == null)
            {
                var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                _workDirectoryPath = dir;
            }
            return _workDirectoryPath;
        }
        internal string GetLogFilePath()
        {
            var path = Path.Combine(GetWorkDirectoryPath(), "current.log");
            if (!File.Exists(path))
                using (var writer = File.AppendText(path))
                    AppendEvent(writer, Event.Create(EventType.FileCreated));
            return path;
        }
        internal string GetHolidayFilePath()
        {
            var path = Path.Combine(GetWorkDirectoryPath(), "holidays.txt");
            if (!File.Exists(path))
                using (var writer = File.AppendText(path))
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd}\t{GetDefaultDayType()}");
            return path;
        }
    }
}
