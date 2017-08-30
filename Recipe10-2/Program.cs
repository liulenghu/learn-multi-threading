using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe10_2
{
    class Program
    {
        private const int CollectionsNumber = 4;
        private const int Count = 5;

        /// <summary>
        /// 使用BlockingCollection实现并行管道
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            // 监视c键（按下c键取消执行）
            Task.Run(() =>
            {
                if (Console.ReadKey().KeyChar == 'c')
                {
                    cts.Cancel();
                }
            }, cts.Token);

            // 创建4个集合，每个集合中有5个元素
            var sourceArrays = new BlockingCollection<int>[CollectionsNumber];
            for (int i = 0; i < sourceArrays.Length; i++)
            {
                sourceArrays[i] = new BlockingCollection<int>(Count);
            }

            // 第一个管道：将int型数据转换成Decimal型
            var convertToDecimal = new PipelineWorker<int, decimal>(
                sourceArrays,
                n => Convert.ToDecimal(n * 100),
                cts.Token,
                "Decimal Converter"
                );

            // 第二个管道：格式化Decimal数据为金额字符串
            var stringifyNumber = new PipelineWorker<decimal, string>(
                convertToDecimal.Output,
                s => $"--{s.ToString("C", CultureInfo.GetCultureInfo("en-us"))}--",
                cts.Token,
                "Console Formatter"
                );

            // 第三个管道：打印格式化后的结果
            var outputResultToConsole = new PipelineWorker<string, string>(
                stringifyNumber.Output,
                s => Console.WriteLine($"The final result is {s} on thread id {Thread.CurrentThread.ManagedThreadId}"),
                cts.Token,
                "Console Output"
                );

            try
            {
                Parallel.Invoke(
                    // 初始化集合数据
                    () => CreateInitialValues(sourceArrays, cts),
                    // 将int型数据转换成Decimal型
                    () => convertToDecimal.Run(),
                    // 格式化Decimal数据为金额字符串
                    () => stringifyNumber.Run(),
                    // 打印格式化后的结果
                    () => outputResultToConsole.Run()
                    );
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    Console.WriteLine(ex.Message + ex.StackTrace);
                }
            }

            if (cts.Token.IsCancellationRequested)
            {
                Console.WriteLine("Operation has been canceled! Press ENTER to exit.");
            }
            else
            {
                Console.WriteLine("Press ENTER to exit.");
            }

            Console.ReadLine();
        }

        /// <summary>
        /// 初始化集合数据
        /// </summary>
        /// <param name="sourceArrays"></param>
        /// <param name="cts"></param>
        static void CreateInitialValues(BlockingCollection<int>[] sourceArrays, CancellationTokenSource cts)
        {
            Parallel.For(0, sourceArrays.Length * Count, (j, state) =>
            {
                if (cts.Token.IsCancellationRequested)
                {
                    state.Stop();
                }
                int number = GetRandomNumber(j);
                int k = BlockingCollection<int>.TryAddToAny(sourceArrays, j);
                if (k >= 0)
                {
                    Console.WriteLine($"added {j} to source data on thread id {Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep(TimeSpan.FromMilliseconds(number));
                }
            });

            foreach (var arr in sourceArrays)
            {
                arr.CompleteAdding();
            }
        }

        static int GetRandomNumber(int seed)
        {
            return new Random(seed).Next(500);
        }

        /// <summary>
        /// 管道类
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        class PipelineWorker<TInput, TOutput>
        {
            Func<TInput, TOutput> _processor;
            Action<TInput> _outputProcessor;
            BlockingCollection<TInput>[] _input;
            CancellationToken _token;
            Random _rnd;

            public BlockingCollection<TOutput>[] Output { get; private set; }

            public string Name { get; private set; }

            public PipelineWorker(
                BlockingCollection<TInput>[] input,
                Func<TInput, TOutput> processor,
                CancellationToken token,
                string name)
            {
                _input = input;
                Output = new BlockingCollection<TOutput>[_input.Length];
                for (int i = 0; i < Output.Length; i++)
                {
                    Output[i] = null == input[i] ? null : new BlockingCollection<TOutput>(Count);
                }

                _processor = processor;
                _token = token;
                Name = name;
                _rnd = new Random(DateTime.Now.Millisecond);
            }

            public PipelineWorker(
                BlockingCollection<TInput>[] input,
                Action<TInput> renderer,
                CancellationToken token,
                string name)
            {
                _input = input;
                _outputProcessor = renderer;
                _token = token;
                Name = name;
                Output = null;
                _rnd = new Random(DateTime.Now.Millisecond);
            }

            public void Run()
            {
                Console.WriteLine($"{Name} is running");
                while (!_input.All(bc => bc.IsCompleted) && !_token.IsCancellationRequested)
                {
                    TInput receivedItem;
                    // 尝试从集合中获取元素；如没有元素则会等待
                    int i = BlockingCollection<TInput>.TryTakeFromAny(_input, out receivedItem, 50, _token);
                    if (i >= 0)
                    {
                        if (Output != null)
                        {
                            TOutput outputItem = _processor(receivedItem);
                            BlockingCollection<TOutput>.AddToAny(Output, outputItem);
                            Console.WriteLine($"{Name} sent {outputItem} to next, on thread id {Thread.CurrentThread.ManagedThreadId}");
                            Thread.Sleep(TimeSpan.FromMilliseconds(_rnd.Next(200)));
                        }
                        else
                        {
                            _outputProcessor(receivedItem);
                        }
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(50));
                    }
                }

                if (Output != null)
                {
                    foreach (var bc in Output)
                    {
                        bc.CompleteAdding();
                    }
                }
            }
        }
    }
}
