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

namespace DestractLess
{

    public enum ButtonState
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
        private ButtonState btnState = ButtonState.Create;
        private int _previousWindowState = -1;


        public MainWindow()
        {
            InitializeComponent();
            trayIcon = new NotifyIcon();
            trayIcon.Icon = new System.Drawing.Icon(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "tray.ico"));

            trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(DoubleClick);
            this.StateChanged += new EventHandler(StateChangedHandler);

            btnState = ButtonState.Create;
            this.button.Content = "Set timer";

        }

        public void TimerCallBack()
        {

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


            btnState = ButtonState.Finished;
            button.Content = "Continue";
            label_t.Reset();


            //switch (this.label_t.State)
            //{
            //    case TimerState.Finished:


            //        break;
            //    case TimerState.Faulted:break;

            //    default: break;

            //}
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            switch (btnState)
            {
                case ButtonState.Create:
                    if (int.TryParse(this.Hours.Text, out int hours) &&
                        int.TryParse(this.Minutes.Text, out int minutes) &&
                        int.TryParse(this.Seconds.Text, out int seconds))
                    {
                        this.label_t.SetTimer(hours, minutes, seconds);
                        this.button.Content = "Start";
                        btnState = ButtonState.Ready;
                    }
                    break;
                case ButtonState.Ready:
                case ButtonState.Paused:
                case ButtonState.Finished:
                    this.label_t.Start(this.TimerCallBack);
                    this.btnState = ButtonState.Running;
                    this.button.Content = "Pause";
                    if (_previousWindowState > 0)
                    {
                        this.WindowState = (WindowState)_previousWindowState;
                        _previousWindowState = -1;
                    }
                    break;
                case ButtonState.Running:
                    this.label_t.Stop();
                    this.btnState = ButtonState.Paused;
                    this.button.Content = "Continue";
                    break;
                default: break;
            }
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

                this.button.Content = "Restart";
                this.btnState = ButtonState.Create;

            }
        }
    }
}
