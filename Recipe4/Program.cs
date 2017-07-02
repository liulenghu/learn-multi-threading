using System;
using System.Threading;

namespace Recipe4
{
    class Program
    {
        /// <summary>
        /// 终止线程
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Thread t = new Thread(PrintNumbersWithDelay);
            t.Start();
            Thread.Sleep(TimeSpan.FromSeconds(6));
            // 终止线程
            // 这给线程注入了ThreadAbortException方法，导致线程被终结。这非常危险，因为该异常可以在任何时刻发生并可能摧毁应用程序
            // 另外，使用该技术也不一定总能终止线程。
            // 目标线程可以通过处理该异常并调用Thread.ResetAbort方法来拒绝被终止。
            // 因此并不推荐使用Abort方法来终止线程。
            // 可优先使用一些其它方法，比如提供一个CancellationToken方法来取消线程的执行。
            t.Abort();
            Console.WriteLine("A thread has been aborted");

            Thread t2 = new Thread(PrintNumbers);
            t2.Start();

            PrintNumbers();
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
