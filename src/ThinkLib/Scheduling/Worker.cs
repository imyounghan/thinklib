using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThinkLib.Scheduling
{
    /// <summary>
    /// 后台循环执行一个特定的方法的工作器
    /// </summary>
    public class Worker
    {
        private CancellationTokenSource cancellationSource;

        /// <summary>Initialize a new Worker for the specified method to run.
        /// </summary>
        public Worker(Action processor, Action successCallback, Action<Exception> exceptionCallback)
            : this(successCallback, exceptionCallback)
        {
            processor.NotNull("processor");

            this.Processor = processor;
        }

        /// <summary>Initialize a new Worker for the specified method to run.
        /// </summary>
        protected Worker(Action successCallback = null, Action<Exception> exceptionCallback = null)
        {
            this.SuccessCallback = successCallback ?? EmptyMethod;
            this.ExceptionCallback = exceptionCallback ?? EmptyMethod;
        }

        private void EmptyMethod()
        { }

        private void EmptyMethod(Exception ex)
        { }

        /// <summary>
        /// 间隔时间(毫秒数)
        /// </summary>
        public int Interval { get; private set; }


        /// <summary>
        /// 成功调用的函数
        /// </summary>
        protected Action SuccessCallback { get; private set; }

        /// <summary>
        /// 失败调用的函数
        /// </summary>
        protected Action<Exception> ExceptionCallback { get; private set; }

        /// <summary>
        /// 要执行的函数。
        /// </summary>
        protected Action Processor { get; private set; }

        /// <summary>
        /// 表示一个持续工作的方法
        /// </summary>
        protected virtual void Working()
        {
            bool success = true;

            try {
                Processor.Invoke();
            }
            catch(Exception ex) {
                success = false;

                try {
                    ExceptionCallback(ex);
                }
                catch(Exception) {
                }
            }
            if(success) {
                try {
                    SuccessCallback();
                }
                catch(Exception) {
                }
            }
        }

        private void AlwaysRunning()
        {
            while(!cancellationSource.IsCancellationRequested) {
                this.Working();

                if(this.Interval > 0)
                    Thread.Sleep(Interval);
            }
        }

        private static readonly CancellationToken cancellationToken = new CancellationToken(true);
        /// <summary>
        /// 获取取消操作的通知
        /// </summary>
        public CancellationToken CancellationToken
        {
            get
            {
                return this.cancellationSource == null ? cancellationToken : this.cancellationSource.Token;
            }
        }

        /// <summary>
        /// 等待下一个任务的间隔时间
        /// </summary>
        /// <param name="interval">毫秒</param>
        public void SetInterval(int interval)
        {
            this.Interval = interval;
        }

        /// <summary>
        /// Start the worker.
        /// </summary>
        public void Start()
        {
            if(this.cancellationSource == null) {
                this.cancellationSource = new CancellationTokenSource();
                Task.Factory.StartNew(
                     this.AlwaysRunning,
                     this.cancellationSource.Token,
                     TaskCreationOptions.LongRunning,
                     TaskScheduler.Default);
            }
        }

        /// <summary>
        /// Stop the worker.
        /// </summary>
        public void Stop()
        {
            if(this.cancellationSource != null) {
                using(this.cancellationSource) {
                    this.cancellationSource.Cancel();
                }

                this.cancellationSource = null;
            }
        }
    }

    /// <summary>
    /// 后台循环执行一个特定的方法的工作器
    /// </summary>
    public class Worker<T> : Worker
    {
        /// <summary>
        /// constructor.
        /// </summary>
        protected Worker(Func<T> factory, Action<T> successCallback, Action<T, Exception> exceptionCallback)
        {
            factory.NotNull("factory");

            this.Factory = factory;
            this.SuccessCallback = successCallback ?? EmptyMethod;
            this.ExceptionCallback = exceptionCallback ?? EmptyMethod;
        }

        /// <summary>
        /// constructor.
        /// </summary>
        public Worker(Func<T> factory, Action<T> processor, Action<T> successCallback, Action<T, Exception> exceptionCallback)
            : this(factory, successCallback, exceptionCallback)
        {
            processor.NotNull("processor");

            this.Processor = processor;
        }


        private void EmptyMethod(T instance)
        { }

        private void EmptyMethod(T instance, Exception ex)
        { }

        /// <summary>
        /// 表示一个持续工作的方法
        /// </summary>
        protected override void Working()
        {
            var message = Factory();

            bool success = true;

            try {
                Processor.Invoke(message);
            }
            catch(Exception ex) {
                success = false;

                try {
                    ExceptionCallback(message, ex);
                }
                catch(Exception) {
                }
            }

            if(success) {
                try {
                    SuccessCallback(message);
                }
                catch(Exception) {
                }
            }
        }


        /// <summary>
        /// 获取该 <typeparamref name="T"/> 实例的工厂。
        /// </summary>
        protected Func<T> Factory { get; private set; }

        /// <summary>
        /// 要执行的函数。
        /// </summary>
        new protected Action<T> Processor { get; private set; }


        /// <summary>
        /// 成功调用的函数
        /// </summary>
        new protected Action<T> SuccessCallback { get; private set; }

        /// <summary>
        /// 失败调用的函数
        /// </summary>
        new protected Action<T, Exception> ExceptionCallback { get; private set; }
    }
}
