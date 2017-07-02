using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe10
{
    class Program
    {
/// <summary>
/// 使用Monitor类锁定资源
/// </summary>
/// <param name="args"></param>
static void Main(string[] args)
{
    object lock1 = new object();
    object lock2 = new object();

    new Thread(() => LockTooMuch(lock1, lock2)).Start();

    lock (lock2)
    {
        Thread.Sleep(TimeSpan.FromSeconds(1));
        Console.WriteLine("Monitor.TyrEnter allows not to get stuck, returning false after a specified timeout is elapased");
        // Monitor.TyrEnter方法接收一个超时参数。
        // 如果在能够获取被lock保护的资源之前，超时时间过期，则该方法会返回false。
        if (Monitor.TryEnter(lock1, TimeSpan.FromSeconds(5)))
        {
            Console.WriteLine("Acquired a protected resource succesfully");
        } else
        {
            Console.WriteLine("Timeout acquiring a resource!");
        }
    }

    new Thread(() => LockTooMuch(lock1, lock2)).Start();

    lock (lock2)
    {
        Console.WriteLine("This will be a deadlock");
        Thread.Sleep(TimeSpan.FromSeconds(1));
        lock (lock1)
        {
            Console.WriteLine("Acquired a protected resource succesfully");
        }
    }
}

static void LockTooMuch(object lock1, object lock2)
{
    lock(lock1)
    {
        Thread.Sleep(TimeSpan.FromSeconds(1));
        lock (lock2)
        {
        }
    }
}
    }
}
