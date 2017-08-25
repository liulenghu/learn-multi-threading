using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Recipe8_6
{
    class Program
    {
        delegate string AsyncDelegate(string name);

        /// <summary>
        /// 使用Rx创建异步操作
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            IObservable<string> o = LongRunningOperationAsync("Task1");
            using (var sub = OutputToConsole(o))
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
            Console.WriteLine("--------------------");

            Task<string> t = LongRunningOperationTaskAsync("Task2");
            // 使用ToObservable方法将Task转换为Observable方法
            using (var sub = OutputToConsole(t.ToObservable()))
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
            Console.WriteLine("--------------------");
            
            // 将异步编程模块模式转换为Observable类
            AsyncDelegate asyncMethod = LongRunningOperation;
            Func<string, IObservable<string>> observableFactory = Observable.FromAsyncPattern<string, string>(asyncMethod.BeginInvoke, asyncMethod.EndInvoke);
            o = observableFactory("Task3");
            using (var sub = OutputToConsole(o))
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
            Console.WriteLine("--------------------");

            // 对Observable操作使用await
            o = observableFactory("Task4");
            AwaitOnObservable(o).Wait();
            Console.WriteLine("--------------------");

            // 把基于事件的异步模式直接转换为Observable类
            using (var timer = new System.Timers.Timer(1000))
            {
                var ot = Observable.FromEventPattern<ElapsedEventHandler, ElapsedEventArgs>(
                    h => timer.Elapsed += h
                    , h => timer.Elapsed -= h
                );
                timer.Start();

                using (var sub = OutputToConsole(ot))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
                Console.WriteLine("--------------------");
                timer.Stop();
            }

            Console.ReadLine();
        }

        static async Task<T> AwaitOnObservable<T>(IObservable<T> observable)
        {
            T obj = await observable;
            Console.WriteLine($"{obj}");
            return obj;
        }

        static Task<string> LongRunningOperationTaskAsync(string name)
        {
            return Task.Run(() => LongRunningOperation(name));
        }

        static IObservable<string> LongRunningOperationAsync(string name)
        {
            // Observable.Start 与 TPL中的Task.Run方法很相似。
            // 启动异步操作并返回同一个字符串结果，然后退出
            return Observable.Start(() => LongRunningOperation(name));
        }

        static string LongRunningOperation(string name)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return $"Task {name} is completed. Thread id {Thread.CurrentThread.ManagedThreadId}";
        }

        static IDisposable OutputToConsole(IObservable<EventPattern<ElapsedEventArgs>> sequence)
        {
            return sequence.Subscribe(
                obj => Console.WriteLine($"{obj.EventArgs.SignalTime}")
                , ex => Console.WriteLine($"Error: {ex.Message}")
                , () => Console.WriteLine("Completed")
            );
        }

        static IDisposable OutputToConsole<T>(IObservable<T> sequence)
        {
            return sequence.Subscribe(
                obj => Console.WriteLine($"{obj}")
                , ex => Console.WriteLine($"Error: {ex.Message}")
                , () => Console.WriteLine("Completed")
            );
        }
    }
}
