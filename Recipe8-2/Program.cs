using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace Recipe8_2
{
    class Program
    {
        /// <summary>
        /// 编写自定义的可观察对象
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var observer = new CustomObserver();

            var goodObservable = new CustomSequence(new[] { 1, 2, 3, 4, 5 });
            var badObservable = new CustomSequence(null);

            // 同步订阅
            using (IDisposable subscription = goodObservable.Subscribe(observer))
            {
            }

            // 异步订阅
            using (IDisposable subscription = goodObservable.SubscribeOn(TaskPoolScheduler.Default).Subscribe(observer))
            {
                // 延迟一段时间等待异步任务完成
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                Console.WriteLine("Press Enter to continue");
                Console.ReadLine();
            }

            // 异步订阅异常时
            using (IDisposable subscription = badObservable.SubscribeOn(TaskPoolScheduler.Default).Subscribe(observer))
            {
                // 延迟一段时间等待异步任务完成
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                Console.WriteLine("Press Enter to continue");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// 自定义观察者
        /// </summary>
        class CustomObserver : IObserver<int>
        {
            public void OnCompleted()
            {
                Console.WriteLine("Completed");
            }

            public void OnError(Exception error)
            {
                Console.WriteLine($"Error: {error.Message}");
            }

            public void OnNext(int value)
            {
                Console.WriteLine($"Next value: {value}; Thread id: {Thread.CurrentThread.ManagedThreadId}");
            }
        }

        /// <summary>
        /// 自定义可观察对象
        /// </summary>
        class CustomSequence : IObservable<int>
        {
            private readonly IEnumerable<int> _numbers;

            public CustomSequence(IEnumerable<int> numbers)
            {
                _numbers = numbers;
            }

            public IDisposable Subscribe(IObserver<int> observer)
            {
                foreach (var number in _numbers)
                {
                    observer.OnNext(number);
                }
                observer.OnCompleted();
                return Disposable.Empty;
            }
        }
    }
}
