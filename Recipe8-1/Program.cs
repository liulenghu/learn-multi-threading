using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace Recipe8_1
{
    class Program
    {
        /// <summary>
        /// 将普通集合转换为异步的可观察集合
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            foreach (int i in EnumerableEventSequence())
            {
                Console.Write(i);
            }

            Console.WriteLine();
            Console.WriteLine("IEnumerable");

            // 使用ToObservable扩展方法，把可枚举的集合转换为可观察的集合
            IObservable<int> o = EnumerableEventSequence().ToObservable();
            // 使用Subscribe方法订阅该可观察集合的更新
            using (IDisposable subscription = o.Subscribe(Console.Write))
            {
                Console.WriteLine();
                Console.WriteLine("IObservable");
            }

            // 使用SubscribeOn方法使其异步化，并提供其TPL任务池调度程序。
            // 这可以使UI在集合更新时仍保持响应并做些其它的事情。
            o = EnumerableEventSequence().ToObservable()
                .SubscribeOn(TaskPoolScheduler.Default);
            using (IDisposable subscription = o.Subscribe(Console.Write))
            {
                // 因为是异步的，所以会先执行using块中的代码
                Console.WriteLine();
                Console.WriteLine("IObservable async");
                // 如果注释掉该行代码，会导致主线程立即结束，从而异步处理也会结束
                Console.ReadLine();
            }
        }

        /// <summary>
        /// 模拟一个效率不高的可枚举的集合
        /// </summary>
        /// <returns></returns>
        static IEnumerable<int> EnumerableEventSequence()
        {
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                yield return i;
            }
        }
    }
}
