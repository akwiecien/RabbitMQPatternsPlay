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
            channel.QueueDeclare(queue: "content-enricher-middle",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            channel.QueueDeclare(queue: "content-enricher-final",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
            //Get message
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                // get first version of model - only name
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var baseModel = JsonConvert.DeserializeObject<BaseModel>(message);
                Console.WriteLine(" [x] Received: {0}", message);

                // enrich model content with LastName
                var finalModel = new FinalModel() {
                    FirstName = baseModel.Name,
                    LastName = "Kwiecień"
                };

                // and send it to final receiver
                var serializedFinalModel = JsonConvert.SerializeObject(finalModel);
                var payload = Encoding.UTF8.GetBytes(serializedFinalModel);
                channel.BasicPublish(exchange: "",
                                routingKey: "content-enricher-final",
                                basicProperties: null,
                                body: payload);
                
                Console.WriteLine(" [x] Enriched: {0}", serializedFinalModel);
            };
            // Acknowledge
            channel.BasicConsume(queue: "content-enricher-middle",
                                autoAck: true,
                                consumer: consumer);

            //exit
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}