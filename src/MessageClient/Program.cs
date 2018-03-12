using System;
using MQ.Models;
using System.Diagnostics;

namespace MessageClient
{
    class Program
    {
		private static RabbitMQClient _apiClient;
		static void Main(string[] args)
		{
			Console.WriteLine($"客户端启动成功，{DateTime.Now}。");
			_apiClient = new RabbitMQClient();

			using (_apiClient)
			{
				try
				{

					News news = new News { title="test", content = "123455" };

					do
					{
						var watch = Stopwatch.StartNew();
						_apiClient.SendNews(news);
						watch.Stop();
						Console.WriteLine($"执行时间：{watch.ElapsedMilliseconds}ms");
						Console.WriteLine("Press any key to continue, q to exit the loop...");
						var key = Console.ReadLine();
						if (key.ToLower() == "q")
							break;
					} while (true);


					
				}
				catch (Exception ex)
				{


				}

				Console.ReadLine();
			}


		}
	}
}
