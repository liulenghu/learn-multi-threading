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
        static volatile bool _isCompleted = false;

        static void Main(string[] args)
        {
            var t1 = new Thread(UserModeWait);
            var t2 = new Thread(HybirdSpinWait);

            Console.WriteLine("执行用户模式等待");
            t1.Start();
            Thread.Sleep(20);

        }

        static void UserModeWait()
        {
            while (!_isCompleted)
            {
                Console.Write(".");
            }
            Console.WriteLine();
            Console.WriteLine("等待已完成");
        }

        static void HybirdSpinWait()
        {
            var w = new SpinWait();
            while (!_isCompleted)
            {
                w.SpinOnce();

                Console.WriteLine(w.NextSpinWillYield);
            }
            Console.WriteLine("等待已完成");
        }
    }
}
