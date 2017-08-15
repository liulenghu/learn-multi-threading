using ImpromptuInterface;
using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe5_9
{
    class Program
    {
        /// <summary>
        /// 对动态类型使用await
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
            // 通过参数指定IsCompleted为true，只需同步调用GetResult方法
            string result = await GetDynamicAwaitableObject(true);
            Console.WriteLine(result);

            // 通过参数指定IsCompleted为false，则会先执行OnCompleted方法
            result = await GetDynamicAwaitableObject(false);
            Console.WriteLine(result);
        }

        static dynamic GetDynamicAwaitableObject(bool completeSynchronously)
        {
            // ExpandoObject类型可在运行时动态添加和删除其成员的对象
            dynamic result = new ExpandoObject(); // 类型t
            dynamic awaiter = new ExpandoObject(); // 类型A

            awaiter.Message = "Completed synchronously";
            awaiter.IsCompleted = completeSynchronously;
            awaiter.GetResult = (Func<string>)(() => awaiter.Message);

            awaiter.OnCompleted = (Action<Action>)(callback => ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
                awaiter.Message = GetInfo();
                callback?.Invoke();
            }));

            // 使用Impromptu.ActLike方法动态的创建代理对象，该对象将实现任何需要的接口
            IAwaiter<string> proxy = Impromptu.ActLike(awaiter);

            // t有一个名为GetAwaiter的可访问的实例或扩展方法
            result.GetAwaiter = (Func<dynamic>)(() => proxy);

            return result;
        }

        static string GetInfo()
        {
            return $"Task is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
        }
    }

    public interface IAwaiter<T> : INotifyCompletion
    {
        bool IsCompleted { get; }

        T GetResult();
    }
}
