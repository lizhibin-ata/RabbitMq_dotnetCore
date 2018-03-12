using System;
using MQ.Models;
using System.Diagnostics;

namespace OrderClient
{
    class Program
    {

		private static RabbitMQClient _apiClient;
		static void Main(string[] args)
        {

			_apiClient = new RabbitMQClient();

			using (_apiClient)
			{
				try
				{
					Console.WriteLine($"客户端启动成功，{DateTime.Now}。");
					Console.WriteLine("*** topic 队列发布  ***");


					Console.WriteLine("Press p key to 发送 purchaseOrder 信息 , q to exit the loop...");
					Console.WriteLine("Press c key to 发送 purchaseCard 信息 , q to exit the loop...");
					do
					{
						
						var key = Console.ReadLine();
						if (key.ToLower() == "p")
						{
							var watch = Stopwatch.StartNew();
							PurchaseOrder purchaseOrder = new PurchaseOrder { AmountToPay = 1, CompanyName = "test", PaymentDayTerms = 10, PoNumber = "123" };
							_apiClient.SendPurchaseOrder(purchaseOrder);
						}
						if (key.ToLower() == "c")
						{
							var watch = Stopwatch.StartNew();
							CardPayment cardPayment = new CardPayment { Amount=1, CardNumber="12131", Name= "lzb" };
							_apiClient.SendPayment(cardPayment);
						}

						if (key.ToLower() == "q")
							break;

					} while (true);

					
				}
				catch (Exception ex)
				{
					Console.WriteLine(" ERROR : " + ex.Message);

				}

				Console.ReadLine();
			}

				
        }
    }
}
