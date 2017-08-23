using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe6_3
{
    class Program
    {
        /// <summary>
        /// 改变ConcurrentStack异步处理顺序
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Task t = RunProgram();
            t.Wait();

            Console.ReadLine();
        }

        static async Task RunProgram()
        {
            var taskStack = new ConcurrentStack<CustomTask>();
            var cts = new CancellationTokenSource();
            // 异步创建任务
            var taskSource = Task.Run(() => TaskProducer(taskStack));

            // 创建4个任务处理线程
            Task[] processors = new Task[4];
            for (int i = 1; i <= 4; i++)
            {
                string processorId = i.ToString();
                processors[i - 1] = Task.Run(() => TaskProcessor(taskStack, $"Processor {processorId}", cts.Token));
            }

            // 等待创建任务结束
            await taskSource;
            // 延迟2秒发送取消指令，确保创建的任务被处理完
            cts.CancelAfter(TimeSpan.FromSeconds(2));
            // 等待所有消费结束
            await Task.WhenAll(processors);
        }

        /// <summary>
        /// 创建任务
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        static async Task TaskProducer(ConcurrentStack<CustomTask> queue)
        {
            for (int i = 1; i <= 20; i++)
            {
                await Task.Delay(50);
                // 创建任务加入队列
                var workItem = new CustomTask { Id = i };
                queue.Push(workItem);
                Console.WriteLine($"Task {workItem.Id} has been posted");
            }
        }

        /// <summary>
        /// 任务处理程序
        /// </summary>
        /// <param name="queue">队列</param>
        /// <param name="name">消费程序名</param>
        /// <param name="token">令牌（取消任务用）</param>
        /// <returns></returns>
        static async Task TaskProcessor(ConcurrentStack<CustomTask> queue, string name, CancellationToken token)
        {
            CustomTask workItem;
            bool dequeueSuccesful = false;

            // 若任务未取消，则延迟随机时间后尝试从队列中获取任务
            await GetRandomDelay();
            do
            {
                dequeueSuccesful = queue.TryPop(out workItem);
                if (dequeueSuccesful)
                {
                    Console.WriteLine($"Task {workItem.Id} has been processed by {name}");
                }
                await GetRandomDelay();
            } while (!token.IsCancellationRequested);

        }

        /// <summary>
        /// 获取随机的延迟时间
        /// </summary>
        /// <returns></returns>
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
