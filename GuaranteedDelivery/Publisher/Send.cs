
using System;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;

for (int i = 0; i < 10; i++)
{
    Thread.Sleep(1000);
    var factory = new ConnectionFactory() { HostName = "localhost" };
    using (var connection = factory.CreateConnection())
    using (var channel = connection.CreateModel())
    {
        channel.QueueDeclare(queue: "guaranteed-delivery-queue",
                            durable: false,  // the queue and messages will survive a broker restart
                            exclusive: false,  //used only by one connection. will be delete when connection is closed
                            autoDelete: false,     // queue will be deleted if last consumer unsubscribe
                            arguments: null);      //additional arguments

        var message = new 
        {
            Id = Guid.NewGuid(),
            Content = "Message"
        };
        
        //before send we save message in database - if sender, broker or receiver fail our message will not be lost
        //_repository.Save(message);

        var serializedObject = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(serializedObject);
       

        channel.BasicPublish(exchange: "",
                            routingKey: "guaranteed-delivery-queue",
                            basicProperties: null,
                            body: body);

        System.Console.WriteLine($"Message has been send: {message}");
    }
}


Console.WriteLine(" Press any key to exit.");
Console.ReadLine();