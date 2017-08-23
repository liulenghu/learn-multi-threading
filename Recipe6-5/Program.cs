using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Recipe6_5
{
    class Program
    {
        /// <summary>
        /// 使用BlockingCollection进行异步处理
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine($"Using a Queue inside of BlockingCollection");
            Console.WriteLine();
            // 默认使用ConcurrentQueue容器
            Task t = RunProgram();
            t.Wait();
            Console.WriteLine();
            Console.WriteLine("Using a Stack inside of BlockingCollection");
            Console.WriteLine();
            // 使用ConcurrentStack容器
            t = RunProgram(new ConcurrentStack<CustomTask>());
            t.Wait();

            Console.ReadLine();
        }

        static async Task RunProgram(IProducerConsumerCollection<CustomTask> collection = null)
        {
            // BlockingCollection类可以改变存储在阻塞集合中的方式
            var taskCollection = new BlockingCollection<CustomTask>();
            // 默认使用ConcurrentQueue容器
            // 如果指定容器，则使用指定的容器
            if (collection != null)
            {
                taskCollection = new BlockingCollection<CustomTask>(collection);
            }

            // 调用生产者创建任务
            var taskSource = Task.Run(() => TaskProducer(taskCollection));

            // 生成消费者，消费任务
            Task[] processors = new Task[4];
            for (int i = 1; i <= 4; i++)
            {
                string processorId = $"Processor {i}";
                processors[i - 1] = Task.Run(() => TaskProcessor(taskCollection, processorId));
            }

            // 等待任务创建完毕
            await taskSource;

            // 等待任务全部消费完
            await Task.WhenAll(processors);
        }

        /// <summary>
        /// 生产者
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        static async Task TaskProducer(BlockingCollection<CustomTask> collection)
        {
            for (int i = 1; i <= 20; i++)
            {
                await Task.Delay(20);
                var workItem = new CustomTask { Id = i };
                collection.Add(workItem);
                Console.WriteLine($"Task {workItem.Id} has been posted");
            }
            // 生产者调用CompleteAdding方法时，该迭代周期会结束
            collection.CompleteAdding();
        }

        /// <summary>
        /// 消费者
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        static async Task TaskProcessor(BlockingCollection<CustomTask> collection, string name)
        {
            await GetRandomDelay();
            // 使用GetConsumingEnumerable方法获取工作项
            foreach (CustomTask item in collection.GetConsumingEnumerable())
            {
                Console.WriteLine($"Task {item.Id} has been processed by {name}");
                await GetRandomDelay();
            }
        }

        static Task GetRandomDelay()
        {
            int delay = new Random(DateTime.Now.Millisecond).Next(1, 500);
            return Task.Delay(delay);
        }

        private class CustomTask
        {
            public int Id { get; set; }
        }
    }
}
