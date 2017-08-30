using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Recipe10_4
{
    class Program
    {
        /// <summary>
        ///  分隔符
        /// </summary>
        static char[] delimiters = { ' ', ',', ';', '\"', '.' };

        /// <summary>
        /// 使用PLINQ实现Map/Reduce模式
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 书籍列表
            var booksList = new Dictionary<string, string>()
            {
                ["Moby Dick; Or, The Whale by Herman Melville"] = "http://www.gutenberg.org/cache/epub/2701/pg2701.txt",
                ["The Adventures of Tom Sawyer by Mark Twain"] = "http://www.gutenberg.org/cache/epub/74/pg74.txt",
                ["Treasure Islan by Robert Louis Stevenson"] = "http://www.gutenberg.org/cache/epub/120/pg120.txt",
                ["The Picture of Dorian Gray by Oscar Wilde"] = "http://www.gutenberg.org/cache/epub/174/pg174.txt",
            };

            // 异步获取过滤词汇
            HashSet<string> stopwords = DownloadStopWordsAsync().GetAwaiter().GetResult();

            var output = new StringBuilder();
            
            // 并行处理书籍
            Parallel.ForEach(booksList.Keys, key => {
                // 异步下载书籍
                var bookContent = DownloadBookAsync(booksList[key]).GetAwaiter().GetResult();
                // 异步统计书籍
                string result = ProcessBookAsync(bookContent, key, stopwords).GetAwaiter().GetResult();
                // 打印结果
                output.Append(result);
                output.AppendLine();
            });

            Console.Write(output.ToString());
            Console.ReadLine();
        }

        async static Task<string> ProcessBookAsync(string bookContent, string title, HashSet<string> stopwords)
        {
            using (var reader = new StringReader(bookContent))
            {
                var query = reader.EnumLines() // 异步获取文件所有行
                    .AsParallel() // 并行化
                    .SelectMany(line => line.Split(delimiters)) // 对每一行分词
                    .MapReduce( // 调用自定义的MapReduce方法
                        word => new[] { word.ToLower() },
                        key => key,
                        g => new[] { new { Word = g.Key, Count = g.Count() } }
                    )
                    .ToList();

                // 过滤单词并根据统计数倒序排序
                var words = query
                    .Where(element => !string.IsNullOrEmpty(element.Word) && !stopwords.Contains(element.Word))
                    .OrderByDescending(element => element.Count);

                var sb = new StringBuilder();

                sb.AppendLine($"'{title}' book stats");
                sb.AppendLine($"Top ten words used in this book:");
                // 打印 TOP 10 单词
                foreach (var w in words.Take(10))
                {
                    sb.AppendLine($"Word: '{w.Word}', times used: '{w.Count}'");
                }

                sb.AppendLine($"Unique Words used: {query.Count()}");

                return sb.ToString();
            }
        }

        async static Task<string> DownloadBookAsync(string bookUrl)
        {
            using (var client = new HttpClient())
            {
                return await client.GetStringAsync(bookUrl);
            }
        }

        async static Task<HashSet<string>> DownloadStopWordsAsync()
        {
            string url = "https://raw.githubusercontent.com/6/stopwords/master/stopwords-all.json";

            using (var client = new HttpClient())
            {
                try
                {
                    var content = await client.GetStringAsync(url);
                    var words = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);
                    return new HashSet<string>(words["en"]);
                }
                catch
                {
                    return new HashSet<string>();
                }
            }
        }
    }

    /// <summary>
    /// 扩展方法类
    /// </summary>
    static class Extensions
    {
        /// <summary>
        /// 自定义的Map/Reduce扩展方法
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TMapped"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source">源</param>
        /// <param name="map">获取单个元素Func</param>
        /// <param name="keySelector">统计Func</param>
        /// <param name="reduce">查询结果Func</param>
        /// <returns></returns>
        public static ParallelQuery<TResult> MapReduce<TSource, TMapped, TKey, TResult>(
            this ParallelQuery<TSource> source,
            Func<TSource, IEnumerable<TMapped>> map,
            Func<TMapped, TKey> keySelector,
            Func<IGrouping<TKey, TMapped>, IEnumerable<TResult>> reduce
            )
        {
            return source
                .SelectMany(map)
                .GroupBy(keySelector)
                .SelectMany(reduce);
        }

        public static IEnumerable<string> EnumLines(this StringReader reader)
        {
            while (true)
            {
                string line = reader.ReadLine();
                if (null == line)
                {
                    yield break;
                }

                yield return line;
            }
        }
    }
}
