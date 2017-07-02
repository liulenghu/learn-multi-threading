using System;
using System.Threading;

namespace Recipe9
{
    class Program
    {
        /// <summary>
        /// 使用C#的lock关键字
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Incorrect counter");

            var c = new Counter();

            var t1 = new Thread(() => TestCounter(c));
            var t2 = new Thread(() => TestCounter(c));
            var t3 = new Thread(() => TestCounter(c));
            t1.Start();
            t2.Start();
            t3.Start();
            t1.Join();
            t2.Join();
            t3.Join();

            // 多个线程中存在竞争条件（race condition）
            // 由于++并不是一个线程安全的操作，所以输出结果一般都不为0
            Console.WriteLine($"Total count: {c.Count}");
            Console.WriteLine("----------------------------------------");

            Console.WriteLine("Correct counter");

            var c1 = new CounterWithLock();

            t1 = new Thread(() => TestCounter(c1));
            t2 = new Thread(() => TestCounter(c1));
            t3 = new Thread(() => TestCounter(c1));
            t1.Start();
            t2.Start();
            t3.Start();
            t1.Join();
            t2.Join();
            t3.Join();
            Console.WriteLine($"Total count: {c1.Count}");

            Console.ReadLine();

        }

        static void TestCounter(CounterBase c)
        {
            for (int i = 0; i < 100000; i++)
            {
                c.Increment();
                c.Decrement();
            }
        }

        class Counter : CounterBase
        {
            public int Count { get; private set; }

            public override void Decrement()
            {
                Count++;
            }

            public override void Increment()
            {
                Count--;
            }
        }

        class CounterWithLock : CounterBase
        {
            // 如果锁定了一个对象，需要访问该对象的所有其他线程则会处于阻塞状态，并等待直到该对象解除锁定。
            // 这可能会导致严重的性能问题。
            private readonly object _ayncRoot = new object();

            public int Count { get; private set; }

            public override void Decrement()
            {
                lock(_ayncRoot)
                {
                    Count++;
                }
            }

            public override void Increment()
            {
                lock (_ayncRoot)
                {
                    Count--;
                }
            }
        }


        abstract class CounterBase
        {
            public abstract void Increment();

            public abstract void Decrement();
        }
    }
}
