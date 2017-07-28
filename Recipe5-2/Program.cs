using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe5_2
{
    class Program
    {
        /// <summary>
        /// 在lambda表达式中使用await操作符
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 由于不能在main方法中使用async，所以将异步函数移到了AsynchronousProcessing方法中
            Task t = AsynchronousProcessing();
            t.Wait();

            Console.ReadLine();
        }

        static async Task AsynchronousProcessing()
        {
            // 由于任何lambda表达式的类型都不能通过lambda自身来推断，
            // 所以不得不显式向C#编译器指定它的类型
            // 这里lambda的参数类型为string，返回值类型为Task<string>
            // lambda表达式中虽然只返回了一个string，但是因为使用async操作符，会自动封装成Task<string>
            Func<string, Task<string>> asyncLambda = async name =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                return $"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. " +
                $"Is thread pool thread：{Thread.CurrentThread.IsThreadPoolThread}";
            };

            // 使用await操作符获取异步操作的返回值
            string result = await asyncLambda("async lambda");
            Console.WriteLine(result);
        }
    }
}
