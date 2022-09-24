using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // define splitter-queue - with aggregated items
            channel.QueueDeclare(queue: "splitter-queue",
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

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
                var serializedObject = Encoding.UTF8.GetString(body);
                var lista = JsonConvert.DeserializeObject<List<string>>(serializedObject);
                if (lista is null)
                    return;
                Console.WriteLine(" [x] Received list with number of items: {0}", lista?.Count);
                foreach (var item in lista)
                {
                    var message = Encoding.UTF8.GetBytes(item);
                    channel.BasicPublish(exchange: "",
                                routingKey: "splitter-item",
                                basicProperties: null,
                                body: message);
                }
            };
            // Acknowledge
            channel.BasicConsume(queue: "splitter-queue",
                                autoAck: true,
                                consumer: consumer);

            //exit
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}