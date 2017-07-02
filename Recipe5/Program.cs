using System;
using System.Threading;

namespace Recipe5
{
    class Program
    {
        /// <summary>
        /// 检测线程状态
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Starting program...");
            Thread t = new Thread(PrintNumbersWithStatus);
            Thread t2 = new Thread(DoNothing);
            Console.WriteLine(t.ThreadState.ToString()); // Unstarted
            t2.Start();
            t.Start();

            for (int i = 0; i < 30; i++)
            {
                Console.WriteLine(t.ThreadState.ToString()); // Running => WaitSleepJoin
                // 估计在30个迭代周期内状态从Running变为WaitSleepJoin
                // 如果没变请增加迭代次数
            }
            Thread.Sleep(TimeSpan.FromSeconds(6));
            t.Abort();
            Console.WriteLine("A thread has been aborted");

            Console.WriteLine(t.ThreadState.ToString()); // AbortRequested <= 被终止后的线程状态
            Console.WriteLine(t2.ThreadState.ToString()); // Stopped <= 正常执行结束

            Console.ReadLine();
        }

        static void DoNothing()
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        static void PrintNumbersWithStatus()
        {
            Console.WriteLine("Starting...");
            Console.WriteLine(Thread.CurrentThread.ThreadState.ToString());
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.WriteLine(i);
            }
        }
    }
}
