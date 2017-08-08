using System;
using System.Threading.Tasks;

namespace Recipe5_5
{
    class Program
    {
        /// <summary>
        /// 处理异步操作中的异常
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Task t = AsynchronousProcessing();
            t.Wait();

            Console.ReadLine();
        }

        static async Task AsynchronousProcessing()
        {
            Console.WriteLine("1. 单个异常");
            Console.WriteLine("   使用try/catch捕获单个异常");
            try
            {
                string result = await GetInfoAsync("任务 1", 2);
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception details: {ex}");
            }

            Console.WriteLine();
            Console.WriteLine("2. 多个异常");
            Console.WriteLine("   尝试使用try/catch捕获多个异常，但此种写法仅能捕获其中一个异常");
            
            Task<string> t1 = GetInfoAsync("任务 1", 3);
            Task<string> t2 = GetInfoAsync("任务 2", 2);
            try
            {
                string[] results = await Task.WhenAll(t1, t2);
                Console.WriteLine(results.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception details: {ex}");
            }

            Console.WriteLine();
            Console.WriteLine("3. 使用AggregateException捕获多个异常");
            Console.WriteLine("   从task的Exception属性中获取多个异常的信息");
            
            t1 = GetInfoAsync("任务 1", 3);
            t2 = GetInfoAsync("任务 2", 2);
            Task<string[]> t3 = Task.WhenAll(t1, t2);
            try
            {
                string[] results = await t3;
                Console.WriteLine(results.Length);
            }
            catch
            {
                var ae = t3.Exception.Flatten();
                var exceptions = ae.InnerExceptions;
                Console.WriteLine($"Exceptions caght: {exceptions.Count}");
                foreach (var e in exceptions)
                {
                    Console.WriteLine($"Exception details: {e}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine("4. 在catch和finally块中使用await");
            Console.WriteLine("   c#6.0中，允许在catch和finally块中使用await，在之前的版本中这种写法会报错");
            Console.WriteLine("   若将语言版本改为C#5.0编译，则会报如下错误：");
            Console.WriteLine("     CS1985 无法在 catch 子句中等待");
            Console.WriteLine("     CS1984  无法在 finally 子句体中等待");
            
            try
            {
                string result = await GetInfoAsync("任务 1", 2);
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                Console.WriteLine($"Catch block with await: Exception details: {ex}");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                Console.Write("Finally block");
            }
        }

        static async Task<string> GetInfoAsync(string name, int seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            throw new Exception($"Boom from {name}");
        }
    }
}
