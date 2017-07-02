using System;
using System.Threading;

namespace Recipe2_3
{
    class Program
    {
        // 构造函数中指定允许的并发线程数量
        // 这里指定了并发数为4个线程
        // 当有4个线程获取了资源后，其它的线程需要等待
        static SemaphoreSlim _samphore = new SemaphoreSlim(4);

        static void Main(string[] args)
        {
            for (int i = 0; i < 6; i++)
            {
                string threadName = "Thread " + i;
                int secondsToWait = 2 + 2 * i;
                var t = new Thread(() => AccessDatabase(threadName, secondsToWait));
                t.Start();
            }
            Console.ReadLine();
        }

        static void AccessDatabase(String name, int secondes)
        {
            Console.WriteLine($"{name} wait to access a database");
            // 调用Wait方法获取资源，当超过最大指定并发数量时，则需要等待其它线程释放资源
            _samphore.Wait();
            Console.WriteLine($"{name} was granted an access to a database");
            Thread.Sleep(TimeSpan.FromSeconds(secondes));
            Console.WriteLine($"{name} is completed");
            // 调用Release方法释放资源
            _samphore.Release();
        }
    }
}
