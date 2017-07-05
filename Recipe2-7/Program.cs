using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe2_7
{
    class Program
    {
        // Barrier类用于组织多个线程及时在某个时刻碰面
        // 其提供一个回调函数，每次线程调用了SignalAndWait方法后该回调函数会被执行
        static Barrier _barrier = new Barrier(2, b => Console.WriteLine($"结束阶段 {b.CurrentPhaseNumber + 1}"));

        static void Main(string[] args)
        {
            var t1 = new Thread(() => PlayMusic("吉他手", "弹一段Solo", 5));
            var t2 = new Thread(() => PlayMusic("歌手", "唱歌", 2));

            t1.Start();
            t2.Start();

            Console.ReadLine();
        }

        static void PlayMusic(string name, string message, int seconds)
        {
            for (int i = 1; i < 3; i++)
            {
                Console.WriteLine("----------------------------------------");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                Console.WriteLine($"{name} 开始 {message}");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                Console.WriteLine($"{name} 完成了 {message}");
                _barrier.SignalAndWait();
            }
        }
    }
}
