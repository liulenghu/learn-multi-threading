using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Recipe6_1
{
    class Program
    {
        const string Item = "Dictionary item";
        const int Iterations = 1000000;
        public static string CurrentItem;

        /// <summary>
        /// 使用ConcurrentDictionary
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var concurrentDictionary = new ConcurrentDictionary<int, string>();
            var dictionary = new Dictionary<int, string>();

            var sw = new Stopwatch();

            sw.Start();
            for (int i = 0; i < Iterations; i++)
            {
                lock (dictionary)
                {
                    dictionary[i] = Item;
                }
            }
            sw.Stop();
            Console.WriteLine($"写入使用粗粒度锁的字典: {sw.Elapsed}");

            sw.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                concurrentDictionary[i] = Item;
            }
            sw.Stop();
            Console.WriteLine($"写入一个ConcurrentDictionary（细粒度锁）: {sw.Elapsed}");

            sw.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                lock (dictionary)
                {
                    CurrentItem = dictionary[i];
                }
            }
            sw.Stop();
            Console.WriteLine($"使用粗粒度锁从字典中读取: {sw.Elapsed}");

            sw.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                CurrentItem = concurrentDictionary[i];
            }
            sw.Stop();
            Console.WriteLine($"从一个ConcurrentDictionary中读取: {sw.Elapsed}");


            Console.WriteLine();


            int taskSize = 10;
            Task[] ts = new Task[taskSize];
            for (int j = 0; j < taskSize; j++)
            {
                Task t = new Task(() => {
                    for (int i = 0; i < Iterations; i++)
                    {
                        lock (dictionary)
                        {
                            dictionary[i] = Item;
                        }
                    }
                });
                ts[j] = t;
            }
            var whenAllTask = Task.WhenAll(ts);
            sw.Restart();
            for (int i = 0; i < ts.Length; i++)
            {
                ts[i].Start();
            }
            whenAllTask.Wait();
            sw.Stop();
            Console.WriteLine($"【多线程】写入使用粗粒度锁的字典: {sw.Elapsed}");

            
            ts = new Task[taskSize];
            for (int j = 0; j < taskSize; j++)
            {
                Task t = new Task(() => {
                    for (int i = 0; i < Iterations; i++)
                    {
                        concurrentDictionary[i] = Item;
                    }
                });
                ts[j] = t;
            }
            whenAllTask = Task.WhenAll(ts);
            sw.Restart();
            for (int i = 0; i < ts.Length; i++)
            {
                ts[i].Start();
            }
            whenAllTask.Wait();
            sw.Stop();
            Console.WriteLine($"【多线程】写入一个ConcurrentDictionary（细粒度锁）: {sw.Elapsed}");
            

            ts = new Task[taskSize];
            for (int j = 0; j < taskSize; j++)
            {
                Task t = new Task(() => {
                    for (int i = 0; i < Iterations; i++)
                    {
                        lock (dictionary)
                        {
                            CurrentItem = dictionary[i];
                        }
                    }
                });
                ts[j] = t;
            }
            whenAllTask = Task.WhenAll(ts);
            sw.Restart();
            for (int i = 0; i < ts.Length; i++)
            {
                ts[i].Start();
            }
            whenAllTask.Wait();
            sw.Stop();
            Console.WriteLine($"【多线程】使用粗粒度锁从字典中读取: {sw.Elapsed}");
            

            ts = new Task[taskSize];
            for (int j = 0; j < taskSize; j++)
            {
                Task t = new Task(() => {
                    for (int i = 0; i < Iterations; i++)
                    {
                        CurrentItem = concurrentDictionary[i];
                    }
                });
                ts[j] = t;
            }
            whenAllTask = Task.WhenAll(ts);
            sw.Restart();
            for (int i = 0; i < ts.Length; i++)
            {
                ts[i].Start();
            }
            whenAllTask.Wait();
            sw.Stop();
            Console.WriteLine($"【多线程】从一个ConcurrentDictionary中读取: {sw.Elapsed}");
            
            Console.ReadLine();
        }
    }
}
