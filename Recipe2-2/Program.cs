using System;
using System.Threading;

namespace Recipe2_2
{
    class Program
    {
        static void Main(string[] args)
        {
            const string MutexName = "CSharp Threading Cookbook";
            // Mutex是一种原始的同步方式，其只对一个线程授予对共享资源的独占访问。
            // 具名的互斥量是全局的操作系统对象，务必正确关闭互斥量。
            // 最好是使用using代码快来包裹互斥对象。
            using (var m = new Mutex(false, MutexName))
            {
                if (!m.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    Console.WriteLine("Second instance is running");
                    Console.ReadLine();
                } else
                {
                    Console.WriteLine("Running");
                    Console.ReadLine();
                    m.ReleaseMutex();
                }
            }
        }
    }
}
