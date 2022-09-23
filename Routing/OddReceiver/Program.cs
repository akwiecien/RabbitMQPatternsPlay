using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

internal class Program
{
    private static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(exchange: "router",
                                    type: ExchangeType.Direct,
                                    durable: true,
                                    autoDelete: false,
                                    arguments: null);

            // this is queue with features: non-durable, exclusive, autodelete with generated name
            var queueName = "OddQueue";
            channel.QueueDeclare(queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false);


            channel.QueueBind(queue: queueName,
                              exchange: "router",
                              routingKey: "odd");


            //Get message
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received: {0}", message);

                int dots = message.Split('.').Length - 1;
                // if (dots % 2 == 0)
                //     dots = dots * 10;
                Thread.Sleep(dots * 1000);
                Console.WriteLine(" [x] Done");

                //manual ack
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            // Acknowledge - manual
            channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);

            //exit
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}