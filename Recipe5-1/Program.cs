using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe5_1
{
    class Program
    {
        /// <summary>
        /// 使用await操作符获取异步任务结果
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Task t = AsynchronyWithTPL();
            t.Wait();

            t = AsynchronyWithAwait();
            t.Wait();

            Console.ReadLine();
        }

        // 标准TPL的写法
        static Task AsynchronyWithTPL()
        {
            Task<string> t = GetInfoAsync("Task 1");
            Task t2 = t.ContinueWith(task => Console.WriteLine(t.Result),
                TaskContinuationOptions.NotOnFaulted);
            Task t3 = t.ContinueWith(task => Console.WriteLine(t.Exception.InnerException),
                TaskContinuationOptions.OnlyOnFaulted);
            return Task.WhenAny(t2, t3);
        }

        // 使用 async await 关键字的写法
        static async Task AsynchronyWithAwait()
        {
            try
            {
                string result = await GetInfoAsync("Task 2");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static async Task<string> GetInfoAsync(string name)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            return $"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. " +
                $"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
        }
    }
}
