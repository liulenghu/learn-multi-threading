using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe2_5
{
    class Program
    {
        static void Main(string[] args)
        {
            var t1 = new Thread(() => TravelThroughGates("Thread 1", 5));
            var t2 = new Thread(() => TravelThroughGates("Thread 2", 6));
            var t3 = new Thread(() => TravelThroughGates("Thread 3", 12));
            t1.Start();
            t2.Start();
            t3.Start();
            Thread.Sleep(TimeSpan.FromSeconds(6));
            Console.WriteLine("门打开啦!");
            _mainEvent.Set();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            _mainEvent.Reset();
            Console.WriteLine("门关上啦！");
            Thread.Sleep(TimeSpan.FromSeconds(10));
            Console.WriteLine("门又打开啦！");
            _mainEvent.Set();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine("门又关上啦!");
            _mainEvent.Reset();
            Console.ReadLine();

        }

        static ManualResetEventSlim _mainEvent = new ManualResetEventSlim(false);

        static void TravelThroughGates(string threadName, int seconds)
        {
            Console.WriteLine($"{threadName} 睡眠 {seconds} 秒");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine($"{threadName} 等待门打开!");
            _mainEvent.Wait();
            Console.WriteLine($"{threadName} 已进门!");
        }
    }
}
