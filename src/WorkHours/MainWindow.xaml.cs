using System;
using System.Collections.Generic;
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

            _playPressedTime = DataHandler.GetWorkStart();
            _workTime = DataHandler.GetWorkHours();
            _lastTick = DateTime.Now;

            if (_playPressedTime != DateTime.MinValue)
                StopButton_Click(null, null);

            this.Title = GetStopText();
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

        System.Windows.Threading.DispatcherTimer _dispatcherTimer;
        TimeSpan _workTime;
        DateTime _playPressedTime;

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            DataHandler.LogStart();
            _playPressedTime = DateTime.Now;
            SetPlayGui();
        }
        private void SetPlayGui()
        {
            PlayButton.Visibility = Visibility.Hidden;
            this.Title = GetPlayText();
            StopButton.Visibility = Visibility.Visible;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopButton.Visibility = Visibility.Hidden;

            var lastTime = DateTime.Now - _playPressedTime;
            _playPressedTime = DateTime.MinValue;
            _workTime += lastTime;
            this.Title = GetStopText();
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
            DateLabel.Content = DateTime.Now.ToString("yyyy.MM.dd dddd");
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
            return "You are is working.";
        }
        private string GetStopText()
        {
            return "Relaxing...";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(_playPressedTime != DateTime.MinValue)
                StopButton_Click(null, null);
        }
    }
}
