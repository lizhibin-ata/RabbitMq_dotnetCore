using System;

namespace MessageService
{
    class Program
    {
        static void Main(string[] args)
        {
			Console.WriteLine("启动 Message 服务队列 Processor....");

			QueueProcessor queueProcessor = new QueueProcessor();
			queueProcessor.Start();

			Console.WriteLine();
		}
    }
}
