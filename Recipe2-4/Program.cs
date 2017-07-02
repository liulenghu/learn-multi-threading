using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe2_4
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = new Thread(() => Process(10));
            t.Start();

            Console.WriteLine("Waiting for another thread to complete work");
            _workerEvent.WaitOne();
            Console.WriteLine("First operation is completed!");
            Console.WriteLine("Performing an operation on a main thread");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            _mainEvent.Set();
            Console.WriteLine("Now running the second operation on a second thread");
            _workerEvent.WaitOne();
            Console.WriteLine("Second operation is completed");
            Console.ReadLine();
        }

        // 参数为false，定义了初始状态为unsignaled
        // 这意味着任何线程调用这个对象的WaitOne方法将会被阻塞，直到调用了Set方法
        private static AutoResetEvent _workerEvent = new AutoResetEvent(false);
        private static AutoResetEvent _mainEvent = new AutoResetEvent(false);
        // 如果参数为true，则初始状态为signaled，如果线程调用WaitOne方法则会被立即处理，然后事件状态自动变为unsignaled，
        // 所以需要对该实例调用一次Set方法，以便让其他的线程对该实例调用WaitOne方法从而继续执行。

        static void Process(int seconds)
        {
            Console.WriteLine("Starting a long running work...");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine("Work is done!");
            _workerEvent.Set();
            Console.WriteLine("Waiting for a main thread to complete its work");
            _mainEvent.WaitOne();
            Console.WriteLine("Starting second operation...");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine("Work is done!");
            _workerEvent.Set();
        }
    }
}
