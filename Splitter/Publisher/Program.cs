using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        Thread.Sleep(1000);
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "splitter-queue",
                                durable: true,  // the queue and messages will survive a broker restart
                                exclusive: false,  //used only by one connection. will be delete when connection is closed
                                autoDelete: false,     // queue will be deleted if last consumer unsubscribe
                                arguments: null);      //additional arguments

            var lista = new List<string>() { "Kacper", "Piotr", "Anna" };
            var payload = JsonConvert.SerializeObject(lista);

            channel.BasicPublish(exchange: "",
                                routingKey: "splitter-queue",
                                basicProperties: null,
                                body: Encoding.UTF8.GetBytes(payload));

            System.Console.WriteLine($"Message has been send: {lista.Count} objects");
        }
    }
}