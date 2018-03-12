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

            _connection = _connectionFactory.CreateConnection("QueueProcessor Connection"); // ����rabbitMQ ��Ϣ��������
            _model = _connection.CreateModel(); 

            _model.BasicQos(0, 1, false); //����RabbitMQ�ͻ�ʹ��ÿ��Consumer��ͬһ��ʱ�����ദ��һ��Message�����仰˵���ڽ��յ���Consumer��ackǰ���������Ὣ�µ�Message�ַ�������
			                              //ע�⣬���ַ������ܻᵼ��queue������Ȼ������������������Ҫ��Ӹ����Consumer�����ߴ��������virtualHost��ϸ�������ơ�

			_model.QueueDeclare(config.QueueName, false, false, true, null); // ��������

            _dataStore = new DataStore();
        }

        public void Start()
        {
            var consumer = new EventingBasicConsumer(_model);

            consumer.Received += (model, ea) =>
            {

				// ��ȡ��Ϣ���л�������
                var props = ea.BasicProperties;
                var replyProps = _model.CreateBasicProperties();

                var body = ea.Body;
                replyProps.CorrelationId = props.CorrelationId;

                var message = Encoding.UTF8.GetString(body);

                var result = string.Empty;

                Console.WriteLine("*** �������� ***");
                Console.WriteLine($"*** Process ID {Process.GetCurrentProcess().Id} ***");
                switch (message)
                {
                    case "students":
                        Console.WriteLine("����ѧ����Ϣ");
                        result = JsonConvert.SerializeObject(_dataStore.Students);
                        break;
                    case "courses":
                        Console.WriteLine("���տγ���Ϣ");
                        result = JsonConvert.SerializeObject(_dataStore.Courses);
                        break;
                    default:
                        Console.WriteLine($"û�д�����Ϣ: {message}");
                        break;
                }

                var resultBytes = Encoding.UTF8.GetBytes(result);

				// �ظ���Ϣ���� routhkey props.ReplyTo
				_model.BasicPublish("", props.ReplyTo, replyProps, resultBytes);



               //��������Ҫȷ��һ����Ϣ�Ѿ�������ʱ�����ǵ��õ� basicAck �����ĵ�һ�������� Delivery Tag��
               //Delivery Tag ������ʶ�ŵ���Ͷ�ݵ���Ϣ��RabbitMQ ������Ϣ�� Consumer ʱ���ḽ��һ�� Delivery Tag���Ա� Consumer ��������Ϣȷ��ʱ���� RabbitMQ ������������Ϣ��ȷ���ˡ�
               //RabbitMQ ��֤��ÿ���ŵ��У�ÿ����Ϣ�� Delivery Tag �� 1 ��ʼ������
                _model.BasicAck(ea.DeliveryTag, false);


            };

            _model.BasicConsume(_config.QueueName, false, consumer); //   ��Ҫ���ܷ�����ack��ִ,ɾ����Ϣ

			Console.WriteLine("�밴 [enter] �˳�.");
            Console.ReadLine();
            
            _model.Dispose();
            _connection.Dispose();
        }
    }
}