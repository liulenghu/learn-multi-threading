using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe5_4
{
    class Program
    {
        /// <summary>
        /// 对并行执行的异步任务使用await操作符
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Task t = AsynchronousProcessing();
            t.Wait();

            Console.ReadLine();
        }

        static async Task AsynchronousProcessing()
        {
            // 定义了两个任务，分别运行3秒和5秒
            Task<string> t1 = GetInfoAsync("Task 1", 3);
            Task<string> t2 = GetInfoAsync("Task 2", 5);
            // 使用Task.WhenALL辅助方法创建一个任务，该任务只有在所有底层任务完成后才会运行
            string[] results = await Task.WhenAll(t1, t2);
            // 5秒后获得观察结果
            foreach (string result in results)
            {
                Console.WriteLine(result);
            }
        }

        static async Task<string> GetInfoAsync(string name, int seconds)
        {
            // Task.Delay方法不阻塞线程，而是在幕后使用了一个计时器
            // 导致两个任务一般总是由同一个线程运行
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            // 如果使用下面注释掉的代码模拟运行时间，则两个任务总是由不同的线程运行
            // 因为Thread.Sleep方法阻塞线程
            // await Task.Run(() => Thread.Sleep(TimeSpan.FromSeconds(seconds)));
            return $"Task {name} is running on a thrad id {Thread.CurrentThread.ManagedThreadId}. " +
                $"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
        }
    }
}
