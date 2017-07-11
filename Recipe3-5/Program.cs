using System;
using System.Threading;

namespace Recipe3_5
{
    class Program
    {
        /// <summary>
        /// 在线程中使用等待事件处理器及超时
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            RunOperations(TimeSpan.FromSeconds(5));
            RunOperations(TimeSpan.FromSeconds(7));
        }

        static void RunOperations(TimeSpan workerOperationTimeout)
        {
            using (var evt = new ManualResetEvent(false))
            using (var cts = new CancellationTokenSource())
            {
                Console.WriteLine($"注册一个超时操作...");
                // 该方法允许我们将回调函数放入线程池中的队列中。
                // 当提供的等待时间处理器收到信号或发生超时时，该回调函数将被调用。
                // 第一个参数是等待对象（System.Threading.WaitHandle）
                // 第二个参数是回调函数
                // 第四个参数是超时事件
                // 第五个参数为true，表示仅执行一次
                var worker = ThreadPool.RegisterWaitForSingleObject(evt,
                    (state, isTimeOut) => WorkerOperationWait(cts, isTimeOut),
                    null,
                    workerOperationTimeout,
                    true);

                Console.WriteLine("开始执行一个长操作");
                ThreadPool.QueueUserWorkItem(_ => WorkerOperation(cts.Token, evt));

                Thread.Sleep(workerOperationTimeout.Add(TimeSpan.FromSeconds(2)));
                worker.Unregister(evt);
            }
        }

        static void WorkerOperation(CancellationToken token, ManualResetEvent evt)
        {
            for (int i = 0; i < 6; i++)
            {
                // 判断token是否已取消
                if (token.IsCancellationRequested)
                {
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            // 6秒后发送事件结束信号
            evt.Set();
        }

        static void WorkerOperationWait(CancellationTokenSource cts, bool isTimeOut)
        {
            if (isTimeOut)
            {
                // 如果操作超时，则发送取消信号
                cts.Cancel();
                Console.WriteLine("操作超时且被取消");
            } else
            {
                Console.WriteLine("操作执行成功");
            }
        }
    }
}
