using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe4_4
{
    class Program
    {
        /// <summary>
        /// 将APM模式转换为任务
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            int threadId;
            AsynchronousTask d = Test;
            IncompatibleAsynchronousTask e = Test;

            // 将APM转换为TPL的关键是Task<T>.Factory.FromAsync方法
            // 其中T为异步操作结果的类型
            // 该方法有多个重载。

            Console.WriteLine("Option 1");
            // 这种方法可以指定回调函数
            Task<string> task = Task<string>.Factory.FromAsync(
                d.BeginInvoke(
                    "AsyncTaskThread",
                    Callback,
                    "a delegate asynchronous call"),
                d.EndInvoke);

            task.ContinueWith(t => Console.WriteLine($"Callback is finished, now running a continuation! Result: {t.Result}"));

            while (!task.IsCompleted)
            {
                Console.WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Console.WriteLine(task.Status);
            Thread.Sleep(TimeSpan.FromSeconds(1));

            Console.WriteLine("------------------------------------");
            Console.WriteLine();
            Console.WriteLine("Option 2");

            // 这种方法不能直接指定回调函数
            // 如果需要，可以使用Task.ContinueWith方法执行回调函数。
            task = Task<string>.Factory.FromAsync(
                d.BeginInvoke,
                d.EndInvoke,
                "AsyncTaskThread",
                "a delegate asynchronous call");

            task.ContinueWith(t => Console.WriteLine($"Task is completed, now running a continuation! Result: {t.Result}"));

            while (!task.IsCompleted)
            {
                Console.WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Console.WriteLine(task.Status);
            Thread.Sleep(TimeSpan.FromSeconds(1));

            Console.WriteLine("------------------------------------");
            Console.WriteLine();
            Console.WriteLine("Option 3");

            // 这里的异步方法带有out参数，导致EndInvoke的签名和FromAsync的签名不一致
            // 这里展示了一个小技巧，使用lambda表达式封装了EndInvoke方法
            IAsyncResult ar = e.BeginInvoke(out threadId, Callback, "a delegate asynchronous call");
            task = Task<string>.Factory.FromAsync(ar, _ => e.EndInvoke(out threadId, ar));

            task.ContinueWith(t => Console.WriteLine($"Task is completed, now running a continuation! Result: {t.Result}, ThreadId: {threadId}"));

            while (!task.IsCompleted)
            {
                Console.WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Console.WriteLine(task.Status);
            Thread.Sleep(TimeSpan.FromSeconds(1));

            Console.ReadLine();
        }

        delegate string AsynchronousTask(string threadName);
        delegate string IncompatibleAsynchronousTask(out int threadId);

        static void Callback(IAsyncResult ar)
        {
            Console.WriteLine("Starting a callback ...");
            Console.WriteLine($"State passed to a callback: {ar.AsyncState}");
            Console.WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Console.WriteLine($"Thread pool worker thread id: {Thread.CurrentThread.ManagedThreadId}");
        }

        static string Test(string threadName)
        {
            Console.WriteLine("Starting...");
            Console.WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Thread.CurrentThread.Name = threadName;
            return $"Thread anme: {Thread.CurrentThread.Name}";
        }

        static string Test(out int threadId)
        {
            Console.WriteLine("Starting...");
            Console.WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            threadId = Thread.CurrentThread.ManagedThreadId;
            return $"Thread pool worker thread id was: {threadId}";
        }
    }
}
