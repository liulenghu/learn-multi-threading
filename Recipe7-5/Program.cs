using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Recipe7_5
{
    class Program
    {
        /// <summary>
        /// 管理PLINQ查询中的数据分区
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            var partitioner = new StringPartitioner(GetTypes());
            var parallelQuery = from t in partitioner.AsParallel()
                                //.WithDegreeOfParallelism(1)
                                select EmulateProcessing(t);

            parallelQuery.ForAll(PrintInfo);
            int count = parallelQuery.Count();
            sw.Stop();
            Console.WriteLine("------------------------------");
            Console.WriteLine($"Total items processed: {count}");
            Console.WriteLine($"Time elapsesd: {sw.Elapsed}");

            Console.ReadLine();
        }

        static void PrintInfo(string typeName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(150));
            Console.WriteLine($"{typeName} type was printed on a thread id {Thread.CurrentThread.ManagedThreadId}");
        }

        static string EmulateProcessing(string typeName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(150));
            Console.WriteLine($"{typeName} type was processed on a thread id {Thread.CurrentThread.ManagedThreadId}. Has {(typeName.Length % 2 == 0 ? "even" : "odd")} length.");
            return typeName;
        }

        static IEnumerable<string> GetTypes()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetExportedTypes());
            return from type in types
                   where type.Name.StartsWith("Web")
                   select type.Name;
        }

        /// <summary>
        /// 自定义的分区器
        /// </summary>
        public class StringPartitioner : Partitioner<string>
        {
            private readonly IEnumerable<string> _data;

            public StringPartitioner(IEnumerable<string> data)
            {
                _data = data;
            }

            /// <summary>
            /// 重写为false，声明只支持静态分区
            /// </summary>
            public override bool SupportsDynamicPartitions => false;

            /// <summary>
            /// 重载生成静态分区方法（长度为奇数和偶数的字符串，分别放在不同的分区）
            /// </summary>
            /// <param name="partitionCount"></param>
            /// <returns></returns>
            public override IList<IEnumerator<string>> GetPartitions(int partitionCount)
            {
                var result = new List<IEnumerator<string>>(partitionCount);

                for (int i = 1; i <= partitionCount; i++)
                {
                    result.Add(CreateEnumerator(i, partitionCount));
                }

                return result;
            }
            
            private IEnumerator<string> CreateEnumerator(int partitionNumber, int partitionCount)
            {
                int evenPartitions = partitionCount / 2;
                bool isEven = partitionNumber % 2 == 0;
                int step = isEven ? evenPartitions : partitionCount - evenPartitions;

                int startIndex = partitionNumber / 2 + partitionNumber % 2;

                var q = _data
                    .Where(v => !(v.Length % 2 == 0 ^ isEven) || partitionCount == 1)
                    .Skip(startIndex - 1);
                return q
                    .Where((x, i) => i % step == 0)
                    .GetEnumerator();
            }
        }
    }
}
