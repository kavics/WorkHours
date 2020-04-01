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

            _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(0.5);
            _dispatcherTimer.Start();

            _workTime = GetWorkTime();

            WorkDayProgress.Minimum = 0;
            WorkDayProgress.Maximum = 8 * 60 * 60;

            SetWorkHoursLabel(_workTime);
            this.Title = GetStopText();
        }

        System.Windows.Threading.DispatcherTimer _dispatcherTimer;
        TimeSpan _workTime;
        DateTime _playPressedTime;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.Name == "PlayButton")
                PlayClick();
            if (button.Name == "StopButton")
                StopClick();
        }

        private void PlayClick()
        {
            PlayButton.Visibility = Visibility.Hidden;

            _playPressedTime = DateTime.Now;
            this.Title = GetPlayText();

            StopButton.Visibility = Visibility.Visible;
        }

        private void StopClick()
        {
            StopButton.Visibility = Visibility.Hidden;

            var lastTime = DateTime.Now - _playPressedTime;
            _playPressedTime = DateTime.MinValue;
            _workTime += lastTime;
            SetWorkTime(_workTime);
            this.Title = GetStopText();

            PlayButton.Visibility = Visibility.Visible;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_playPressedTime == DateTime.MinValue)
                return;
            SetWorkHoursLabel(_workTime + (DateTime.Now - _playPressedTime));
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


        /* ================================================================ */

        private TimeSpan GetWorkTime()
        {
            return new TimeSpan(0, 0, 0);
        }
        private void SetWorkTime(TimeSpan time)
        {
            // do nothing
        }
    }
}
