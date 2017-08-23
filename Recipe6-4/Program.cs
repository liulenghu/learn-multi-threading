using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Recipe6_4
{
    class Program
    {
        static Dictionary<string, string[]> _contentEmulation = new Dictionary<string, string[]>();

        /// <summary>
        /// 使用ConcurrentBag创建一个可扩展的爬虫
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            CreateLinks();
            Task t = RunProgram();
            t.Wait();

            Console.ReadLine();
        }

        /// <summary>
        /// 创建模拟用的链接数据
        /// </summary>
        private static void CreateLinks()
        {
            _contentEmulation["http://liujiajia.me"] = new[] { "http://liujiajia.me/#/blog/it", "http://liujiajia.me/#/blog/game" };

            _contentEmulation["http://liujiajia.me/#/blog/it"] = new[] {
                "http://liujiajia.me/#/blog/details/csharp-multi-threading-06-concurrent-00-summary",
                "http://liujiajia.me/#/blog/details/cookie-http-only" };

            _contentEmulation["http://liujiajia.me/#/blog/game"] = new[] {
                "http://liujiajia.me/#/blog/details/wow-7-3-ptr",
                "http://liujiajia.me/#/blog/details/63b737b6-7663-43f6-acd2-dc6e020c14ba" };
        }

        static async Task RunProgram()
        {
            var bag = new ConcurrentBag<CrawlingTask>();
            // 定义4个网站根url地址，并创建4个对应的Task
            string[] urls =
            {
                "http://liujiajia.me",
                "http://weibo.com",
                "http://sf.gg",
                "http://ngacn.cc"
            };

            var crawlers = new Task[4];
            for (int i = 1; i <= 4; i++)
            {
                string crawlerName = $"Crawler {i}";
                bag.Add(new CrawlingTask { UrlToCrawl = urls[i-1], ProducerName = "root" });
                crawlers[i - 1] = Task.Run(() => Crawl(bag, crawlerName));
            }

            await Task.WhenAll(crawlers);
        }

        /// <summary>
        /// 模拟爬虫程序
        /// </summary>
        /// <param name="bag"></param>
        /// <param name="crawlerName"></param>
        /// <returns></returns>
        static async Task Crawl(ConcurrentBag<CrawlingTask> bag, string crawlerName)
        {
            CrawlingTask task;
            while (bag.TryTake(out task))
            {
                // 如果页面中存在URL地址，则将这些地址放入待爬取的任务集合
                IEnumerable<string> urls = await GetLinksFromContent(task);
                if (urls != null)
                {
                    foreach (var url in urls)
                    {
                        var t = new CrawlingTask
                        {
                            UrlToCrawl = url,
                            ProducerName = crawlerName
                        };

                        bag.Add(t);
                    }

                    Console.WriteLine($"Indexing url {task.UrlToCrawl} posted by {task.ProducerName} is completed by {crawlerName}");
                }
            }
        }

        /// <summary>
        /// 获取页面上的URL地址
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        static async Task<IEnumerable<string>> GetLinksFromContent(CrawlingTask task)
        {
            await GetRandomDelay();

            if (_contentEmulation.ContainsKey(task.UrlToCrawl))
            {
                return _contentEmulation[task.UrlToCrawl];
            }

            return null;
        }

        static Task GetRandomDelay()
        {
            int delay = new Random(DateTime.Now.Millisecond).Next(150, 200);
            return Task.Delay(delay);
        }

        private class CrawlingTask
        {
            public string UrlToCrawl { get; set; }
            public string ProducerName { get; set; }
        }
    }
}
