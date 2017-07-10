using System;
using System.Threading;

namespace Recipe3_2
{
    class Program
    {
        /// <summary>
        /// 向线程池中放入异步操作
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            const int x = 1;
            const int y = 2;
            const string lambdaState = "lambda state 2";

            // 使用QueueUserWorkItem方法将AsyncOperation方法放到线程池中
            ThreadPool.QueueUserWorkItem(AsyncOperation);
            Thread.Sleep(TimeSpan.FromSeconds(3));

            // 带参数的调用（参数值传递到AsyncOperation方法的参数object state）
            ThreadPool.QueueUserWorkItem(AsyncOperation, "async state");
            // 使线程睡眠，从而让线程池拥有为新操作重用线程的可能性
            Thread.Sleep(TimeSpan.FromSeconds(3));

            // 使用lambda表达式放置到线程池
            ThreadPool.QueueUserWorkItem(state =>
            {
                Console.WriteLine($"操作状态：{state}");
                Console.WriteLine($"当前工作线程Id：{Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }, "lambda state");

            // 这个lambda表达式中使用了闭包机制，从而无须传递lambda表达式的状态
            // 闭包更灵活，允许我们向异步方法传递一个以上的对象，而且这些对象具有静态类型
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Console.WriteLine($"操作状态：{x + y}, {lambdaState}");
                Console.WriteLine($"当前工作线程Id：{Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(TimeSpan.FromSeconds(2));
            });

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        private static void AsyncOperation(object state)
        {
            Console.WriteLine($"操作状态：{state ?? "(null)"}");
            Console.WriteLine($"当前工作线程Id：{Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }
    }
}
