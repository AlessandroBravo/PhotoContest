using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Text;

namespace TSAC.Bravo.PhotoContest.Queue
{
    public class QueueAccess : IQueueAccess
    {
        private readonly IConfiguration _config;

        public QueueAccess(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void SendToQueue(string message)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = _config["RabbitMQ:HostName"], UserName = _config["RabbitMQ:UserName"], Password = _config["RabbitMQ:Password"] };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "task_queue",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: "",
                                         routingKey: "task_queue",
                                         basicProperties: properties,
                                         body: body);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("rabbitmq error" + e);
            }
        }
    }
}
