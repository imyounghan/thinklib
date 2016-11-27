using System;
using System.Timers;

namespace ThinkLib.Scheduling
{
    /// <summary>
    /// 定时调度程序
    /// </summary>
    public sealed class TimeScheduler : DisposableObject
    {
        /// <summary>
        /// 创建一个定时器
        /// </summary>
        public static TimeScheduler Create(string name, Action action)
        {
            return new TimeScheduler(name, action);
        }


        private readonly Timer _timer;
        private readonly Action _action;

        private readonly string _name;


        private TimeScheduler(string name, Action action)
        {
            name.NotNullOrEmpty("name");
            action.NotNull("action");

            this._name = name;
            this._action = action;
            this._timer = BuildTimer();
        }

        private Timer BuildTimer()
        {
            var timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler((object source, ElapsedEventArgs e) => {
                try {
                    _action();
                }
                catch(Exception ex) {
                    if(LogManager.Default.IsErrorEnabled)
                        LogManager.Default.Error(ex, "Scheduler of {0} Encounters an error.", _name);
                }

                (source as Timer).Start();
            });
            timer.AutoReset = false;

            return timer;
        }

        /// <summary>
        /// 重新设置任务间隔
        /// </summary>
        /// <param name="interval">间隔时间(毫秒)</param>
        public TimeScheduler SetInterval(double interval)
        {
            _timer.Interval = interval;
            return this;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {

            _timer.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
        }

        /// <summary>
        /// 终止定时器
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if(disposing)
                _timer.Dispose();
        }
    }
}
