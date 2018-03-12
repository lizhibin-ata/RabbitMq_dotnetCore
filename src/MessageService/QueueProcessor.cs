using System;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MQ.Models;


using RabbitMQ.Client.MessagePatterns;


namespace MessageService
{
	public class QueueProcessor
	{

		private ConnectionFactory _factory;
		private IConnection _connection;
		private const string ExchangeName = "Fanout_Exchange";
		private const string QueueName = "news";


		public QueueProcessor()
		{
			_factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
		}


		public void Start()
		{
			using (_connection = _factory.CreateConnection())
			{
				using (var channel = _connection.CreateModel())
				{
					Console.WriteLine("开始监听 fanout <news>");
					Console.WriteLine("------------------------------------------");
					Console.WriteLine();

					channel.ExchangeDeclare(ExchangeName, "fanout");
					//channel.QueueDeclare(QueueName, true, false, false, null);


					// 定义临时队列
					var queueName = channel.QueueDeclare().QueueName;

					// 绑定队列
					channel.QueueBind(queueName, ExchangeName,
					  "");

					channel.BasicQos(0, 1, false);
					Subscription subscription = new Subscription(channel, queueName, false);  // 定义消息订阅

					while (true)
					{
						BasicDeliverEventArgs deliveryArguments = subscription.Next();

						var message = (News)deliveryArguments.Body.DeSerialize(typeof(News));
						var routingKey = deliveryArguments.RoutingKey;

						Console.WriteLine("-- Send news - Routing Key <{0}> :titile: {1}, content:{2} time :{3}" , routingKey, message.title, message.content,DateTime.Now);

						Console.WriteLine("------------------------------------------");

						subscription.Ack(deliveryArguments);

					}

				}
			}

		}
	}
}