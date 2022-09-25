using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;

var factory = new ConnectionFactory() { HostName = "localhost" };
using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    channel.QueueDeclare(queue: "guaranteed-delivery-queue",
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);
    //Get message
    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var serializedMessage = Encoding.UTF8.GetString(body);
        var message = JsonConvert.DeserializeObject<object>(serializedMessage);

        //after job is finished we cen remove it or mark as finished in database
        // _repository.Delete(message.Id)
        
        Console.WriteLine(" [x] Received: {0}", message);
    };
    // Acknowledge
    channel.BasicConsume(queue: "guaranteed-delivery-queue",
                         autoAck: true,
                         consumer: consumer);

    //exit
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}