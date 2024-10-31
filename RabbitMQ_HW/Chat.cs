using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ_HW
{
    class Chat
    {
        private static string _username;
        private const string ExchangeName = "chat_exchange";

        public static void Main(string[] args)
        {
            Console.Write("Enter your username: ");
            _username = Console.ReadLine();
            var receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            SendMessages();
        }

        private static void SendMessages()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout);

                Console.WriteLine("Start chatting! Type your messages below:");

                while (true)
                {
                    var message = Console.ReadLine();
                    var fullMessage = $"{_username}: {message}";
                    var body = Encoding.UTF8.GetBytes(fullMessage);

                    channel.BasicPublish(exchange: ExchangeName, routingKey: "", basicProperties: null, body: body);
                }
            }
        }

        private static void ReceiveMessages()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName, exchange: ExchangeName, routingKey: "");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    if (!message.StartsWith(_username + ":"))
                    {
                        Console.WriteLine(message);
                    }
                };

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

  
                while (true) Thread.Sleep(100);
            }
        }
    }
}
