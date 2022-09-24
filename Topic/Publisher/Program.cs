using System.Text;
using RabbitMQ.Client;

internal class Program
{
    private static void Main(string[] args)
    {
        // var dots = new List<string>{"",".","..","...","....","....."};
        var dots = new List<string>{"Pl.En.De","Pl.En.*","Pl.*.*"};

        for (int i = 0; i < 3; i++)
        {
            var message = $"Wiadomosc:  {dots[i]}";

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic",
                                        type: ExchangeType.Topic,
                                        durable: true,
                                        autoDelete: false,
                                        arguments: null);


                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "topic",
                                    routingKey: dots[i],
                                    basicProperties: null,
                                    body: body);

                Console.WriteLine($"Message has been send: {message}");
            }
        }

        Console.WriteLine(" Press any key to exit.");
        Console.ReadLine();
    }
}