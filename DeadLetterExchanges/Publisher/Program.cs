using System.Text;
using RabbitMQ.Client;

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

            foreach (var item in new List<string>(){"true", "false"})
            {
                string message = $"{item}";
                var body = Encoding.UTF8.GetBytes(message);
                var props = channel.CreateBasicProperties();
                props.Persistent = true;
                props.DeliveryMode = 2;
                channel.ConfirmSelect();


                channel.BasicPublish(exchange: "",
                                    routingKey: "reject-queue",
                                    basicProperties: props,
                                    body: body);

                System.Console.WriteLine($"Message has been send: {message}");
                
            }
            Console.WriteLine(" Press any key to exit.");
            Console.ReadLine();
        }


    }
}