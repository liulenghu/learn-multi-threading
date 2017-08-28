using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recipe9_1
{
    class Program
    {
        /// <summary>
        /// 异步地使用文件
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var t = ProcessAsynchronousIO();
            t.GetAwaiter().GetResult();

            Console.ReadLine();
        }

        const int BUFFER_SIZE = 4096;

        static async Task ProcessAsynchronousIO()
        {
            // 使用FileStream创建文件
            using (var stream = new FileStream("test1.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.None, BUFFER_SIZE))
            {
                Console.WriteLine($"1. Uses I/O Threads: {stream.IsAsync}");

                byte[] buffer = Encoding.UTF8.GetBytes(CreateFileContent());
                // 将异步编程模型API转换成任务
                var writeTask = Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, 0, buffer.Length, null);
                await writeTask;
            }

            // 使用FileStream创建文件（提供了FileOptions.Asynchronous参数）
            // 只有提供了提供了FileOptions.Asynchronous选项，才能对FileStream类使用异步IO
            using (var stream = new FileStream("test2.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.None, BUFFER_SIZE, FileOptions.Asynchronous))
            {
                Console.WriteLine($"2. Uses I/O Threads: {stream.IsAsync}");

                byte[] buffer = Encoding.UTF8.GetBytes(CreateFileContent());
                // 将异步编程模型API转换成任务
                var writeTask = Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, 0, buffer.Length, null);
                await writeTask;
            }

            // 使用File.Create（提供了FileOptions.Asynchronous参数）和StreamWriter创建和写入文件
            using (var stream = File.Create("test3.txt", BUFFER_SIZE, FileOptions.Asynchronous))
            using (var sw = new StreamWriter(stream))
            {
                Console.WriteLine($"3. Uses I/O Threads: {stream.IsAsync}");
                // 异步写入流
                await sw.WriteAsync(CreateFileContent());
            }

            // 仅使用StreamWriter创建文件
            using (var sw = new StreamWriter("test4.txt", true))
            {
                Console.WriteLine($"4. Uses I/O Threads: {((FileStream)sw.BaseStream).IsAsync}");

                // 异步写入流（因为没有提供FileOptions.Asynchronous参数，Stream其实并没有使用异步I/O）
                await sw.WriteAsync(CreateFileContent());
            }

            Console.WriteLine("Starting parsing files in parallel");
            var readTasks = new Task<long>[4];
            for (int i = 0; i < 4; i++)
            {
                string fileName = $"test{i + 1}.txt";
                // 异步读取文件并Sum
                readTasks[i] = SumFileContent(fileName);
            }
            // 等待所有异步Task完成，并获取返回值数组
            long[] sums = await Task.WhenAll(readTasks);
            Console.WriteLine($"Sum is all files: {sums.Sum()}");

            Console.WriteLine("Deleting files");
            Task[] deleteTasks = new Task[4];
            for (int i = 0; i < 4; i++)
            {
                string filename = $"test{i + 1}.txt";
                // 异步删除文件
                deleteTasks[i] = SimulateAsynchronousDelete(filename);
            }
            // 等待所有异步删除操作结束
            await Task.WhenAll(deleteTasks);
            Console.WriteLine("Deleting complete.");
        }

        /// <summary>
        /// 异步删除文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        static Task SimulateAsynchronousDelete(string filename)
        {
            // 使用Task.Run模拟异步删除
            return Task.Run(() => File.Delete(filename));
        }

        /// <summary>
        /// 异步统计文件中随机数的合计值
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static async Task<long> SumFileContent(string fileName)
        {
            // 使用FileStream和StreamReader异步读取文件
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, BUFFER_SIZE, FileOptions.Asynchronous))
            using (var sr = new StreamReader(stream))
            {
                long sum = 0;
                while (sr.Peek() > -1)
                {
                    string line = await sr.ReadLineAsync();
                    sum += long.Parse(line);
                }
                return sum;
            }
        }

        /// <summary>
        /// 创建随机的文件内容
        /// </summary>
        /// <returns></returns>
        static string CreateFileContent()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 100000; i++)
            {
                sb.Append($"{ new Random(DateTime.Now.Millisecond).Next(0, 99999)}");
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
