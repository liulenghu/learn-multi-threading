using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe4_5
{
    class Program
    {
        /// <summary>
        /// 将EAP模式转换为任务
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 主要是使用TaskCompletionSource类型
            // T是异步处理返回结果类型
            var tcs = new TaskCompletionSource<int>();
            var worker = new BackgroundWorker();
            worker.DoWork += (sender, eventArgs) =>
            {
                eventArgs.Result = TaskMethod("Background worker", 5);
            };

            worker.RunWorkerCompleted += (sender, eventArgs) =>
            {
                if (eventArgs.Error != null)
                {
                    tcs.SetException(eventArgs.Error);
                }
                else if (eventArgs.Cancelled)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    try
                    {
                        // 将SetResult方法封装在try-catch中，保证发生错误时仍然会设置给任务完成源对象
                        // 若发生异常导致没有执行SetResult，则程序会一直阻塞在获取Task结果那里(tcs.Task.Result)
                        tcs.SetResult((int)eventArgs.Result);
                    }
                    catch (Exception)
                    {
                        tcs.SetResult(0);
                    }
                }
            };

            worker.RunWorkerAsync();
            int result = tcs.Task.Result;

            Console.WriteLine($"Result is: {result}");
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
