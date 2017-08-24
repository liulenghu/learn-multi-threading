using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Recipe7_6
{
    class Program
    {
        /// <summary>
        /// 为PLINQ查询创建一个自定义的聚合器
        /// 统计集合中所有字母出现的频率
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var parallelQuery = from t in GetTypes().AsParallel()
                                select t;

            var parallelAggregator = parallelQuery.Aggregate(
                // 一个工厂类
                () => new ConcurrentDictionary<char, int>(),
                // 每个分区的聚合函数
                (taskTotal, item) => AccumulateLettersInformation(taskTotal, item),
                // 高阶聚合函数
                (total, taskTotal) => MergeAccululators(total, taskTotal),
                // 选择器函数（指定全局对象中我们需要的确切数据）
                total => total);

            Console.WriteLine();
            Console.WriteLine("There were the following letters in type names:");
            // 按字符出现的频率排序
            var orderedKeys = from k in parallelAggregator.Keys
                              orderby parallelAggregator[k] descending
                              select k;
            // 打印聚合结果
            foreach (var c in orderedKeys)
            {
                Console.WriteLine($"Letter '{c}' ---- {parallelAggregator[c]} times");
            }

            Console.ReadLine();
        }

        static ConcurrentDictionary<char, int> AccumulateLettersInformation(ConcurrentDictionary<char, int> taskTotal, string item)
        {
            foreach (var c in item)
            {
                if (taskTotal.ContainsKey(c))
                {
                    taskTotal[c] += 1;
                }
                else
                {
                    taskTotal[c] = 1;
                }
            }

            Console.WriteLine($"{item} type was aggregated on a thread id {Thread.CurrentThread.ManagedThreadId}");

            return taskTotal;
        }

        static ConcurrentDictionary<char, int> MergeAccululators(ConcurrentDictionary<char, int> total, ConcurrentDictionary<char, int> taskTotal)
        {
            foreach (var key in taskTotal.Keys)
            {
                if (total.ContainsKey(key))
                {
                    total[key] += taskTotal[key];
                } else
                {
                    total[key] = 1;
                }
            }

            Console.WriteLine("---");
            Console.WriteLine($"Total aggregate value was calculated on a thread in {Thread.CurrentThread.ManagedThreadId}");

            return total;
        }

        static IEnumerable<string> GetTypes()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetExportedTypes());
            return from type in types
                   where type.Name.StartsWith("Web")
                   select type.Name;
        }
    }
}
