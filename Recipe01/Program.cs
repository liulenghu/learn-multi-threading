using System;
using System.Threading;

namespace Recipe01
{
    /// <summary>
    /// 创建线程
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Thread t = new Thread(PrintNumbers);
            t.Start();
            PrintNumbers();

            Console.ReadLine();
        }

        static void PrintNumbers()
        {
            Console.WriteLine("Starting...");
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(i);
            }
        }
    }
}
