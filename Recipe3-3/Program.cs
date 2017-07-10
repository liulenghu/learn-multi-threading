using System;
using System.Diagnostics;
using System.Threading;

namespace Recipe3_3
{
    class Program
    {
        /// <summary>
        /// 线程池和并行度
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            const int numberOfOpeartions = 500;
            var sw = new Stopwatch();
            sw.Start();
            UseThreads(numberOfOpeartions);
            sw.Stop();
            Console.WriteLine($"使用线程的执行时间：{sw.ElapsedMilliseconds}ms");

            sw.Reset();
            sw.Start();
            UseThreadPool(numberOfOpeartions);
            sw.Stop();
            Console.WriteLine($"使用线程池的执行时间：{sw.ElapsedMilliseconds}ms");

            Console.ReadLine();
        }

        /// <summary>
        /// 使用线程
        /// </summary>
        /// <param name="numberOfOperations"></param>
        static void UseThreads(int numberOfOperations)
        {
            using (var countdown = new CountdownEvent(numberOfOperations))
            {
                Console.WriteLine("通过创建线程执行处理");
                for (int i = 0; i < numberOfOperations; i++)
                {
                    var thread = new Thread(() =>
                    {
                        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countdown.Signal();
                    });
                    thread.Start();
                }
                countdown.Wait();

                Console.WriteLine();
            }
        }

        /// <summary>
        /// 使用线程池
        /// </summary>
        /// <param name="numberOfOperations"></param>
        static void UseThreadPool(int numberOfOperations)
        {
            using (var countdown = new CountdownEvent(numberOfOperations))
            {
                Console.WriteLine("在一个线程池中执行处理");
                for (int i = 0; i < numberOfOperations; i++)
                {
                    ThreadPool.QueueUserWorkItem(_ => {
                        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countdown.Signal();
                    });
                }
                countdown.Wait();

                Console.WriteLine();
            }
        }
    }
}
