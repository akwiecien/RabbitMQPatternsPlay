using System.Text;
using Model;
using RabbitMQ.Client;
using Newtonsoft.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        var correlationId = Guid.NewGuid();

        var dots = new List<string> { "Dead", "Letter", "Exchange" };
        for (int i = 0; i < 3; i++)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "aggregate-queue",
                                    durable: false,  // the queue and messages will survive a broker restart
                                    exclusive: false,  //used only by one connection. will be delete when connection is closed
                                    autoDelete: false,     // queue will be deleted if last consumer unsubscribe
                                    arguments: null);      //additional arguments

                // We CorrelationId property to give aggregator key for aggregation
                var word = new Word();
                word.CorrelationId = correlationId;
                word.Phrase = dots[i];
                // this flat is for aggregator to let know if last item was send
                word.Conclude = i == dots.Count-1;

                var serializedWord = JsonConvert.SerializeObject(word);

                var body = Encoding.UTF8.GetBytes(serializedWord);

                // this is required to persist messages
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                    routingKey: "aggregate-queue",
                                    basicProperties: null,
                                    body: body);

                Console.WriteLine($"Message has been send: {dots[i]}");
                Console.ReadLine();
            }
        }

        Console.WriteLine(" Press any key to exit.");
        Console.ReadLine();
    }
}