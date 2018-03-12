using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SchoolClient
{
    public class Program
    {
        private static IConfigurationRoot _configuration;
        private static ServiceClient _apiClient;
        public static void Main(string[] args)
        {
            LoadConfig();

            var logger = new LoggerFactory().AddConsole().CreateLogger<ServiceClient>();
            _apiClient = new ServiceClient(_configuration, logger);

            using (_apiClient)
            {
                try
                {
					// 获取学生列表
                    ListStudents();

					// 获取课程列表
                    ListCourses();
                }
                catch (Exception)
                {
                    logger.LogError("请求不到资源！");
                }
                Console.ReadLine();
            }
        }

        private static void LoadConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
        }

        private static void ListStudents()
        {
            var students = _apiClient.GetStudents();
            Console.WriteLine($"Student Count: {students.Count()}");
        }

        private static void ListCourses()
        {
            var courses = _apiClient.GetCourses();
            Console.WriteLine($"Course Count: {courses.Count()}");
        }
    }
}
