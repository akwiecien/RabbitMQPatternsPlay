using RabbitMQ.Client;
using System.Text;
using Model;
using Newtonsoft.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "content-enricher-middle",
                                durable: false,  // the queue and messages will survive a broker restart
                                exclusive: false,  //used only by one connection. will be delete when connection is closed
                                autoDelete: false,     // queue will be deleted if last consumer unsubscribe
                                arguments: null);      //additional arguments

            // sending first version of model - only name
            var baseModel = new BaseModel() { Name = "Andrzej" };
            var serializedBaseModel = JsonConvert.SerializeObject(baseModel);
            var body = Encoding.UTF8.GetBytes(serializedBaseModel);

            channel.BasicPublish(exchange: "",
                                routingKey: "content-enricher-middle",
                                basicProperties: null,
                                body: body);
            System.Console.WriteLine($"Message has been send: {serializedBaseModel}");
        }

        System.Console.ReadLine();
    }
}