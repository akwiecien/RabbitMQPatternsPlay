using RabbitMQ.Client;
using System.Text;

internal partial class Program
{
    private static void Main(string[] args)
    {
        var dots = new List<string>{"",".","..","...","....","....."};
        for (int i = 1; i < 6; i++)
        {
            var message = $"Wiadomosc:  {dots[i]}";

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "router",
                                        type: ExchangeType.Direct,
                                        durable: true,
                                        autoDelete: false,
                                        arguments: null);


                var body = Encoding.UTF8.GetBytes(message);
                var isTrue =  i % 2 == 0;
                channel.BasicPublish(exchange: "router",
                                    routingKey: isTrue ? "even" : "odd",
                                    basicProperties: null,
                                    body: body);

                Console.WriteLine($"Message has been send: {message}");
            }
        }

        Console.WriteLine(" Press any key to exit.");
        Console.ReadLine();
    }
}