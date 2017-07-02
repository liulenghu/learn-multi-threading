using System;
using System.Diagnostics;
using System.Threading;

namespace Recipe6
{
    class Program
    {
        /// <summary>
        /// 线程优先级
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine($"Current thread priority: {Thread.CurrentThread.Priority}");
            Console.WriteLine("Running on all cores available");
            // 在所有可用的CPU核心上启动线程
            // 如果拥有一个以上的计算核心，将在两秒内得到初步结果
            // 最高优先级的线程通常会计算更多的迭代，但两个值应该很接近
            RunThreads();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine("Running on a single core");
            // 让操作系统的所有线程运行在单个CPU核心（第一核心）上
            // 结果会完全不同，计算耗时也会超过2秒
            // 这是因为CPU核心大部分时间在运行高优先级的线程，只留给剩下的线程很少的时间来运行
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
            RunThreads();

            Console.ReadLine();
        }

        static void RunThreads()
        {
            var sample = new ThreadSample();

            var threadOne = new Thread(sample.CountNumbers);
            threadOne.Name = "ThreadOne";
            var threadTwo = new Thread(sample.CountNumbers);
            threadTwo.Name = "ThreadTwo";

            // 设置线程优先级
            threadOne.Priority = ThreadPriority.Highest;
            threadTwo.Priority = ThreadPriority.Lowest;
            threadOne.Start();
            threadTwo.Start();

            Thread.Sleep(TimeSpan.FromSeconds(2));
            sample.Stop();
        }

        private class ThreadSample
        {
            private bool _isStopped = false;

            public void Stop()
            {
                _isStopped = true;
            }

            public void CountNumbers()
            {
                long counter = 0;
                while(!_isStopped)
                {
                    counter++;
                }
                // 打印执行结果（相同时间内的迭代次数）
                Console.WriteLine($"{Thread.CurrentThread.Name} with {Thread.CurrentThread.Priority,11} priority has a counte = {counter,13:N0}");
            }
        }
    }
}
