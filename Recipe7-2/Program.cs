using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Recipe7_2
{
    class Program
    {
        /// <summary>
        /// PLINQ查询
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            // 正常的顺序LINQ查询
            // 所有操作都运行在当前线程
            var query = from t in GetTypes()
                        select EmulateProcessing(t);

            foreach (string typeName in query)
            {
                PrintInfo(typeName);
            }

            sw.Stop();
            Console.WriteLine("---");
            Console.WriteLine("Sequential LINQ query.");
            Console.WriteLine("正常的顺序LINQ查询");
            Console.WriteLine("所有操作都运行在当前线程");
            Console.WriteLine($"Time elapsed: {sw.Elapsed}");
            Console.WriteLine("Press Enter to continue....");
            Console.ReadLine();
            Console.Clear();
            sw.Reset();

            sw.Start();
            // 使用AsParallel方法将查询并行化
            // 默认情况下结果会被合并到单个线程中
            var paralleQuery = from t in GetTypes().AsParallel()
                               select EmulateProcessing(t);
            foreach (var typeName in paralleQuery)
            {
                PrintInfo(typeName);
            }
            sw.Stop();
            Console.WriteLine("---");
            Console.WriteLine("Parallel LINQ query. The results are being merged on a single thrad");
            Console.WriteLine("使用AsParallel方法将查询并行化");
            Console.WriteLine("默认情况下结果会被合并到单个线程中");
            Console.WriteLine($"Time elapsed: {sw.Elapsed}");
            Console.WriteLine("Press Enter to continue....");
            Console.ReadLine();
            Console.Clear();
            sw.Reset();


            sw.Start();
            // 使用AsParallel方法将查询并行化
            paralleQuery = from t in GetTypes().AsParallel()
                           select EmulateProcessing(t);
            // 使用ForAll方法将打印操作和查询操作放到了同一个线程，跳过了结果合并的步骤
            paralleQuery.ForAll(PrintInfo);

            sw.Stop();
            Console.WriteLine("---");
            Console.WriteLine("Parallel LINQ query. The results are being processed in parallel");
            Console.WriteLine("使用ForAll方法将打印操作和查询操作放到了同一个线程，跳过了结果合并的步骤");
            Console.WriteLine($"Time elapsed: {sw.Elapsed}");
            Console.WriteLine("Press Enter to continue....");
            Console.ReadLine();
            Console.Clear();
            sw.Reset();


            sw.Start();
            // 使用AsSequential方法将PLINQ查询已顺序方式执行
            query = from t in GetTypes().AsParallel().AsSequential()
                    select EmulateProcessing(t);
            foreach (string typeName in query)
            {
                PrintInfo(typeName);
            }

            sw.Stop();
            Console.WriteLine("---");
            Console.WriteLine("Parallel LINQ query, transformed into sequential.");
            Console.WriteLine("使用AsSequential方法将PLINQ查询以顺序方式执行");
            Console.WriteLine("运行结果同第一个示例完全一样");
            Console.WriteLine($"Time elapsed: {sw.Elapsed}");
            Console.WriteLine("Press Enter to continue....");
            Console.ReadLine();
            Console.Clear();
            sw.Reset();
        }

        static void PrintInfo(string typeName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(15));
            Console.WriteLine($"{typeName} type was printed on a thread id {Thread.CurrentThread.ManagedThreadId}");
        }

        static string EmulateProcessing(string typeName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(150));
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
                   select type.Name;
        }
    }
}
