using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe2_9
{
    class Program
    {
        // volatile 关键字指出一个字段可能会被同时执行的多个线程修改。
        // 声明为 volatile 的字段不会被编译器和处理器优化为只能被单个线程访问。
        // 这确保了该字段总是最新的值
        static volatile bool _isCompleted = false;

        static void Main(string[] args)
        {
            var t1 = new Thread(UserModeWait);
            var t2 = new Thread(HybirdSpinWait);

            Console.WriteLine("执行用户模式等待");
            t1.Start();
            Thread.Sleep(20);
            // Thread.Sleep(TimeSpan.FromSeconds(20));
            _isCompleted = true;
            Thread.Sleep(TimeSpan.FromSeconds(1));
            _isCompleted = false;

            Console.WriteLine("执行混合模式等待");
            t2.Start();
            Thread.Sleep(5);
            // Thread.Sleep(TimeSpan.FromSeconds(20));
            _isCompleted = true;

            Console.ReadLine();
        }

        static void UserModeWait()
        {
            while (!_isCompleted)
            {
                // 这里会一直消耗CPU时间
                Console.Write(".");
            }
            Console.WriteLine();
            Console.WriteLine("等待已完成");
        }

        static void HybirdSpinWait()
        {
            // 使用SpinWait来使线程等待
            // 
            var w = new SpinWait();
            while (!_isCompleted)
            {
                w.SpinOnce();
                // 获取是否确保下次调用 System.Threading.SpinWait.SpinOnce 会产生处理器，同时触发强制的上下文切换。
                // false : 用户模式 不会发生上下文切换 但会浪费CPU时间
                // true : 内核模式 会发生上下文切换 但会节省CPU时间
                Console.WriteLine(w.NextSpinWillYield);
            }
            Console.WriteLine("等待已完成");
        }
    }
}
