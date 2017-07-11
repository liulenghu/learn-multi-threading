using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe3_4
{
    class Program
    {
        /// <summary>
        /// 实现一个取消选项
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => AsyncOperation1(token));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }

            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => AsyncOperation2(token));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }

            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => AsyncOperation3(token));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.ReadLine();
        }

        static void AsyncOperation1(CancellationToken token)
        {
            Console.WriteLine("开始第一个任务");
            for (int i = 0; i < 5; i++)
            {
                // 轮询检查token.IsCancellationRequested，如果为true，说明操作需要被取消
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("第一个任务已经被取消");
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            Console.WriteLine("第一个任务已经成功完成");
        }

        static void AsyncOperation2(CancellationToken token)
        {
            try
            {
                Console.WriteLine("开始第二个任务");
                for (int i = 0; i < 5; i++)
                {
                    // 如果已取消，则抛出异常
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
                Console.WriteLine("第二个任务已经成功完成");
            }
            catch (OperationCanceledException)
            {
                // 在主处理意外处理取消过程
                Console.WriteLine("第二个任务已经被取消");
            }
        }

        static void AsyncOperation3(CancellationToken token)
        {
            bool cancelltionFlag = false;
            // 注册一个回调函数
            // 当操作被取消时，线程池将调用该回调函数
            token.Register(() => cancelltionFlag = true);
            Console.WriteLine("开始第三个任务");
            for (int i = 0; i < 5; i++)
            {
                if (cancelltionFlag)
                {
                    Console.WriteLine("第三个任务已经被取消");
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            Console.WriteLine("第三个任务已经成功完成");
        }
    }
}
