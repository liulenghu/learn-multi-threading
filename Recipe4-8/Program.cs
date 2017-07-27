using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe4_8
{
    class Program
    {
        /// <summary>
        /// 并行运行任务
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 创建两个任务
            var firstTask = new Task<int>(() => TaskMethod("First Task", 3));
            var secondTask = new Task<int>(() => TaskMethod("Second Task", 2));
            // 使用Task.WhenAll创建第三个任务，该任务将在所有任务完成后执行
            // 该任务的结果提供了一个结果数组
            var whenAllTask = Task.WhenAll(firstTask, secondTask);
            whenAllTask.ContinueWith(
                t => Console.WriteLine($"The first answer is {t.Result[0]}, the second is {t.Result[1]}"),
                TaskContinuationOptions.OnlyOnRanToCompletion);

            firstTask.Start();
            secondTask.Start();

            Thread.Sleep(TimeSpan.FromSeconds(4));

            // 创建一个任务列表
            var tasks = new List<Task<int>>();
            for (int i = 0; i < 4; i++)
            {
                int counter = i;
                var task = new Task<int>(() => TaskMethod($"Task {counter}", counter));
                tasks.Add(task);
                task.Start();
            }

            while (tasks.Count > 0)
            {
                // 通过Task.WhenAny方法等待任何一个任务完成
                var completedTask = Task.WhenAny(tasks).Result;
                tasks.Remove(completedTask);
                Console.WriteLine($"A task has been completed with result {completedTask.Result}.");
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));
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
