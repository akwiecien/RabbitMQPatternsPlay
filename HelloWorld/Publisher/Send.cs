


using System;
using RabbitMQ.Client;
using System.Text;

for (int i = 0; i < 10; i++)
{
    Thread.Sleep(1000);
    var factory = new ConnectionFactory() { HostName = "localhost" };
    using (var connection = factory.CreateConnection())
    using (var channel = connection.CreateModel())
    {
        channel.QueueDeclare(queue: "hello",
                            durable: false,  // the queue and messages will survive a broker restart
                            exclusive: false,  //used only by one connection. will be delete when connection is closed
                            autoDelete: false,     // queue will be deleted if last consumer unsubscribe
                            arguments: null);      //additional arguments

        string message = $"Czołem z Warszawy !!! Item: {i}";
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
                            routingKey: "hello",
                            basicProperties: null,
                            body: body);

        System.Console.WriteLine($"Message has been send: {message}");
    }
}


Console.WriteLine(" Press any key to exit.");
Console.ReadLine();