using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe7_1
{
    class Program
    {
        static void Main(string[] args)
        {
            // 调用Invoke方法并行的运行多个任务
            Parallel.Invoke(
                () => EmulateProcessing("Task1"),
                () => EmulateProcessing("Task2"),
                () => EmulateProcessing("Task3")
            );

            // 使用ForEach方法并行的循环任务
            var cts = new CancellationTokenSource();
            var result = Parallel.ForEach(
                Enumerable.Range(1, 30),
                new ParallelOptions
                {
                    // 可以指定CancellationToken取消循环
                    CancellationToken = cts.Token,
                    // 限制最大并行度
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    // 设置自定义的TaskScheduler类
                    TaskScheduler = TaskScheduler.Default
                },
                // Action可以接受一个附加的ParallelLoopState参数
                (i, state) =>
                {
                    Console.WriteLine(i);
                    if (i == 20)
                    {
                        // 调用Break方法停止循环
                        // Bread方法停止之后的迭代，但之前的迭代还要继续工作
                        state.Break();
                        // 也可以使用Stop方法停止循环
                        // Stop方法会告诉循环停止任何工作，并设置并行循环状态属性IsStopped值为true
                        // state.Stop();
                        Console.WriteLine($"Loop is stopped: {state.IsStopped}");
                    }
                });

            Console.WriteLine("---");
            // 循环是否已完成
            Console.WriteLine($"IsCompleted: {result.IsCompleted}");
            // 最低迭代索引
            Console.WriteLine($"Lowest break iteration： {result.LowestBreakIteration}");

            Console.ReadLine();
        }

        static string EmulateProcessing(string taskName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(new Random(DateTime.Now.Millisecond).Next(250, 350)));
            Console.WriteLine($"{taskName} task was processed on a thrad id {Thread.CurrentThread.ManagedThreadId}");
            return taskName;
        }
    }
}
