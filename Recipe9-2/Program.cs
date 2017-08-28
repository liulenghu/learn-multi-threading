using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Recipe9_2
{
    class Program
    {
        /// <summary>
        /// 编写一个异步的HTTP服务器和客户端
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var server = new AsyncHttpServer(1234);
            var t = Task.Run(() => server.Start());
            Console.WriteLine("Listening on port 1234. Opent http://localhost:1234 iin your browser.");
            Console.WriteLine("Trying to conect:");
            Console.WriteLine();

            GetResponseAsync("http://localhost:1234").GetAwaiter().GetResult();

            Console.WriteLine();
            Console.WriteLine("Press Enter to stop the server");
            Console.ReadLine();

            server.Stop().GetAwaiter().GetResult();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

        static async Task GetResponseAsync(string url)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                string responseHeaders = responseMessage.Headers.ToString();
                string response = await responseMessage.Content.ReadAsStringAsync();

                Console.WriteLine("Response headers:");
                Console.WriteLine(responseHeaders);
                Console.WriteLine("Response body:");
                Console.WriteLine(response);
            }
        }

        class AsyncHttpServer
        {
            // 使用HttpListener实现一个简单的服务器
            readonly HttpListener _listener;
            const string RESPONSE_TEMPLATE = "<html><head><title>Test</title></head><body><h2>Testpage</h2><h4>Today is: {0}</h4></body></html>";

            public AsyncHttpServer(int portNumber)
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://localhost:{portNumber}/");
            }
            
            public async Task Start()
            {
                // 启动服务器
                _listener.Start();
                while (true)
                {
                    // 调用GetContextAsync会发生异步I/O操作
                    var ctx = await _listener.GetContextAsync();
                    // 接收到请求时继续下面的处理
                    Console.WriteLine("Client connected...");
                    // 返回一个简单的HTML页面
                    var response = string.Format(RESPONSE_TEMPLATE, DateTime.Now);

                    using (var sw = new StreamWriter(ctx.Response.OutputStream))
                    {
                        await sw.WriteAsync(response);
                        await sw.FlushAsync();
                    }
                }
            }

            public async Task Stop()
            {
                // 调用Abort方法丢弃所有连接并关闭服务器
                _listener.Abort();
            }
        }
    }
}
