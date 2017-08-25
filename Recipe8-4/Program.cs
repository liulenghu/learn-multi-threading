using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace Recipe8_4
{
    class Program
    {
        /// <summary>
        /// 创建可观察的对象
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("使用值创建Observable方法");
            Console.WriteLine("Observable.Return(0)");
            IObservable<int> o = Observable.Return(0);
            using (var sub = OutputToConsole(o)) ;
            Console.WriteLine("--------------------");

            Console.WriteLine("不使用值创建Observable方法");
            Console.WriteLine("Observable.Empty<int>()");
            o = Observable.Empty<int>();
            using (var sub = OutputToConsole(o)) ;
            Console.WriteLine("--------------------");

            Console.WriteLine("通过Observable.Throw触发OnError处理器");
            Console.WriteLine("Observable.Throw<int>( new Exception())");
            o = Observable.Throw<int>( new Exception());
            using (var sub = OutputToConsole(o)) ;
            Console.WriteLine("--------------------");

            Console.WriteLine("使用Observable.Repeat创建无尽序列");
            Console.WriteLine("Observable.Repeat(42)");
            o = Observable.Repeat(42);
            using (var sub = OutputToConsole(o.Take(5))) ;
            Console.WriteLine("--------------------");

            Console.WriteLine("使用Observable.Range创建一组值");
            Console.WriteLine("Observable.Range(0, 10)");
            o = Observable.Range(0, 10);
            using (var sub = OutputToConsole(o)) ;
            Console.WriteLine("--------------------");

            Console.WriteLine("Observable.Create方法支持很多的自定义场景");
            // Create方法接受一个函数，该函数接受一个观察者实例，并且返回IDisposable对象来代表订阅者
            o = Observable.Create<int>(ob =>
            {
                for (int i = 0; i < 10; i++)
                {
                    ob.OnNext(i);
                }
                return Disposable.Empty;
            });
            using (var sub = OutputToConsole(o)) ;
            Console.WriteLine("--------------------");

            Console.WriteLine("Observable.Generate是另一个创建自定义序列的方式");
            o = Observable.Generate(
                0 // install state 初始值
                , i => i < 5 // while this is true we continue the sequence 一个断言，用来决定是否需要生成更多元素或者完成序列
                , i => ++i // iteration 迭代逻辑
                , i => i * 2  // selecting result 选择器函数，允许我们定制化结果
            );
            using (var sub = OutputToConsole(o)) ;
            Console.WriteLine("--------------------");

            Console.WriteLine("Interval会以TimeSpan间隔产生计时器标记事件");
            Console.WriteLine("Observable.Interval(TimeSpan.FromSeconds(1))");
            IObservable<long> ol = Observable.Interval(TimeSpan.FromSeconds(1));
            using (var sub = OutputToConsole(ol))
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
            Console.WriteLine("--------------------");


            Console.WriteLine("Timer指定了启动时间");
            Console.WriteLine("Observable.Timer(DateTimeOffset.Now.AddSeconds(2))");
            ol = Observable.Timer(DateTimeOffset.Now.AddSeconds(2));
            using (var sub = OutputToConsole(ol))
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
            Console.WriteLine("--------------------");

            Console.ReadLine();
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
