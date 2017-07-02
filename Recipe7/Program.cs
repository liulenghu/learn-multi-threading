using System;
using System.Threading;

namespace Recipe7
{
    class Program
    {
        /// <summary>
        /// 前台线程和后台线程
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var sampleForeground = new ThreadSample(10);
            var sampleBackground = new ThreadSample(20);

            var threadOne = new Thread(sampleForeground.CountNumbers);
            threadOne.Name = "ForegroundThread";
            var threadTwo = new Thread(sampleBackground.CountNumbers);
            threadTwo.Name = "BackgroundThread";
            // 设置为后台线程
            threadTwo.IsBackground = true;

            threadOne.Start();
            threadTwo.Start();
            // 前台线程终止后，程序结束，并且后台线程被终结
            // 进程会等待所有的前台线程完成后再结束工作，但是如果只剩下后台线程，则会直接结束工作。
        }

        class ThreadSample
        {
            private readonly int _iterations;

            public ThreadSample(int iterations)
            {
                _iterations = iterations;
            }

            public void CountNumbers()
            {
                for (int i = 0; i < _iterations; i++)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    Console.WriteLine($"{Thread.CurrentThread.Name} prints {i}");
                }
            }
        }
    }
}
