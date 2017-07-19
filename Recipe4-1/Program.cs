using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe4_1
{
    class Program
    {
        static void Main(string[] args)
        {
            var t1 = new Task(() => TaskMethod("Task 1"));
            var t2 = new Task(() => TaskMethod("Task 2"));
            t2.Start();
            t1.Start();

            // 无需调用Start方法，立即开始工作
            // Task.Run只是Task.Factory.StartNew的一个快捷方式，但是后者有附加的选项
            Task.Run(() => TaskMethod("Task 3"));
            Task.Factory.StartNew(() => TaskMethod("Task 4"));

            // 标记任务为长时间运行，结果该任务将不使用线程池，而在单独的线程中运行；
            // 然而根据该任务的当前的任务调度器（task scheduler）,运行方式有可能不同。
            Task.Factory.StartNew(() => TaskMethod("Task 5"), TaskCreationOptions.LongRunning);

            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.ReadLine();
        }

        static void TaskMethod(string name)
        {
            Console.WriteLine($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
        }
    }
}
