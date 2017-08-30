using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Recipe10_3
{
    class Program
    {
        /// <summary>
        /// 使用TPL数据流实现并行管道
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var t = ProcessAsynchronously();
            t.GetAwaiter().GetResult();
        }

        async static Task ProcessAsynchronously()
        {
            var cts = new CancellationTokenSource();
            Random _rnd = new Random(DateTime.Now.Millisecond);

            Task.Run(() =>
            {
                 if (Console.ReadKey().KeyChar == 'c')
                 {
                     cts.Cancel();
                 }
            }, cts.Token);

            // BufferBlock：将元素传给流中的下一个块
            // BoundedCapacity：指定其容量，超过时不再接受新元素，直到一个现有元素被传递给下一个块
            var inputBlock = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = 5, CancellationToken = cts.Token });

            // TransformBlock：用于数据转换步骤
            //   MaxDegreeOfParallelism：通过该选项指定最大工作者线程数
            // 将int转换为decimal
            var convertToDecimalBlock = new TransformBlock<int, decimal>(n =>
            {
                decimal result = Convert.ToDecimal(n * 100);
                Console.WriteLine($"Decimal Converter sent {result} to the next stage on {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(TimeSpan.FromMilliseconds(_rnd.Next(200)));
                return result;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token });

            // 将decimal转换为string
            var stringifyBlock = new TransformBlock<decimal, string>(n =>
            {
                string result = $"--{n.ToString("C", CultureInfo.GetCultureInfo("en-us"))}--";
                Console.WriteLine($"String Formatter sent {result} to the next stage on {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(TimeSpan.FromMilliseconds(_rnd.Next(200)));
                return result;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token });

            // ActionBlock:对每个传入的元素运行一个指定的操作
            var outputBlock = new ActionBlock<string>(s => {
                Console.WriteLine($"The final result is {s} on thread id {Thread.CurrentThread.ManagedThreadId}");
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token });

            // 通过LinkTo方法将这些块连接到一起
            // PropagateCompletion = true : 当前步骤完成时，自动将结果和异常传播到下一个阶段
            inputBlock.LinkTo(convertToDecimalBlock, new DataflowLinkOptions { PropagateCompletion = true });
            convertToDecimalBlock.LinkTo(stringifyBlock, new DataflowLinkOptions { PropagateCompletion = true });
            stringifyBlock.LinkTo(outputBlock, new DataflowLinkOptions { PropagateCompletion = true });

            try
            {
                // 向块中添加项
                Parallel.For(0, 20, new ParallelOptions {
                    MaxDegreeOfParallelism = 4, CancellationToken = cts.Token
                }, i => {
                    Console.WriteLine($"added {i} to source data on thread id {Thread.CurrentThread.ManagedThreadId}");
                    inputBlock.SendAsync(i).GetAwaiter().GetResult();
                });
                // 添加完成后需要调用Complete方法
                inputBlock.Complete();
                // 等待最后的块完成
                await outputBlock.Completion;
                Console.WriteLine("Press ENTER to exit.");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation has been canceled! Press ENTER to exit.");
            }

            Console.ReadLine();
        }
    }
}
