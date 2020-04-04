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
        public static DateTime GetWorkStart()
        {
            var lastStart = GetEvents().LastOrDefault(x => x.Type == EventType.Start);
            if (lastStart == null)
                return DateTime.MinValue;

            var lastStop = GetEvents().LastOrDefault(x => x.Type == EventType.Stop);
            if (lastStop == null)
                return lastStart.Time;

            if(lastStart.Time > lastStop.Time)
                return lastStart.Time;

            return DateTime.MinValue;
        }

        /// <summary>
        /// Returns the daily work time.
        /// </summary>
        public static TimeSpan GetWorkHours()
        {
            var now = DateTime.Now;
            var today = new DateTime(now.Year, now.Day, now.Month, 0, 0, 0);

            var todayEvents = GetEvents().Where(x => x.Time >= today).ToArray();

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
        public static void LogDayTransition(DateTime stopTime, DateTime startTime)
        {
            AppendEvents(
                Event.Create(EventType.Stop, stopTime),
                Event.Create(EventType.Start, startTime)
            );
        }

        /// <summary>
        /// Writes a "start" record
        /// </summary>
        public static void LogStart(DateTime? time = null)
        {
            AppendEvents(Event.Create(EventType.Start, time));
        }

        /// <summary>
        /// Writes a "stop" record
        /// </summary>
        public static void LogStop(DateTime? time = null)
        {
            AppendEvents(Event.Create(EventType.Stop, time));
        }

        /* ========================================================================= READERS AND WRITERS */

        private static void AppendEvents(params Event[] events)
        {
            WriteToFile(writer =>
            {
                foreach (var @event in events)
                    AppendEvent(writer, @event);
            });
        }

        private static void AppendEvent(TextWriter writer, Event @event)
        {
            writer.WriteLine($"{@event.Time:yyyy-MM-dd HH:mm:ss.fffff}\t{@event.Type}");
            AddEvent(@event);
        }

        private static List<Event> __events;
        private static  List<Event> GetEvents()
        {
            List<Event> LoadEvents()
            {
                var result = new List<Event>();

                LoadFromFile(reader =>
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
        private static void AddEvent(Event @event)
        {
            if (__events != null)
                GetEvents().Add(@event);
        }

        /* ================================================================================ FILE HANDLER */

        private static void LoadFromFile(Action<TextReader> readerCallback)
        {
            using (var writer = new StreamReader(GetFilePath(), true))
            {
                readerCallback(writer);
            }
        }
        private static void WriteToFile(Action<TextWriter> writeCallback)
        {
            using(var writer = new StreamWriter(GetFilePath(), true))
            {
                writeCallback(writer);
            }
        }

        private static string _filePath;
        private static string GetFilePath()
        {
            if (_filePath == null)
            {
                var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var path = Path.Combine(dir, "current.log");
                if (!File.Exists(path))
                    using (var writer = File.AppendText(path))
                        AppendEvent(writer, Event.Create(EventType.FileCreated));

                _filePath = path;
            }
            return _filePath;
        }

    }
}
