using System.Text;
using Model;
using Newtonsoft.Json;
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
            channel.QueueDeclare(queue: "aggregate-queue",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

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
                var word = JsonConvert.DeserializeObject<Word>(message);
                if (word is null)
                    return;
                if (!Persisntance.Keys.Contains(word.CorrelationId))
                    Persisntance.Add(word.CorrelationId, new List<string>());
                Persisntance[word.CorrelationId].Add(word.Phrase);

                System.Console.WriteLine($"Aggregated Items: {Persisntance[word.CorrelationId].Count} {word.Phrase}");

                if (word.Conclude)
                {
                    channel.BasicPublish(exchange: "",
                                        routingKey: "aggregated-queue",
                                        basicProperties: null,
                                        body: Encoding.UTF8.GetBytes(string.Join(" ", Persisntance[word.CorrelationId].ToList())));
                }
            };
            // Acknowledge
            channel.BasicConsume(queue: "aggregate-queue",
                                autoAck: true,
                                consumer: consumer);

            //exit
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}