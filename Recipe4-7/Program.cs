using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe4_7
{
    class Program
    {
        /// <summary>
        /// 处理任务中的异常
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Task<int> task;
            try
            {
                task = Task.Run(() => TaskMethod("Task 1", 2));
                // 尝试同步获取task的结果
                // 捕获的异常是一个被封装的异常（AggregateExcption），可以访问InnerException获取底层异常
                int result = task.Result;
                Console.WriteLine($"Result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught: {ex}");
            }
            Console.WriteLine("----------------------------------------");
            Console.WriteLine();

            try
            {
                task = Task.Run(() => TaskMethod("Task 2", 2));
                // 使用GetAwaiter和GetResult方法获取任务结果
                // 这种情况下不会封装异常，因为TPL基础设施会提取该异常。
                // 如果只有一个底层任务，那么一次只能获取一个原始异常。
                int result = task.GetAwaiter().GetResult();
                Console.WriteLine($"Result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught: {ex}");
            }
            Console.WriteLine("----------------------------------------");
            Console.WriteLine();

            var t1 = new Task<int>(() => TaskMethod("Task 3", 3));
            var t2 = new Task<int>(() => TaskMethod("Task 4", 2));
            var complexTask = Task.WhenAll(t1, t2);
            // 待t1和t2都结束后才会打印异常
            // 异常类型为AggregateExcption，其内部封装了2个任务抛出的异常
            var exceptionHandler = complexTask.ContinueWith(
                t => Console.WriteLine($"Exception caught: {t.Exception}"),
                TaskContinuationOptions.OnlyOnFaulted);
            t1.Start();
            t2.Start();
            Thread.Sleep(TimeSpan.FromSeconds(5));
            Console.ReadLine();
        }

        static int TaskMethod(string name, int seconds)
        {
            Console.WriteLine($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. " +
                $"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            throw new Exception("Boom!");
            return 42 * seconds;
        }
    }
}
