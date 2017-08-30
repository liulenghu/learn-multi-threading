using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe10_1
{
    class Program
    {
        /// <summary>
        /// 实现惰性求值的共享状态
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var t = ProcessAsynchronously();
            t.Wait();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

        static async Task ProcessAsynchronously()
        {
            // 不安全的对象，构造方法会被调用了多次，
            // 并且不同的线程中值是不同的
            var unsafeState = new UnsafeState();
            Task[] tasks = new Task[4];

            for (int i = 0; i < 4; i++)
            {
                tasks[i] = Task.Run(() => Worker(unsafeState));
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("------------------------------");

            // 使用双重锁定模式
            var firstState = new DoubleCheckedLocking();
            for (int i = 0; i < 4; i++)
            {
                tasks[i] = Task.Run(() => Worker(firstState));
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("------------------------------");

            // 使用LazyInitializer.EnsureInitialized方法
            var secondState = new BCLDoubleChecked();
            for (int i = 0; i < 4; i++)
            {
                tasks[i] = Task.Run(() => Worker(secondState));
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("------------------------------");

            // 使用Lazy<T>类型
            var lazy = new Lazy<ValueToAccess>(Compute);
            var thirdState = new LazyWrapper(lazy);
            for (int i = 0; i < 4; i++)
            {
                tasks[i] = Task.Run(() => Worker(thirdState));
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("------------------------------");

            // 使用LazyInitializer.EnsureInitialized方法的一个不使用锁的重载
            var fourthState = new BCLThreadSafeFactory();
            for (int i = 0; i < 4; i++)
            {
                tasks[i] = Task.Run(() => Worker(fourthState));
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("------------------------------");
        }

        private static void Worker(IHasValue state)
        {
            Console.WriteLine($"Worker runs on thread id {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"State value: {state.Value.Text}");
        }

        class UnsafeState : IHasValue
        {
            private ValueToAccess _value;

            // 不安全的对象，构造方法Compute会被调用多次
            public ValueToAccess Value => _value ?? (_value = Compute());
        }

        class DoubleCheckedLocking : IHasValue
        {
            private readonly object _syncRoot = new object();
            private volatile ValueToAccess _value;
            
            public ValueToAccess Value
            {
                get
                {
                    // 使用锁及双重验证，确保构造方法Compute仅执行一次
                    if (_value == null)
                    {
                        lock (_syncRoot)
                        {
                            if (_value == null)
                            {
                                _value = Compute();
                            }
                        }
                    }
                    return _value;
                }
            }
        }

        class BCLDoubleChecked : IHasValue
        {
            private object _syncRoot = new object();
            private ValueToAccess _value;
            private bool _initialized;

            // 使用LazyInitializer.EnsureInitialized方法初始化
            // 该方法内部实现了双重锁定模式
            public ValueToAccess Value => LazyInitializer.EnsureInitialized(ref _value, ref _initialized, ref _syncRoot, Compute);
        }

        class BCLThreadSafeFactory : IHasValue
        {
            private ValueToAccess _value;

            // 使用LazyInitializer.EnsureInitialized方法初始化
            // 这个构造函数的重载没有使用锁，会导致初始化方法Compute被执行多次，但是结果的对象仍然是线程安全的
            public ValueToAccess Value => LazyInitializer.EnsureInitialized(ref _value, Compute);
        }

        class LazyWrapper : IHasValue
        {
            // 使用Lazy<T>类型
            // 效果同使用LazyInitializer一样
            // 区别是LazyInitializer是静态类，不需要初始化
            private readonly Lazy<ValueToAccess> _value;

            public LazyWrapper(Lazy<ValueToAccess> value)
            {
                _value = value;
            }

            public ValueToAccess Value => _value.Value;
        }

        static ValueToAccess Compute()
        {
            Console.WriteLine($"The value is being constructed on a thread id {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return new ValueToAccess($"Constructed on thread id {Thread.CurrentThread.ManagedThreadId}");
        }

        interface IHasValue
        {
            ValueToAccess Value { get; }
        }

        class ValueToAccess
        {
            private readonly string _text;
            public ValueToAccess(string text)
            {
                _text = text;
            }
            public string Text => _text;
        }
    }
}
