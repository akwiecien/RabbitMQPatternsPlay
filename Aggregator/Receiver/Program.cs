using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

internal class Program
{
    private static void Main(string[] args)
    {
        Dictionary<Guid, List<string>> Persisntance = new Dictionary<Guid, List<string>>();
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "aggregated-queue",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            //Get message
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                System.Console.WriteLine($"And final Sentence is: {message}");
            };
            // Acknowledge
            channel.BasicConsume(queue: "aggregated-queue",
                                autoAck: true,
                                consumer: consumer);

            //exit
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}