using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using Model;

internal partial class Program
{
    private static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "content-enricher-final",
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
                var baseModel = JsonConvert.DeserializeObject<FinalModel>(message);
                Console.WriteLine(" [x] Received: {0}", message);
            };
            // Acknowledge
            channel.BasicConsume(queue: "content-enricher-final",
                                autoAck: true,
                                consumer: consumer);

            //exit
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}