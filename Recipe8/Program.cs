using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe8
{
    class Program
    {
        /// <summary>
        /// 向线程传递参数
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 通过类的实例化传递参数
            var sample = new ThreadSample(10);
            var threadOne = new Thread(sample.CountNumbers);
            threadOne.Name = "ThreadOne";
            threadOne.Start();
            threadOne.Join();

            Console.WriteLine("--------------------------------------");

            // 通过Start方法传参
            var threadTwo = new Thread(Count);
            threadTwo.Name = "ThreadTwo";
            threadTwo.Start(8);
            threadTwo.Join();

            Console.WriteLine("--------------------------------------");

            // 通过lambda表达式  传参
            var threadThree = new Thread(() => CountNumbers(12));
            threadThree.Start();
            threadThree.Join();

            Console.WriteLine("--------------------------------------");

            // 使用lambda表达式引用另一个C#对象的方法被称为闭包。
            // 当在lambda表达式中使用任何局部变量时，C#会生成一个类，并将该变量作为该类的一个属性。
            // 所以实际上该方法与threadOne中使用的一样，但是我们无需定义该类，C#编译器会自动帮我们实现
            // 如果在多个lambda表达式中使用相同的变量，它们会共享该变量值。
            int i = 10;
            var threadFour = new Thread(() => PrintNumber(i));
            i = 20;
            var threadFive = new Thread(() => PrintNumber(i));
            threadFour.Start(); // print 20
            threadFive.Start(); // print 20
            // 如果在线程启动后再更改i的值，则不会影响到已经启动的线程。

            Console.ReadLine();
        }

        static void Count(object iterations)
        {
            CountNumbers((int)iterations);
        }

        private static void CountNumbers(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine($"{Thread.CurrentThread.Name} prints {i}");
            }
        }

        static void PrintNumber(int number)
        {
            Console.WriteLine(number);
        }

        class ThreadSample
        {
            private readonly int _iterations;

            public ThreadSample(int iterations)
            {
                _iterations = iterations;
            }

            public void CountNumbers()
            {
                for (int i = 0; i < _iterations; i++)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    Console.WriteLine($"{Thread.CurrentThread.Name} prints {i}");
                }
            }
        }
    }
}
