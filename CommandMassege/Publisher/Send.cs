using RabbitMQ.Client;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "command",
                                durable: false,  // the queue and messages will survive a broker restart
                                exclusive: false,  //used only by one connection. will be delete when connection is closed
                                autoDelete: false,     // queue will be deleted if last consumer unsubscribe
                                arguments: null);      //additional arguments

            // passing command via message
            string message = $"sql command";
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                routingKey: "command",
                                basicProperties: null,
                                body: body);

            System.Console.WriteLine($"Message has been send: {message}");
        }


        Console.WriteLine(" Press any key to exit.");
        Console.ReadLine();
    }
}