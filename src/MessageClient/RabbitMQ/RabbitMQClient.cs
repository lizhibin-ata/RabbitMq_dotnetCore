using System;
using System.Collections.Generic;
using MQ.Models;
using RabbitMQ.Client;

namespace MessageClient
{
    public class RabbitMQClient : IDisposable
	{
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _model;

        private const string ExchangeName = "Fanout_Exchange";
        private const string QueueName = "news";
        

        public RabbitMQClient()
        {
            CreateConnection();
        }

        private static void CreateConnection()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost", UserName = "guest", Password = "guest"
            };

            _connection = _factory.CreateConnection();
            _model = _connection.CreateModel();

		
			_model.ExchangeDeclare(ExchangeName, "fanout");   // fanout 扇形 

			//_model.QueueDeclare(QueueName, true, false, false, null);

			//_model.QueueBind(QueueName, ExchangeName,
			//  "");

		}

        public void Close()
        {
            _connection.Close();
        }

        public void SendNews(News news)
        {
            SendMessage(news.Serialize());

			Console.WriteLine(" news Sent {0}, {1}", news.title,
			news.content);

		}

      
        public void SendMessage(byte[] message)
        {
			// routhkey 为空
            _model.BasicPublish(ExchangeName,"", null, message);
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
