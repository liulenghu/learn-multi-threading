using System;
using System.Linq;
using System.Reactive.Linq;

namespace Recipe8_5
{
    class Program
    {
        /// <summary>
        /// 对可观察的集合使用LINQ查询
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 计时器序列
            IObservable<long> sequence = Observable.Interval(TimeSpan.FromMilliseconds(50)).Take(21);
            // 偶数序列
            var evenNumbers = from n in sequence
                              where n % 2 == 0
                              select n;
            // 奇数序列
            var oddNumbers = from n in sequence
                            where n % 2 != 0
                            select n;
            // 连接序列
            var combine = from n in evenNumbers.Concat(oddNumbers)
                          select n;
            // 副作用序列（Do函数）
            var nums = (from n in combine
                        where n % 5 == 0
                        select n)
                       .Do(n => Console.WriteLine($"------Number {n} is processed in Do method"));

            // 以上测查询均是惰性的，只有订阅了其结果，查询才会运行
            using (var sub = OutputToConsole(sequence, 0))
            using (var sub2 = OutputToConsole(combine, 1))
            using (var sub3 = OutputToConsole(nums, 2))
            // using (var sub4 = OutputToConsole(evenNumbers, 4))
            // using (var sub5 = OutputToConsole(oddNumbers, 5))
            { 
                Console.WriteLine("Press Enter to finish the demo");
                Console.ReadLine();
            }
        }

        static IDisposable OutputToConsole<T>(IObservable<T> sequence, int innerLevel)
        {
            string delimiter = innerLevel == 0
                ? String.Empty
                : new string('-', innerLevel * 3);

            return sequence.Subscribe(
                obj => Console.WriteLine($"{delimiter}{obj}")
                , ex => Console.WriteLine($"Error: {ex.Message}")
                , () => Console.WriteLine($"{delimiter}Completed")
            );
        }
    }
}
