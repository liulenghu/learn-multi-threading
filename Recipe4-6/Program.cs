using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe4_6
{
    class Program
    {
        /// <summary>
        /// 实现取消选项
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var longTask = new Task<int>(() => TaskMethod("Task 1", 10, cts.Token));
            Console.WriteLine(longTask.Status); // Created
            cts.Cancel();
            Console.WriteLine(longTask.Status); // Created
            Console.WriteLine($"First task has benn cancelled before execution");

            cts = new CancellationTokenSource();
            longTask = new Task<int>(() => TaskMethod("Task 2", 10, cts.Token));
            longTask.Start();
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine(longTask.Status); // Running
            }
            cts.Cancel();
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine(longTask.Status); // RanToCompletion
            }

            Console.WriteLine($"A task has been completed with result {longTask.Result}"); // -1
            Console.ReadLine();
        }

        static int TaskMethod(string name, int seconds, CancellationToken token)
        {
            Console.WriteLine($"Task {name} is running on a thread id : {Thread.CurrentThread.ManagedThreadId}. " +
                $"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            for (int i = 0; i < seconds; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                if (token.IsCancellationRequested)
                {
                    return -1;
                }
            }
            return 42 * seconds;
        }
    }
}
