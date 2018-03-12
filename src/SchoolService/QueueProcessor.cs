using System;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SchoolService.Infrastructure;

namespace SchoolService
{
    public class QueueProcessor
    {
        private readonly QueueConfig _config;
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _model;
        private DataStore _dataStore;

        public QueueProcessor(QueueConfig config)
        {
            _config = config;

            _connectionFactory = new ConnectionFactory
            {
                HostName = config.HostName,
                UserName = config.UserName,
                Password = config.Password
            };

            _connection = _connectionFactory.CreateConnection("QueueProcessor Connection"); // 建立rabbitMQ 消息队列连接
            _model = _connection.CreateModel(); 

            _model.BasicQos(0, 1, false); //这样RabbitMQ就会使得每个Consumer在同一个时间点最多处理一个Message。换句话说，在接收到该Consumer的ack前，他它不会将新的Message分发给它。
			                              //注意，这种方法可能会导致queue满。当然，这种情况下你可能需要添加更多的Consumer，或者创建更多的virtualHost来细化你的设计。

			_model.QueueDeclare(config.QueueName, false, false, true, null); // 声明队列

            _dataStore = new DataStore();
        }

        public void Start()
        {
            var consumer = new EventingBasicConsumer(_model);

            consumer.Received += (model, ea) =>
            {

				// 获取消息队列基本属性
                var props = ea.BasicProperties;
                var replyProps = _model.CreateBasicProperties();

                var body = ea.Body;
                replyProps.CorrelationId = props.CorrelationId;

                var message = Encoding.UTF8.GetString(body);

                var result = string.Empty;

                Console.WriteLine("*** 处理请求 ***");
                Console.WriteLine($"*** Process ID {Process.GetCurrentProcess().Id} ***");
                switch (message)
                {
                    case "students":
                        Console.WriteLine("接收学生信息");
                        result = JsonConvert.SerializeObject(_dataStore.Students);
                        break;
                    case "courses":
                        Console.WriteLine("接收课程信息");
                        result = JsonConvert.SerializeObject(_dataStore.Courses);
                        break;
                    default:
                        Console.WriteLine($"没有处理信息: {message}");
                        break;
                }

                var resultBytes = Encoding.UTF8.GetBytes(result);

				// 回复消息队列 routhkey props.ReplyTo
				_model.BasicPublish("", props.ReplyTo, replyProps, resultBytes);



               //当我们需要确认一条消息已经被消费时，我们调用的 basicAck 方法的第一个参数是 Delivery Tag。
               //Delivery Tag 用来标识信道中投递的消息。RabbitMQ 推送消息给 Consumer 时，会附带一个 Delivery Tag，以便 Consumer 可以在消息确认时告诉 RabbitMQ 到底是哪条消息被确认了。
               //RabbitMQ 保证在每个信道中，每条消息的 Delivery Tag 从 1 开始递增。
                _model.BasicAck(ea.DeliveryTag, false);


            };

            _model.BasicConsume(_config.QueueName, false, consumer); //   需要接受方发送ack回执,删除消息

			Console.WriteLine("请按 [enter] 退出.");
            Console.ReadLine();
            
            _model.Dispose();
            _connection.Dispose();
        }
    }
}