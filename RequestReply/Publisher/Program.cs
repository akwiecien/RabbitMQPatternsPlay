using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

internal class Program
{
    private static void Main(string[] args)
    {
        var dots = new List<string> { "Kacper", "Piotr", "Ania" };

        for (int i = 0; i < 3; i++)
        {
            var message = $"{dots[i]}";

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //prepare for response receiving
                //we can use anonymous queue to handle response. 
                var replyQueueName = channel.QueueDeclare().QueueName;
                var consumer = new EventingBasicConsumer(channel);
                var correlationId = Guid.NewGuid().ToString();

                var props = channel.CreateBasicProperties();
                props.CorrelationId = correlationId;
                props.ReplyTo = replyQueueName;

                //create consumer for response receiving:
                consumer.Received += (model, ea) =>
                {
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        var body = ea.Body.ToArray();
                        var response = Encoding.UTF8.GetString(body);
                        Console.WriteLine($"Response has returned: {response}");
                    }
                };
                channel.BasicConsume(queue: replyQueueName,
                                     autoAck: true,
                                     consumer: consumer);

                //and now send message
                var queueName = "UpperCaseReplayQueue";
                channel.QueueDeclare(queue: queueName,
                            durable: true,
                            exclusive: false,
                            autoDelete: false);

                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "",
                                    routingKey: queueName,
                                    basicProperties: props,
                                    body: body);

                Console.WriteLine($"Message has been send: {message}");
                Console.ReadLine();
            }
        }

    }
}