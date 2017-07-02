using System;
using System.Threading;

namespace Recipe3
{
    class Program
    {
        /// <summary>
        /// 线程等待
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Thread t = new Thread(PrintNumbersWithDelay);
            t.Start();
            // 线程等待直到t执行结束，主线程才会继续执行
            t.Join();

            PrintNumbers();
            Console.WriteLine("Thrad completed");
            Console.ReadLine();
        }

        static void PrintNumbers()
        {
            Console.WriteLine("Starting...");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
            }
        }

        static void PrintNumbersWithDelay()
        {
            Console.WriteLine("Starting...");
            for (int i = 0; i < 10; i++)
            {
                // 暂停线程
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.WriteLine(i);
            }
        }
    }
}
