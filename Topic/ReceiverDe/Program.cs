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
            channel.ExchangeDeclare(exchange: "topic",
                                    type: ExchangeType.Topic,
                                    durable: true,
                                    autoDelete: false,
                                    arguments: null);

            var queueName = "DeQueue";
            channel.QueueDeclare(queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false);


            channel.QueueBind(queue: queueName,
                              exchange: "topic",
                              routingKey: "*.*.De");


            //Get message
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received: {0}", message);

                int dots = message.Split('.').Length - 1;
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