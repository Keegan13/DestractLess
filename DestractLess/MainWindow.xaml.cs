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
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DestractLess
{

    public enum TimerState
    {
        Create,
        Paused,
        Ready,
        Running,
        Finished
    }



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// TODO:   create dynamic icon
    ///         focus, and close to tray
    public partial class MainWindow : Window
    {
        private NotifyIcon trayIcon;
        private TimerState State = TimerState.Create;
        protected TimeSpan timer;
        private int _previousWindowState = -1;
        private Stopwatch s;
        private Task _awaiter;



        public MainWindow()
        {
            InitializeComponent();
            trayIcon = new NotifyIcon();
            trayIcon.Icon = new System.Drawing.Icon(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "tray.ico"));

            trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(DoubleClick);
            this.StateChanged += new EventHandler(StateChangedHandler);
            this.s = new Stopwatch();

            this.Minutes.Text = "10";
            this.timer = TimeSpan.FromMinutes(10);
            this.button.Content = "Start";
            State = TimerState.Ready;
            UpdateTimer(timer);

        }


        private void StateChangedHandler(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                trayIcon.BalloonTipTitle = "Minimize Sucessful";
                trayIcon.BalloonTipText = "Minimized the app ";
                trayIcon.ShowBalloonTip(400);
                trayIcon.Visible = true;

            }
            if (this.WindowState == WindowState.Normal)
            {
                trayIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void DoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            this.WindowState = WindowState.Normal;
            //this.ShowInTaskbar = true;
            //this.Show();
        }

        public async Task Start()
        {
            
            if (State == TimerState.Paused)
                return;

            State = TimerState.Running;
            checked
            {
                while (timer.Ticks - s.Elapsed.Ticks > 0)
                {
                    this.label_elapsed.Content = s.Elapsed;
                    this.label_diff.Content = timer - s.Elapsed;
                    s.Start();
                    UpdateTimer(timer - s.Elapsed);
                    if (_awaiter != null)
                    {
                        await _awaiter;
                        s.Start();
                        State = TimerState.Running;
                        _awaiter = null;
                    }
                    await Task.Delay(100);
                }
            }
            s.Stop();
            s.Reset();

            if (this.WindowState == WindowState.Minimized)
            {
                this._previousWindowState = (int)WindowState;
                this.WindowState = WindowState.Normal;
            }

            this.Show();
            this.Activate();
            this.Focus();
            this.Topmost = true;
            this.Topmost = false;
            UpdateTimer(timer);
            State = TimerState.Finished;
            button.Content = "Continue";

        }




        private void button_Click(object sender, RoutedEventArgs e)
        {
            switch (State)
            {
                case TimerState.Create:
                    if (int.TryParse(this.Hours.Text, out int hours) &&
                        int.TryParse(this.Minutes.Text, out int minutes) &&
                        int.TryParse(this.Seconds.Text, out int seconds))
                    {
                        this.timer = new TimeSpan(hours, minutes, seconds);
                        UpdateTimer(timer-s.Elapsed);
                        this.button.Content = "Start";
                        State = TimerState.Ready;
                    }
                    break;
                case TimerState.Paused:
                case TimerState.Ready:              
                case TimerState.Finished:
                    _awaiter?.Start();
                    Start();
                    this.State = TimerState.Running;
                    this.button.Content = "Pause";
                    if (_previousWindowState > 0)
                    {
                        this.WindowState = (WindowState)_previousWindowState;
                        _previousWindowState = -1;
                    }
                    break;
                case TimerState.Running:
                    Stop();
                    this.State = TimerState.Paused;
                    this.button.Content = "Continue";
                    break;
                default: break;
            }
        }

        private void UpdateTimer(TimeSpan t)
        {
            this.label_t.Content = t.ToString(@"hh\:mm\:ss\.fff");
        }

        private void Stop()
        {
            s.Stop();
            this._awaiter = new Task(() => { });
        }

        private void Timer_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as System.Windows.Controls.TextBox;
            if (this.button != null)
            {
                if (Regex.IsMatch(textbox.Text, @"^[0-9]{1,2}$") && int.TryParse(textbox.Text, out int result) && result < 60)
                {

                }
                else
                {
                    textbox.Text = "0";
                }
                s.Stop();
                s.Reset();
                this.button.Content = "Restart";
                this.State = TimerState.Create;
                

            }
        }
    }
}
