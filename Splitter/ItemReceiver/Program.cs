using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

internal class Program
{
    private static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // define splitter-item - with seperated items
            channel.QueueDeclare(queue: "splitter-item",
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            //Get message
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received item: {0}", message);
            };
            // Acknowledge
            channel.BasicConsume(queue: "splitter-item",
                                autoAck: true,
                                consumer: consumer);

            //exit
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}