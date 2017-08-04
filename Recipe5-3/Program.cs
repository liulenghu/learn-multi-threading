using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe5_3
{
    class Program
    {
        /// <summary>
        /// 对连续的异步任务使用await操作符
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

        /// <summary>
        /// 使用TPL方式的实现
        /// 功能等同于AsynchronyWithAwait方法的功能
        /// 但是写法复杂很多
        /// </summary>
        /// <returns></returns>
        static Task AsynchronyWithTPL()
        {
            var containerTask = new Task(() => {
                Task<string> t = GetInfoAsync("TPL 1");
                t.ContinueWith(task => {
                    Console.WriteLine(t.Result);
                    Task<string> t2 = GetInfoAsync("TPL 2");
                    t2.ContinueWith(innerTask => Console.WriteLine(innerTask.Result),
                        TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.AttachedToParent);
                    t2.ContinueWith(innerTask => Console.WriteLine(innerTask.Exception.InnerException),
                        TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.AttachedToParent);
                }, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.AttachedToParent);

                t.ContinueWith(task => Console.WriteLine(t.Exception.InnerException),
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.AttachedToParent);
            });
            containerTask.Start();
            return containerTask;
        }

        static async Task AsynchronyWithAwait()
        {
            try
            {
                // 使用了两个await操作符，但代码还是按顺序执行的
                string result = await GetInfoAsync("Async 1");
                Console.WriteLine(result);
                result = await GetInfoAsync("Async 2");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static async Task<string> GetInfoAsync(string name)
        {
            Console.WriteLine($"Task {name} started!");
            await Task.Delay(TimeSpan.FromSeconds(2));
            if (name == "TPL 2" || name == "Async 2")
            {
                throw new Exception("Boom!");
            }
            return $"Task {name} is running on a thrad id {Thread.CurrentThread.ManagedThreadId}. " +
                $"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
        }
    }
}
