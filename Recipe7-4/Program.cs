using System;
using System.Collections.Generic;
using System.Linq;

namespace Recipe7_4
{
    class Program
    {
        /// <summary>
        /// 处理PLINQ查询中的异常
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            IEnumerable<int> numbers = Enumerable.Range(-5, 10);

            Console.WriteLine("执行顺序的LINQ查询");
            var query = from number in numbers
                        select 100 / number;
            try
            {
                foreach (var n in query)
                {
                    Console.WriteLine(n);
                }
            }
            catch (DivideByZeroException)
            {
                Console.WriteLine("被0除！");
            }
            
            Console.WriteLine("---");
            Console.WriteLine();

            Console.WriteLine("执行并行的LINQ查询");
            var parallelQuery = from number in numbers.AsParallel()
                                select 100 / number;
            try
            {
                parallelQuery.ForAll(Console.WriteLine);
            }
            catch (DivideByZeroException)
            {
                Console.WriteLine("被0除！ - 通常的异常处理");
            }
            catch (AggregateException e)
            {
                e.Flatten().Handle(ex => {
                    if (ex is DivideByZeroException)
                    {
                        Console.WriteLine("被0除！ - Aggregate异常处理");
                        return true;
                    }
                    return false;
                });
            }
            Console.WriteLine("---");

            Console.ReadLine();
        }
    }
}
