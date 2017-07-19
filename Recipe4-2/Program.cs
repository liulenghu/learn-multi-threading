using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe4_2
{
    class Program
    {
        /// <summary>
        ///  使用任务执行基本的操作
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 直接调用，同步执行，不是线程池中的线程
            TaskMethod("Main Thread Task");
            
            Task<int> task = CreateTask("Task 1");
            // 启动任务
            // 该任务会被放置在线程池中
            task.Start();
            // 等待结果（直到任务返回前，主线程一直处于阻塞状态）
            int result = task.Result;
            Console.WriteLine($"Result is: {result}");

            task = CreateTask("Task 2");
            // 同步运行该任务
            // 该任务会运行在主线程中，运行结果同直接调用一样
            task.RunSynchronously();
            result = task.Result;
            Console.WriteLine($"Result is: {result}");

            task = CreateTask("Task 3");
            Console.WriteLine(task.Status); // Created
            // 启动任务
            task.Start();
            // 这里没有阻塞主线程，而是在任务完成前循环打印任务状态
            // 任务状态分别为：Created、Running和RanToCompletion
            while (!task.IsCompleted)
            {
                Console.WriteLine(task.Status); // Running和RanToCompletion
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Console.WriteLine(task.Status); // RanToCompletion
            result = task.Result;
            Console.WriteLine($"Result is: {result}");

            Console.ReadLine();
        }

        static Task<int> CreateTask(string name)
        {
            return new Task<int>(() => TaskMethod(name));
        }

        static int TaskMethod(string name)
        {
            Console.WriteLine($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            return 42;
        }
    }
}
