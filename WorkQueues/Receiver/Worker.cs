using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

internal class Worker
{
    private static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // Fair Dispatch - send message to first free receiver.                     
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            //Get message
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received: {0}", message);

                int dots = message.Split('.').Length -1;
                if (dots % 2 == 0)
                    dots = dots*10;
                Thread.Sleep(dots * 1000);
                Console.WriteLine(" [x] Done");

                //manual ack
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            // Acknowledge - manual
            channel.BasicConsume(queue: "hello",
                                 autoAck: false,
                                 consumer: consumer);

            // Acknowledge - auto
            // channel.BasicConsume(queue: "hello",
            //                      autoAck: true,
            //                      consumer: consumer);

            //exit
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}