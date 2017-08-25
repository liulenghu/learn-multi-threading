using System;
using System.Reactive.Subjects;
using System.Threading;

namespace Recipe8_3
{
    class Program
    {
        /// <summary>
        /// 使用 Subject
        /// Subject代表了IObservable和IObserver这两个接口的实现
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 一旦订阅了Subject，它就会把事件序列发送给订阅者
            Console.WriteLine("Subject");
            var subject = new Subject<string>();
            
            subject.OnNext("A"); // A在订阅之前，不会被打印
            using (var subscription = OutputToConsole(subject))
            {
                subject.OnNext("B");
                subject.OnNext("C");
                subject.OnNext("D");
                // 当调用OnCompleted或OnError方法时，事件序列传播会被停止
                subject.OnCompleted();
                // 事件传播停止之后的事件不会被打印
                subject.OnNext("Will not be printed out");
            }

            Console.WriteLine("ReplaySubject");
            // ReplaySubject可以缓存从广播开始的所有事件
            var replaySubject = new ReplaySubject<string>();

            replaySubject.OnNext("A");
            // 稍后订阅也可以获得之前的事件
            using (var subscription = OutputToConsole(replaySubject))
            {
                replaySubject.OnNext("B");
                replaySubject.OnNext("C");
                replaySubject.OnNext("D");
                replaySubject.OnCompleted();
            }
            Console.WriteLine("Buffered ReplaySubject");
            // 指定ReplaySubject缓存的大小
            // 参数2表示只可以缓存最后的2个事件
            var bufferedSubject = new ReplaySubject<string>(2);

            bufferedSubject.OnNext("A");
            bufferedSubject.OnNext("B");
            bufferedSubject.OnNext("C");
            using (var subscription = OutputToConsole(bufferedSubject))
            {
                bufferedSubject.OnNext("D");
                bufferedSubject.OnCompleted();
            }

            Console.WriteLine("Time window ReplaySubject");
            // 指定ReplaySubject缓存的事件
            // TimeSpan.FromMilliseconds(200) 表示只缓存200ms内发生的事件
            var timeSubject = new ReplaySubject<string>(TimeSpan.FromMilliseconds(200));
            timeSubject.OnNext("A");
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            timeSubject.OnNext("B");
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            timeSubject.OnNext("C");
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            using (var subscription = OutputToConsole(timeSubject))
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(300));
                timeSubject.OnNext("D");
                timeSubject.OnCompleted();
            }

            Console.WriteLine("AsyncSubject");
            // AsyncSubject类似于任务并行库中的Task类型
            // 它代表了单个异步操作
            // 如果有多个事件发布，它将等待事件序列完成，并把最后一个事件提供给订阅者
            var asyncSubject = new AsyncSubject<string>();

            asyncSubject.OnNext("A");
            using (var subscription = OutputToConsole(asyncSubject))
            {
                asyncSubject.OnNext("B");
                asyncSubject.OnNext("C");
                asyncSubject.OnNext("D");
                asyncSubject.OnCompleted();
            }

            Console.WriteLine("BehaviorSubject");
            // BehaviorSubject与ReplaySubject很相似，但它只缓存一个值
            // 并允许万一还没有发送任何通知时，指定一个默认值
            // 默认值会被自动替换为订阅前的最后一个事件
            var behaviorSubject = new BehaviorSubject<string>("Default");
            using (var subscription = OutputToConsole(behaviorSubject))
            {
                behaviorSubject.OnNext("B");
                behaviorSubject.OnNext("C");
                behaviorSubject.OnNext("D");
                behaviorSubject.OnCompleted();

            }

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
