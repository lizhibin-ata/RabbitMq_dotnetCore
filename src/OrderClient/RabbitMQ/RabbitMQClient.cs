using System;
using System.Collections.Generic;
using MQ.Models;
using RabbitMQ.Client;

namespace OrderClient
{
	public class RabbitMQClient : IDisposable
	{
		private static ConnectionFactory _factory;
		private static IConnection _connection;
		private static IModel _model;

		private const string ExchangeName = "Topic_Exchange";
		private const string CardPaymentQueueName = "CardPaymentTopic_Queue";
		private const string PurchaseOrderQueueName = "PurchaseOrderTopic_Queue";
		

		public RabbitMQClient()
		{
			CreateConnection();
		}

		private static void CreateConnection()
		{
			_factory = new ConnectionFactory
			{
				HostName = "localhost",
				UserName = "guest",
				Password = "guest"
			};

			_connection = _factory.CreateConnection();
			_model = _connection.CreateModel();
			_model.ExchangeDeclare(ExchangeName, "topic");  
			
			// topic 通配符类型 routhkey 
			// 可以放任意的key在routing_key中，当然最长不能超过255 bytes
			//*(星号)代表任意一个单词
			// #(hash)0个或多个单词

			/**
			* 配置一个队列的信息
			* 在RabbitMQ中，队列声明是幂等性的（一个幂等操作的特点是其任意多次执行所产生的影响均与一次执行的影响相同）
			* 也就是说，如果不存在，就创建，如果存在，不会对已经存在的队列产生任何影响。
			* @param queue 队列名称
			* @param durable 如果我们声明持久队列（队列将在服务器重新启动后生效），则为true
			* @param exclusive 如果我们声明排他队列（限于此连接），则为true
			* @param autoDelete 如果我们声明一个自动删除队列，则为true（服务器将在不再使用时将其删除）
			* @param arguments 队列的其他属性（构造参数）
			*/



			//_model.QueueDeclare(CardPaymentQueueName, true, false, false, null);
			_model.QueueDeclare(PurchaseOrderQueueName, true, false, false, null);
		
			// 多个key 绑定同一个队列
			_model.QueueBind(PurchaseOrderQueueName, ExchangeName, "payment.cardpayment");
			_model.QueueBind(PurchaseOrderQueueName, ExchangeName,
				"payment.purchaseorder");

		}

		public void Close()
		{
			_connection.Close();
		}



		public void SendPayment(CardPayment payment)
		{
			SendMessage(payment.Serialize(), "payment.cardpayment");
			Console.WriteLine(" Payment Sent {0},{1} ,${2}", payment.Name,payment.CardNumber,
				payment.Amount);
		}

		public void SendPurchaseOrder(PurchaseOrder purchaseOrder)
		{
			SendMessage(purchaseOrder.Serialize(), "payment.purchaseorder");

			Console.WriteLine(" Purchase Order Sent {0}, ￥{1}, {2}, {3}",
				purchaseOrder.CompanyName, purchaseOrder.AmountToPay,
				purchaseOrder.PaymentDayTerms, purchaseOrder.PoNumber);
		}

		public void SendMessage(byte[] message, string routingKey)
		{


			/**
			* 将消息放在队列中（字节数组形式）
			* @param exchange 交换发布消息
			* @param routingKey 路由密钥
			* @param props 消息的其他属性 - 路由头等
			* @param body 消息体
			*/

			_model.BasicPublish(ExchangeName, routingKey, null, message);
		}


		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_model.Dispose();
					_connection.Dispose();
				}

				disposedValue = true;
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
