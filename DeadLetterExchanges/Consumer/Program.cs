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
            //we need to create dead letter queue exchange and queue and bind both
            channel.ExchangeDeclare(exchange: "dead-letter-reject-exchange",
                                ExchangeType.Fanout,
                                durable: true,
                                autoDelete: false);

            channel.QueueDeclare(queue: "dead-letter-reject-queue",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: new Dictionary<string, object>(){
                                    {"x-queque-mode", "lazy"}
                                });

            channel.QueueBind(queue: "dead-letter-reject-queue",
                              exchange: "dead-letter-reject-exchange",
                              routingKey: "",
                              null);

            // the queue that will not ack (reject) must have arguments
            channel.QueueDeclare(queue: "reject-queue",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: new Dictionary<string, object>(){
                                    {"x-queque-mode", "lazy"},
                                    {"x-dead-letter-exchange", "dead-letter-reject-exchange"}
                                 });

            //Get message
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received: {0}", message);
                if (message == "true") 
                {
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    Console.WriteLine(" [x] Successfully finished. We acknowledge message");
                }
                else
                {
                    // we can reject by BasicNack or BasicReject - BasicNack have bulk options - BasicReject can reject only single message
                    channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, false);
                    // channel.BasicReject(deliveryTag: ea.DeliveryTag, false);
                    Console.WriteLine(" [x] But exception happened. We reject message to broker");
                }
            };
            // Acknowledge
            channel.BasicConsume(queue: "reject-queue",
                                 autoAck: false,
                                 consumer: consumer);

            //exit
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}