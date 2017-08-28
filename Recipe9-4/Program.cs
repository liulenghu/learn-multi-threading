using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;

namespace Recipe9_4
{
    class Program
    {
        /// <summary>
        /// 异步调用WCF服务
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ServiceHost host = null;

            try
            {
                // 创建一个的主机（服务类型为HelloWorldService；承载服务的地址为http://localhost:1234/HelloWorld）
                host = new ServiceHost(typeof(HelloWorldService), new Uri(SERVICE_URL));
                var metadata = host.Description.Behaviors.Find<ServiceMetadataBehavior>() ?? new ServiceMetadataBehavior();
                // 允许通过HTTPGET获取元数据
                // （即可以通过浏览器访问http://localhost:1234/HelloWorld?wsdl或http://localhost:1234/HelloWorld?singleWsdl获取xml格式的元数据）
                metadata.HttpGetEnabled = true;
                // 指定正在使用的 WS-Policy 规范的版本。
                metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(metadata);

                host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

                var endpoint = host.AddServiceEndpoint(typeof(IHelloWorldService), new BasicHttpBinding(), SERVICE_URL);

                host.Faulted += (sender, e) => Console.WriteLine("Error!");

                // 开启主机
                host.Open();

                Console.WriteLine($"Greeting service is running and listening on: {endpoint.Address} ({endpoint.Binding.Name})");

                var client = RunServiceClient();
                client.GetAwaiter().GetResult();
                
                Console.WriteLine("Press Enter to exit");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in catch block: {ex}");
            }
            finally
            {
                if (null != host)
                {
                    if (host.State == CommunicationState.Faulted)
                    {
                        host.Abort();
                    }
                    else
                    {
                        host.Close();
                    }
                }
            }
        }

        const string SERVICE_URL = "http://localhost:1234/HelloWorld";

        /// <summary>
        /// 模拟远程访问WCF服务
        /// </summary>
        /// <returns></returns>
        static async Task RunServiceClient()
        {
            var endpoint = new EndpointAddress(SERVICE_URL);
            // 创建服务通道
            var channel = ChannelFactory<IHelloWorldServiceClient>.CreateChannel(new BasicHttpBinding(), endpoint);

            // 通过通道异步访问Greet服务
            var greeting = await channel.GreetAsync("Eugene");
            Console.WriteLine(greeting);
        }

        /// <summary>
        /// 服务端接口
        /// </summary>
        [ServiceContract(Namespace = "Packt", Name = "HelloWorldServiceContract")]
        public interface IHelloWorldService
        {
            [OperationContract]
            string Greet(string name);
        }

        /// <summary>
        /// 客户端服务接口
        /// </summary>
        [ServiceContract(Namespace = "Packt", Name = "HelloWorldServiceContract")]
        public interface IHelloWorldServiceClient
        {
            [OperationContract]
            string Greet(string name);

            [OperationContract]
            Task<string> GreetAsync(string name);
        }

        /// <summary>
        /// 服务端实现
        /// </summary>
        public class HelloWorldService : IHelloWorldService
        {
            public string Greet(string name)
            {
                return $"Greetings, {name}";
            }
        }
    }
}
