using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MQ.Models;
using RabbitMQ.Client.MessagePatterns;

namespace OrderService
{
    public class RabbitMQConsumer
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;        

        private const string ExchangeName = "Topic_Exchange";
        private const string PurchaseOrderQueueName = "PurchaseOrderTopic_Queue";

        public void CreateConnection()
        {
            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };            
        }

        public void Close()
        {
            _connection.Close();
        }

        public void ProcessMessages()
        {

			// RabbitMQ Server建立连接

			using (_connection = _factory.CreateConnection())
            {
                using (var channel = _connection.CreateModel())
                {
					Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
					Console.WriteLine("监听 Topic <payment.purchaseorder> 队列");
                    Console.WriteLine("------------------------------------------");
                    Console.WriteLine();
                    
                    channel.ExchangeDeclare(ExchangeName, "topic");

					//声明队列以向其发送消息消息

					channel.QueueDeclare(PurchaseOrderQueueName, true, false, false, null);
                    channel.QueueBind(PurchaseOrderQueueName, ExchangeName, "payment.*");

                    channel.BasicQos(0, 1, false);
                    Subscription subscription = new Subscription(channel, PurchaseOrderQueueName, false);
                    
                    while (true)
                    {
                        BasicDeliverEventArgs deliveryArguments = subscription.Next();
						var routingKey = deliveryArguments.RoutingKey;

						if (routingKey == "payment.purchaseorder")
						{
							var message = (PurchaseOrder)deliveryArguments.Body.DeSerialize(typeof(PurchaseOrder));
							Console.WriteLine("-- Purchase Order - Routing Key <{0}> : {1}, ￥{2}, {3}, {4}", routingKey, message.CompanyName, message.AmountToPay, message.PaymentDayTerms, message.PoNumber);
						}
						if (routingKey == "payment.cardpayment")
						{
							var message = (CardPayment)deliveryArguments.Body.DeSerialize(typeof(CardPayment));
							Console.WriteLine("-- Purchase Card - Routing Key <{0}> : {1}, {2}, ${3}", routingKey, message.Name, message.CardNumber, message.Amount);
						}

                        subscription.Ack(deliveryArguments);
                    }
                }
            }
        }
    }
}
