using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DestractLess
{
    public enum TimerState
    {
        Running,
        Unstarted,
        Paused,
        Finished,
        Faulted
    }
    public class LabelTimer : Label
    {


        public TimeSpan Timer { get { return _timer; } set { SetTimer(value); } }

        public TimerState State { get; protected set; }

        //
        private TimeSpan _timer;
        protected CancellationTokenSource cts;
        private object locker = new object();
        // that's a thing
        private Task _waiter { get; set; }

        public LabelTimer() : this(0, 0, 0)
        {

        }

        public LabelTimer(int hours, int minutes, int secods) : this(new TimeSpan(hours, minutes, secods))
        {

        }

        public LabelTimer(TimeSpan timer)
        {
            SetTimer(timer);
            State = TimerState.Unstarted;
        }


        public void SetTimer(int hours, int minutes, int second)
        {
            SetTimer(new TimeSpan(hours, minutes, second));
        }

        private void SetTimer(TimeSpan timer)
        {
            if (State == TimerState.Running || State == TimerState.Paused)
            {
                Reset();
            }
            _timer = timer;
            SetLabel(_timer);
        }
        protected void SetLabel(TimeSpan timer)
        {
            lock (locker)
            {
                if (timer != null)
                    this.Content = timer.ToString(@"hh\:mm\:ss\.ffff");
            }
        }

        public void Reset()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            
            State = TimerState.Unstarted;
            SetLabel(this._timer);
        }

        public void Stop()
        {
            switch (State)
            {
                case TimerState.Running:
                    this._waiter = new Task(() => { });
                    State = TimerState.Paused;
                    break;
                default: break;
            }

        }

        public void Start(Action callback)
        {
            switch (State)
            {
                case TimerState.Unstarted:
                case TimerState.Finished:
                case TimerState.Faulted:

                    cts = new CancellationTokenSource();
                    var token = cts.Token;

                    Task Main = Task.Factory.StartNew((x) =>
                    {
                        var label = x as LabelTimer;
                        Stopwatch s = new Stopwatch();
                        label.State = TimerState.Running;
                        s.Start();
                        while (label._timer > s.Elapsed)
                        {
                            if (label.State == TimerState.Paused)
                            {
                                s.Stop();
                                label._waiter.Wait();
                                label.State = TimerState.Running;
                                s.Start();
                            }

                            if (token.IsCancellationRequested)
                            {
                                s.Stop();
                                Dispatcher.Invoke(() =>
                                {
                                    label.SetLabel(label.Timer);
                                });
                                token.ThrowIfCancellationRequested();
                            }

                            Dispatcher.Invoke(() =>
                            {
                                label.SetLabel(label.Timer - s.Elapsed);
                            });
                            Task.Delay(100).Wait();
                        }
                        s.Stop();
                        Dispatcher.Invoke(() =>
                        {
                            label.SetLabel(TimeSpan.FromTicks(0));
                        });
                    }, this, token);

                    Task handler = Main.ContinueWith((prev) =>
                    {
                        State = TimerState.Finished;
                        if (prev.IsFaulted)
                        {
                            this.State = TimerState.Faulted;
                        }
                        Dispatcher.Invoke(callback);
                    });
                    break;
                case TimerState.Paused:
                    this._waiter?.Start();
                    break;
                default: break;
            }
        }


    }
}
