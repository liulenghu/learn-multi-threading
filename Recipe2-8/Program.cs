using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

            Thread.Sleep(TimeSpan.FromSeconds(30));
            Console.ReadLine();
        }

        static void Read()
        {
            Console.WriteLine("读取字典中的内容");
            while (true)
            {
                try
                {
                    _rw.EnterReadLock();
                    foreach (var key in _items)
                    {
                        Console.WriteLine($"线程 {Thread.CurrentThread.Name} : {key}");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                    }
                }
                finally
                {
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
                    _rw.EnterUpgradeableReadLock();
                    if (!_items.ContainsKey(newKey))
                    {
                        try
                        {
                            _rw.EnterWriteLock();
                            _items[newKey] = 1;
                            Console.WriteLine($"新Key {newKey} 被 {threadName} 加入字典");
                        }
                        finally
                        {
                            _rw.ExitWriteLock();
                        }
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(0.1));
                }
                finally
                {
                    _rw.ExitUpgradeableReadLock();
                }
            }
        }
    }
}
