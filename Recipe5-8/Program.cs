using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe5_8
{
    class Program
    {
        /// <summary>
        /// 自定义awaitable类型
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
            var sync = new CustomAwaitable(true);
            string result = await sync;
            // Completed synchronously
            Console.WriteLine(result);

            var async = new CustomAwaitable(false);
            result = await async;
            // Task is running on a thread id 3. Is thread pool thread: True
            Console.WriteLine(result);
        }
        
        /// <summary>
        /// 类型t
        /// </summary>
        class CustomAwaitable
        {
            private readonly bool _completeSynchronously;

            public CustomAwaitable(bool completeSynchronously)
            {
                _completeSynchronously = completeSynchronously;
            }

            /// <summary>
            /// t有一个名为GetAwaiter的可访问的实例或扩展方法
            /// </summary>
            /// <returns>类型A</returns>
            public CustomAwaiter GetAwaiter()
            {
                return new CustomAwaiter(_completeSynchronously);
            }
        }

        /// <summary>
        /// 类型A 实现了 System.Runtime.CompilerServices.INotifyCompletion 接口
        /// </summary>
        class CustomAwaiter : INotifyCompletion
        {
            private string _result = "Completed synchronously";
            private readonly bool _completeSynchronously;

            /// <summary>
            /// A有一个可访问的、可读的类型为bool的实例属性IsCompleted
            /// 如果IsCompleted属性返回true，则只需同步调用GetResult方法
            /// </summary>
            public bool IsCompleted => _completeSynchronously;

            public CustomAwaiter(bool completeSynchronously)
            {
                _completeSynchronously = completeSynchronously;
            }

            /// <summary>
            /// A有一个名为GetResult的可访问的实例方法，该方法没有任何参数和类型参数。
            /// </summary>
            /// <returns></returns>
            public string GetResult()
            {
                return _result;
            }

            public void OnCompleted(Action continuation)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    _result = GetInfo();
                    continuation?.Invoke();
                });
            }

            private string GetInfo()
            {
                return $"Task is running on a thread id {Thread.CurrentThread.ManagedThreadId}. " +
                    $"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
            }
        }
    }
}
