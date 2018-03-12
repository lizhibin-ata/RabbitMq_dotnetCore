using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using MQ.Models;

namespace SchoolClient
{
    public class ServiceClient: IDisposable
    {
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private Subscription _subscription;
        private IModel _model;
        private string _sendQueue;
        private string _replyQueueName;
        private readonly ILogger<ServiceClient> _logger;

        public ServiceClient(IConfigurationRoot configuration, ILogger<ServiceClient> logger)
        {
            _logger = logger;
            _connectionFactory = new ConnectionFactory
            {
                HostName = configuration.GetSection("rabbitmq-settings")["hostName"],
                UserName = configuration.GetSection("rabbitmq-settings")["userName"],
                Password = configuration.GetSection("rabbitmq-settings")["password"]
            };

            _connection = _connectionFactory.CreateConnection();
            _model = _connection.CreateModel();

            _sendQueue = configuration.GetSection("rabbitmq-settings")["sendQueue"];
            _model.QueueDeclare(_sendQueue, false, false, true, null); // 声明队列

            _replyQueueName = _model.QueueDeclare().QueueName;
            _subscription = new Subscription(_model, _replyQueueName, true);  // 订阅信息

			// exchange 选择默认 routhkey 跟queue名称一样

        }

        public IEnumerable<Student> GetStudents() => SendRequest<IEnumerable<Student>>("students");

        public IEnumerable<Course> GetCourses() => SendRequest<IEnumerable<Course>>("courses");

        private T SendRequest<T>(string message)
        {
            var corrId = Guid.NewGuid().ToString();

            var props = _model.CreateBasicProperties(); // 消息队列属性
            props.ReplyTo = _replyQueueName;
            props.CorrelationId = corrId;

            _logger.LogInformation($"回复队列: {_replyQueueName}\nCorrelation ID: {corrId}");

            var messageBytes = Encoding.UTF8.GetBytes(message);

            _logger.LogInformation($"发布消息: {message}");
            _model.BasicPublish("", _sendQueue, props, messageBytes);  // 发布队列信息

            while (true)
            {
                var delivery = _subscription.Next();
                if (delivery.BasicProperties.CorrelationId != corrId) continue;

                var resultString = Encoding.UTF8.GetString(delivery.Body);
                var result = JsonConvert.DeserializeObject<T>(resultString);
                return result;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            _logger.LogInformation("Disposing....");
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