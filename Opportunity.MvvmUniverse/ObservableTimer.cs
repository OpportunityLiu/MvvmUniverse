using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Opportunity.MvvmUniverse
{
    /// <summary>
    /// A wrapper of <see cref="DispatcherTimer"/> that is observable.
    /// </summary>
    public class ObservableTimer : ObservableObject
    {
        private readonly DispatcherTimer _Timer = new DispatcherTimer();

        /// <summary>
        /// Create new instance of <see cref="ObservableTimer"/>.
        /// </summary>
        /// <param name="interval">Interval of timer's ticks.</param>
        /// <param name="countDown">Initial value for <see cref="CountDown"/> counter.</param>
        public ObservableTimer(TimeSpan interval, int countDown)
        {
            Interval = interval;
            CountDown = countDown;
            _Timer.Tick += _Timer_Tick;
        }

        /// <summary>
        /// Create new instance of <see cref="ObservableTimer"/>.
        /// </summary>
        /// <param name="interval">Interval of timer's ticks.</param>
        public ObservableTimer(TimeSpan interval)
            : this(interval, -1) { }

        /// <summary>
        /// Create new instance of <see cref="ObservableTimer"/>.
        /// </summary>
        public ObservableTimer()
            : this(TimeSpan.FromSeconds(1)) { }

        private void _Timer_Tick(object sender, object e)
        {
            if (_CountDown >= 0)
                CountDown--;
            OnTick(_TickCommandParameter);
            if (_CountDown == 0)
                IsEnabled = false;
        }

        /// <summary>
        /// Reset the enabled timer, so the <see cref="Tick"/> will wait for another <see cref="Interval"/>.
        /// </summary>
        public void Reset()
        {
            if (_Timer.IsEnabled)
            {
                _Timer.Stop();
                _Timer.Start();
            }
        }

        /// <summary>
        /// Interval of timer's ticks.
        /// </summary>
        public TimeSpan Interval
        {
            get => _Timer.Interval;
            set
            {
                _Timer.Interval = value;
                OnPropertyChanged();
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _IsEnabled;
        /// <summary>
        /// Indicate the timer is runing or not.
        /// </summary>
        public bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                if (Set(ref _IsEnabled, value))
                {
                    if (value)
                        _Timer.Start();
                    else
                        _Timer.Stop();
                }
            }
        }

        /// <summary>
        /// Raise <see cref="Tick"/> event and <see cref="TickCommand"/>.
        /// </summary>
        /// <param name="parameter">Command parameter for <see cref="Tick"/> and <see cref="TickCommand"/>.</param>
        protected virtual void OnTick(object parameter)
        {
            _TickCommand?.Execute(parameter);
            _ = _Tick.RaiseAsync(this, parameter);
        }

        private readonly DepedencyEvent<EventHandler<object>, ObservableTimer, object> _Tick
            = new DepedencyEvent<EventHandler<object>, ObservableTimer, object>((h, s, e) => h(s, e));
        /// <summary>
        /// Raised every <see cref="Interval"/> when the timer <see cref="IsEnabled"/>.
        /// </summary>
        public event EventHandler<object> Tick
        {
            add => _Tick.Add(value);
            remove => _Tick.Remove(value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand _TickCommand;
        /// <summary>
        /// Command will execute when <see cref="Tick"/>.
        /// </summary>
        public ICommand TickCommand { get => _TickCommand; set => Set(ref _TickCommand, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object _TickCommandParameter;
        /// <summary>
        /// Parameter of <see cref="TickCommand"/>.
        /// </summary>
        public object TickCommandParameter { get => _TickCommandParameter; set => Set(ref _TickCommandParameter, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _CountDown = -1;
        /// <summary>
        /// Decrese every time <see cref="Tick"/> raised, will disable the <see cref="ObservableTimer"/> when reaches 0.
        /// Set to negative integer to disable this feature.
        /// </summary>
        public int CountDown { get => _CountDown; set => Set(ref _CountDown, value); }
    }
}
