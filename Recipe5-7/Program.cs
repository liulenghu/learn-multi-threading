using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe5_7
{
    class Program
    {
        /// <summary>
        /// 使用 async void 方法
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 使用 async Task 可以通过返回值的Task示例，监控任务的状态
            Task t = AsyncTask();
            t.Wait();

            // 使用 async void 没有返回值，无法监控任务状态
            AsyncVoid();
            // 这里使用Sleep方法确保任务完成
            Thread.Sleep(TimeSpan.FromSeconds(3));

            // 根据Task.IsFaulted属性可以判断是否发生异常
            // 捕获的异常信息可以从Task.Exception中获取
            t = AsyncTaskWithErrors();
            while (!t.IsFaulted)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            Console.WriteLine(t.Exception);

            // 该段代码虽然使用try/catch捕获异常，但是由于使用了 async void 方法，
            // 异常处理方法会被放置到当前的同步上下文中（即线程池的线程中）。
            // 线程池中未被处理的异常会终止整个进程。

            //try
            //{
            //    AsyncVoidWithErrors();
            //    Thread.Sleep(TimeSpan.FromSeconds(3));
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}

            // Action类型也是可以使用 async 关键字的；
            // 在lambda表达式中很容易忘记对异常的处理，而这会导致程序崩溃。
            int[] numbers = { 1, 2, 3, 4, 5 };
            Array.ForEach(numbers, async number =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                if (number == 3)
                {
                    throw new Exception("Boom!");
                }
                Console.WriteLine(number);
            });

            Console.ReadLine();
        }

        static async Task AsyncTaskWithErrors()
        {
            string result = await GetInfoAsync("AsyncTaskException", 2);
            Console.WriteLine(result);
        }

        static async void AsyncVoidWithErrors()
        {
            string result = await GetInfoAsync("AsyncVoidException", 2);
            Console.WriteLine(result);
        }

        static async Task AsyncTask()
        {
            string result = await GetInfoAsync("AsyncTask", 2);
            Console.WriteLine(result);
        }

        static async void AsyncVoid()
        {
            string result = await GetInfoAsync("AsyncVoid", 2);
            Console.WriteLine(result);
        }

        static async Task<string> GetInfoAsync(string name, int seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            if (name.Contains("Exception"))
            {
                throw new Exception($"Boom from {name}");
            }
            return $"Task {name} is running on a thrad id {Thread.CurrentThread.ManagedThreadId}. " +
                $"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
        }
    }
}
