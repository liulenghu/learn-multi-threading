using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe4_3
{
    class Program
    {
        /// <summary>
        /// 组合任务
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var firstTask = new Task<int>(() => TaskMethod("First Task", 3));
            var secondTask = new Task<int>(() => TaskMethod("Second Task", 2));

            // 设置后续操作
            firstTask.ContinueWith(t => Console.WriteLine($"The first answer is {t.Result}. " +
                $"Thread id {Thread.CurrentThread.ManagedThreadId}, " +
                $"is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}"),
                TaskContinuationOptions.OnlyOnRanToCompletion);

            firstTask.Start();
            secondTask.Start();

            // 等待上面的两个任务完成
            Thread.Sleep(TimeSpan.FromSeconds(4));

            // 给第二个任务设置后续操作，并使用TaskContinuationOptions.ExecuteSynchronously选项尝试同步执行该后续操作
            Task continuation = secondTask.ContinueWith(t => Console.WriteLine($"The first answer is {t.Result}. " +
                $"Thread id {Thread.CurrentThread.ManagedThreadId}, " +
                $"is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}"),
                TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
            // 为上面的后续操作再定义一个后续操作
            continuation.GetAwaiter().OnCompleted(() => Console.WriteLine($"Continuation Task Completed! " +
                $"Thread id: {Thread.CurrentThread.ManagedThreadId}, " +
                $"is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}"));

            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine();

            firstTask = new Task<int>(() =>
            {
                // 使用TaskCreationOptions.AttachedToParent运行子任务
                // 只有所有子任务结束工作，父任务才会完成
                var innerTask = Task.Factory.StartNew(() => TaskMethod("Second Task", 5), TaskCreationOptions.AttachedToParent);

                innerTask.ContinueWith(t => TaskMethod("Third Task", 2), TaskContinuationOptions.AttachedToParent);

                return TaskMethod("First task", 2);
            });

            firstTask.Start();

            // 打印任务状态
            // firstTask 父任务处理仍在执行中时状态为running
            // 父任务执行结束而子任务仍在执行时状态为WaitingForChildrenToComplete
            // 全部完成后状态为RanToCompletion
            while (!firstTask.IsCompleted)
            {
                Console.WriteLine(firstTask.Status); // running / WaitingForChildrenToComplete
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Console.WriteLine(firstTask.Status); // RanToCompletion
            Thread.Sleep(TimeSpan.FromSeconds(10));

            Console.ReadLine();
        }

        static int TaskMethod(string name, int seconds)
        {
            Console.WriteLine($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. " +
                $"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            return 42 * seconds;
        }
    }
}
