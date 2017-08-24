using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Recipe7_3
{
    class Program
    {
        /// <summary>
        /// 调整PLINQ查询的参数
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var parallelQuery = from t in GetTypes().AsParallel()
                                select EmulateProcessing(t);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            try
            {
                parallelQuery
                    // 设置要在查询中使用的并行度。并行度是将用于处理查询的同时执行的任务的最大数目。
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    // 设置查询的执行模式
                    // （ForceParallelism：并行化整个查询，即使要使用系统开销大的算法。）
                    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                    // 设置此查询的合并选项，它指定查询对输出进行缓冲处理的方式。
                    // (Default:使用默认合并类型，即 AutoBuffered。)
                    // (AutoBuffered:利用系统选定大小的输出缓冲区进行合并。在向查询使用者提供结果之前，会先将结果累计到输出缓冲区中。)
                    .WithMergeOptions(ParallelMergeOptions.Default)
                    // 设置要与查询关联的 System.Threading.CancellationToken。
                    .WithCancellation(cts.Token)
                    .ForAll(Console.WriteLine);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("---");
                Console.WriteLine("操作已被取消");
            }

            Console.WriteLine("---");
            Console.WriteLine("执行未排序的PLINQ查询");

            var unorderedQuery = from i in ParallelEnumerable.Range(1, 30)
                                 select i;

            foreach (var i in unorderedQuery)
            {
                Console.WriteLine(i);
            }

            Console.WriteLine("---");
            Console.WriteLine("执行已排序的PLINQ查询");
            var orderedQuery = from i in ParallelEnumerable.Range(1, 30).AsOrdered()
                               select i;

            foreach (var i in orderedQuery)
            {
                Console.WriteLine(i);
            }

            Console.ReadLine();
        }

        static string EmulateProcessing(string typeName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(new Random(DateTime.Now.Millisecond).Next(250,350)));
            Console.WriteLine($"{typeName} type was processed on a thread id {Thread.CurrentThread.ManagedThreadId}");
            return typeName;
        }

        /// <summary>
        /// 使用反射API查询加载到当前应用程序域中的所有组件中名称以“Web”开头的类型
        /// </summary>
        /// <returns></returns>
        static IEnumerable<string> GetTypes()
        {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                   from type in assembly.GetExportedTypes()
                   where type.Name.StartsWith("Web")
                   orderby type.Name.Length
                   select type.Name;
        }
    }
}
