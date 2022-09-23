using RabbitMQ.Client;
using System.Text;

internal class Receiver1
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
                //we declare exchange to force Fanout approvach - in contrast to WorkQueues approvach when we didn't declare exchange
                channel.ExchangeDeclare(exchange: "logs",
                                        type: ExchangeType.Fanout,
                                        durable: true,
                                        autoDelete: false,
                                        arguments: null);

                //if one receiver not ack then 

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "logs",
                                    routingKey: "",
                                    basicProperties: null,
                                    body: body);

                Console.WriteLine($"Message has been send: {message}");
            }
        }

        Console.WriteLine(" Press any key to exit.");
        Console.ReadLine();
    }
}