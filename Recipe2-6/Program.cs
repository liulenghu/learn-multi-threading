using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe2_6
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始两个操作");
            var t1 = new Thread(() => PerformOperation("操作1已经完成", 4));
            var t2 = new Thread(() => PerformOperation("操作2已经完成", 8));

            t1.Start();
            t2.Start();

            // 直到_countdown的计数变为0才会继续执行
            _countdown.Wait();
            Console.WriteLine("两个操作都已经完成");
            _countdown.Dispose();

            Console.ReadLine();
        }

        static CountdownEvent _countdown = new CountdownEvent(2);
        
        static void PerformOperation(string message, int seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine(message);
            // _countdown的计数减1
            _countdown.Signal();
        }
    }
}
