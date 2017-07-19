using System;
using System.Threading;

namespace Recipe3_6
{
    class Program
    {
        /// <summary>
        /// 使用计时器
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Press 'Enter' to stop the timer ...");
            DateTime start = DateTime.Now;
            // 第一个参数为定时器定时执行的处理
            // 第三个参数为多长时间后第一次执行
            // 第四个参数为第一次执行之后再次调用的间隔时间
            _timer = new Timer(_ => TimerOperation(start), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            try
            {
                Thread.Sleep(TimeSpan.FromSeconds(6));
                // 使用Change方法改变第一次执行时间和之后再次调用的间隔时间
                _timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4));
                Console.ReadLine();
            }
            catch (Exception)
            {
                // 注销定时器
                _timer.Dispose();
            }
        }

        static Timer _timer;

        static void TimerOperation(DateTime start)
        {
            TimeSpan elapsed = DateTime.Now - start;
            Console.WriteLine($"{elapsed.TotalSeconds} seconds from {start}. Timer thread pool thrad id:{Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
