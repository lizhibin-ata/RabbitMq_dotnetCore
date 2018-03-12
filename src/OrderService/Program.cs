using System;

namespace OrderService
{
    class Program
    {
        static void Main(string[] args)
        {
			RabbitMQConsumer client = new RabbitMQConsumer();
			client.CreateConnection();

			Console.WriteLine("启动 order 服务队列 Processor....");

			client.ProcessMessages();
			

			Console.ReadLine();

			client.Close();
		}
    }
}
