using System;
using System.Threading;

namespace Recipe3_1
{
    /// <summary>
    /// 在线程池中调用委托
    /// </summary>
    class Program
    {
        private delegate string RunOnThreadPool(out int threadId);

        static void Main(string[] args)
        {
            int threadId = 0;

            RunOnThreadPool poolDelegate = Test;

            // 不使用异步处理
            var t = new Thread(() => Test(out threadId));
            t.Start();
            // 等待t结束
            t.Join();
            // 打印结果
            Console.WriteLine($"线程Id：{threadId}");

            // 开始异步处理，并指定回调函数
            IAsyncResult r = poolDelegate.BeginInvoke(out threadId, Callback, "一个代理异步调用");
            // 等待poolDelegate执行结束
            r.AsyncWaitHandle.WaitOne();
            // 获取异步处理结果
            // 虽然threadId是按址传参，但也必须通过EndInvoke获取返回结果
            string result = poolDelegate.EndInvoke(out threadId, r);
            // 打印结果
            Console.WriteLine($"线程池工作线程Id：{threadId}");
            Console.WriteLine(result);
            // 这里的延迟是为了给回调函数足够的执行时间
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        private static void Callback(IAsyncResult ar)
        {
            Console.WriteLine("开始一个回调");
            // AsyncState值为BeginInvoke的第三个参数值
            Console.WriteLine($"回调状态：{ar.AsyncState}");
            Console.WriteLine($"是否为线程池中的线程：{Thread.CurrentThread.IsThreadPoolThread}"); // true
            // 回调函数的线程ID和异步处理的线程ID是相同的
            Console.WriteLine($"线程池工作线程Id：{Thread.CurrentThread.ManagedThreadId}");
        }

        private static string Test(out int threadId)
        {
            Console.WriteLine("开始...");
            Console.WriteLine($"是否为线程池中的线程：{Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            threadId = Thread.CurrentThread.ManagedThreadId;
            // 返回值可以通过代理的EndInvoke方法获取
            return $"线程池工作线程Id是 {Thread.CurrentThread.ManagedThreadId}";
        }
    }
}
