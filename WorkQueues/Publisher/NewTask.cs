using RabbitMQ.Client;
using System.Text;

internal class NewTask
{
    private static void Main(string[] args)
    {
        var dots = new List<string>{"",".","..","...","....","....."};
        for (int i = 1; i < 6; i++)
        {
            var message = $"Wiadomosc:  {dots[i]}";

            // Thread.Sleep(1000);
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                    durable: false,  // the queue and messages will survive a broker restart
                                    exclusive: false,  //used only by one connection. will be delete when connection is closed
                                    autoDelete: false,     // queue will be deleted if last consumer unsubscribe
                                    arguments: null);      //additional arguments

                var body = Encoding.UTF8.GetBytes(message);

                // this is required to persist messages
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                    routingKey: "hello",
                                    basicProperties: null,
                                    body: body);

                Console.WriteLine($"Message has been send: {message}");
            }
        }

        Console.WriteLine(" Press any key to exit.");
        Console.ReadLine();
    }
}