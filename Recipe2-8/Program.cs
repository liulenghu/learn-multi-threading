using System;
using System.Collections.Generic;
using System.Threading;

namespace Recipe2_8
{
    class Program
    {
        static ReaderWriterLockSlim _rw = new ReaderWriterLockSlim();
        static Dictionary<int, int> _items = new Dictionary<int, int>();

        static void Main(string[] args)
        {
            new Thread(Read) { IsBackground = true, Name = "Read Thread 1"}.Start();
            new Thread(Read) { IsBackground = true, Name = "Read Thread 2" }.Start();
            new Thread(Read) { IsBackground = true, Name = "Read Thread 3" }.Start();

            new Thread(() => Write("Write Thread 1")) { IsBackground = true }.Start();
            new Thread(() => Write("Write Thread 2")) { IsBackground = true }.Start();

            // 30s后主线程结束
            Thread.Sleep(TimeSpan.FromSeconds(30));
        }

        static void Read()
        {
            Console.WriteLine("读取字典中的内容");
            while (true)
            {
                try
                {
                    // 获取读锁（允许多个线程同时获取读锁）
                    _rw.EnterReadLock();
                    foreach (var key in _items)
                    {
                        // Console.WriteLine($"线程 {Thread.CurrentThread.Name} : {key}");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                    }
                }
                finally
                {
                    // 在finally中释放锁，确保锁最终会被释放
                    _rw.ExitReadLock();
                }
            }
        }

        static void Write(string threadName)
        {
            while (true)
            {
                try
                {
                    int newKey = new Random().Next(250);
                    // 获取可升级读锁
                    _rw.EnterUpgradeableReadLock();
                    if (!_items.ContainsKey(newKey))
                    {
                        try
                        {
                            // 等待所有的读锁释放后获取写锁，此时所有的获取读锁操作会被阻塞
                            _rw.EnterWriteLock();
                            _items[newKey] = 1;
                            Console.WriteLine($"新Key {newKey} 被 {threadName} 加入字典");
                        }
                        finally
                        {
                            // 在finally中释放锁，确保锁最终会被释放
                            _rw.ExitWriteLock();
                        }
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(0.1));
                }
                finally
                {
                    // 在finally中释放锁，确保锁最终会被释放
                    _rw.ExitUpgradeableReadLock();
                }
            }
        }
    }
}
