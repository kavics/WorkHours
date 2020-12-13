using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WorkHours
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            StopButton.Visibility = Visibility.Hidden;
            PlayButton.Visibility = Visibility.Visible;

            RefreshStatistics();

            _playPressedTime = DataHandler.GetWorkStart();
            _workTime = DataHandler.GetWorkHours();
            _lastTick = DateTime.Now;
            _isHoliday = DataHandler.IsHoliday();
            DayTypeLabel.Content = _isHoliday ? "Holiday" : "WorkDay";

            if (_playPressedTime != DateTime.MinValue)
                StopButton_Click(null, null);

            SetDateLabel();
            if (_playPressedTime != DateTime.MinValue)
                SetPlayGui();

            WorkDayProgress.Minimum = 0;
            WorkDayProgress.Maximum = 8 * 60 * 60;

            SetWorkHoursLabel(_workTime);
            SetDateLabel();

            _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(0.5);
            _dispatcherTimer.Start();

        }

        private string StatisticsToString(Statistics stat)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"STATISTICS");
            sb.AppendLine($"----------");
            sb.AppendLine();
            sb.AppendLine($"WORK TIME PER WORKDAY");
            sb.AppendLine();
            sb.AppendLine($"Expectation        {Statistics.ExpectedWorkTimePerWorkDay:hh':'mm':'ss}");
            sb.AppendLine($"Average:           {stat.Average:hh':'mm':'ss}");
            sb.AppendLine($"Rate:              {stat.Rate:F3}");
            sb.AppendLine();
            sb.AppendLine($"WORK TIME SUMMARY");
            sb.AppendLine();
            sb.AppendLine($"Total work time:   {stat.TotalWorkTime:d'.'hh':'mm':'ss}");
            sb.AppendLine($"Expectation:       {stat.TotalExpectedWorkTime}");
            sb.AppendLine($"Diff:              {stat.Diff:d'.'hh':'mm':'ss}");
            sb.AppendLine($"Diff (workday):    {stat.DiffWd:F3}");
            sb.AppendLine();
            sb.AppendLine($"First day:         {stat.WorkDays.FirstOrDefault()?.Date:yyyy-MM-dd}");
            sb.AppendLine($"Days:              {stat.WorkDays.Count}");
            sb.AppendLine($"Workdays:          {stat.WorkDayCount}");
            return sb.ToString();
        }

        private DataHandler DataHandler { get; } = new DataHandler();

        System.Windows.Threading.DispatcherTimer _dispatcherTimer;
        TimeSpan _workTime;
        DateTime _playPressedTime;
        private bool _isHoliday;

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            DataHandler.LogStart();
            _playPressedTime = DateTime.Now;
            SetPlayGui();
        }
        private void SetPlayGui()
        {
            PlayButton.Visibility = Visibility.Hidden;
            SetDateLabel();
            StopButton.Visibility = Visibility.Visible;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopButton.Visibility = Visibility.Hidden;

            var lastTime = DateTime.Now - _playPressedTime;
            _playPressedTime = DateTime.MinValue;
            _workTime += lastTime;
            SetDateLabel();
            DataHandler.LogStop();

            PlayButton.Visibility = Visibility.Visible;
        }

        DateTime _lastTick;
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;

            if (_lastTick.Day != now.Day)
            {
                if (_playPressedTime != DateTime.MinValue)
                {
                    _playPressedTime = DateTime.Now;
                    DataHandler.LogDayTransition(_lastTick, now);
                }

                SetDateLabel();
            }

            _lastTick = now;

            if (_playPressedTime == DateTime.MinValue)
                return;
            SetWorkHoursLabel(_workTime + (now - _playPressedTime));
        }

        private void SetDateLabel()
        {
            var text = _playPressedTime == DateTime.MinValue ? GetStopText() : GetPlayText();
            this.Title = $"{DateTime.Now:yyyy.MM.dd dddd} - {text}";
        }

        private void SetWorkHoursLabel(TimeSpan time)
        {
            WorkHoursLabel.Content = GetWorkHoursText(time);
            WorkDayProgress.Value = time.TotalSeconds;
        }
        private string GetWorkHoursText(TimeSpan time)
        {
            return $"{time:hh\\:mm\\:ss}";
        }

        private string GetPlayText()
        {
            return "Hard working...";
        }
        private string GetStopText()
        {
            return "Relaxing...";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_playPressedTime != DateTime.MinValue)
                StopButton_Click(null, null);
        }

        private void WorkHoursLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenLog();
        }
        private void DateLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenLog();
        }
        private void DayTypeLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenHolidays();
        }
        private void StatisticsTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RefreshStatistics();
        }

        private void OpenLog()
        {
            Process.Start("notepad.exe", DataHandler.GetLogFilePath());
        }
        private void OpenHolidays()
        {
            Process.Start("notepad.exe", DataHandler.GetHolidayFilePath());
        }
        private void RefreshStatistics()
        {
            StatisticsTextBox.Text = StatisticsToString(DataHandler.GetStatistics());
        }

    }
}
